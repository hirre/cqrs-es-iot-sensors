using IoT.Persistence.Events;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace IoT.Persistence
{
    public class EventStore
    {
        private const string EVENTSTORE_CONNECTIONSTRING_KEY = "MongoDb:ConnectionString";
        private const string EVENTSTORE_DBNAME_KEY = "MongoDb:DatabaseName";
        private const string EVENTSTORE_NAME_KEY = "MongoDb:EventStoreCollectionName";
        private const string EVENTSTORE_SNAPSHOT_NAME = "Snapshots";

        private readonly ILogger<EventStore> _logger;
        private readonly IConfiguration _config;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<DomainEvent> _eventCollection;
        private readonly IMongoCollection<Snapshot> _snapshotCollection;

        public EventStore(ILogger<EventStore> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            var mongoClient = new MongoClient(_config.GetSection(EVENTSTORE_CONNECTIONSTRING_KEY).Value);
            _mongoDatabase = mongoClient.GetDatabase(_config.GetSection(EVENTSTORE_DBNAME_KEY).Value);

            if (_mongoDatabase == null)
            {
                _logger.LogError("Database not found");
                throw new Exception("Database not found");
            }

            // Register the payload types
            BsonClassMap.RegisterClassMap<SensorPayload>();

            _mongoDatabase.CreateCollection(_config.GetSection(EVENTSTORE_NAME_KEY).Value);
            _mongoDatabase.CreateCollection(EVENTSTORE_SNAPSHOT_NAME);

            _eventCollection = _mongoDatabase.GetCollection<DomainEvent>(_config.GetSection(EVENTSTORE_NAME_KEY).Value);
            var eventIndexKeys = Builders<DomainEvent>.IndexKeys.Ascending("Timestamp").Ascending(x => x.Version);
            _eventCollection.Indexes.CreateOne(new CreateIndexModel<DomainEvent>(eventIndexKeys));

            _snapshotCollection = _mongoDatabase.GetCollection<Snapshot>(EVENTSTORE_SNAPSHOT_NAME);
            var snapshotIndexKeys = Builders<Snapshot>.IndexKeys
                .Ascending(x => x.AggregateId)
                .Descending(x => x.Version);
            var snapshotUniqueKeys = Builders<Snapshot>.IndexKeys.Combine(
                Builders<Snapshot>.IndexKeys.Ascending(x => x.AggregateId),
                Builders<Snapshot>.IndexKeys.Descending(x => x.Version));

            _snapshotCollection.Indexes.CreateMany(
            [
                new CreateIndexModel<Snapshot>(snapshotIndexKeys, new CreateIndexOptions { Name = "SnapshotIndex" }),
                new CreateIndexModel<Snapshot>(snapshotUniqueKeys, new CreateIndexOptions { Unique = true, Name = "SnapshotUniqueIndex" })
            ]);
        }

        public async Task AppendEventAsync(DomainEvent document)
        {
            ArgumentNullException.ThrowIfNull(document);

            // Find the latest version of this aggregate's events
            var lastEvent = await _eventCollection
                .Find(Builders<DomainEvent>.Filter.Eq(x => x.AggregateId, document.AggregateId))
                .Sort(Builders<DomainEvent>.Sort.Descending(x => x.Version))
                .Limit(1)
                .FirstOrDefaultAsync();

            document.Version = lastEvent == null ? 1 : lastEvent.Version + 1;

            await _eventCollection.InsertOneAsync(document);
        }

        public async Task<IAsyncCursor<DomainEvent>> GetEventsAsync(string aggregateId, int startVersion)
        {
            return await _eventCollection.Find(Builders<DomainEvent>.Filter
                .And(
                        Builders<DomainEvent>.Filter.Eq(x => x.AggregateId, aggregateId),
                        Builders<DomainEvent>.Filter.Gte(x => x.Version, startVersion)
                    ))
                .Sort(Builders<DomainEvent>.Sort
                .Ascending(x => x.Version))
                .ToCursorAsync();
        }

        public async Task StoreSnapShotAsync(string aggregateId, int version, byte[] data)
        {
            var snap = await GetSnapshotAsync(aggregateId, version);

            if (snap != null)
                return;

            var snapshot = new Snapshot()
            {
                AggregateId = aggregateId,
                Version = version,
                Timestamp = DateTimeOffset.UtcNow,
                Data = data
            };

            await _snapshotCollection.InsertOneAsync(snapshot);
        }

        public async Task<Snapshot> GetSnapshotAsync(string aggregateId, int version)
        {
            var filter = Builders<Snapshot>.Filter
                            .And(
                                    Builders<Snapshot>.Filter.Eq(x => x.AggregateId, aggregateId),
                                    Builders<Snapshot>.Filter.Eq(x => x.Version, version)
                                );

            return await _snapshotCollection.Find(filter)
                                             .Sort(Builders<Snapshot>.Sort.Descending(x => x.Version))
                                             .Limit(1)
                                             .FirstOrDefaultAsync();
        }

        public async Task<Snapshot> GetLatestSnapshotAsync(string aggregateId)
        {
            var filter = Builders<Snapshot>.Filter.Eq(x => x.AggregateId, aggregateId);

            return await _snapshotCollection.Find(filter)
                                             .Sort(Builders<Snapshot>.Sort.Descending(x => x.Version))
                                             .Limit(1)
                                             .FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetUniqueAggregateIdsAsync()
        {
            var aggregateIds = await _eventCollection.DistinctAsync<string>("AggregateId", Builders<DomainEvent>.Filter.Empty);
            return await aggregateIds.ToListAsync();
        }

    }
}
