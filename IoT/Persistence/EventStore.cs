using IoT.Common;
using IoT.Persistence.Events;
using MongoDB.Bson;
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
        private readonly MongoClient _mongoClient;
        private readonly IMongoCollection<DomainEvent> _eventCollection;
        private readonly IMongoCollection<Snapshot> _snapshotCollection;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public EventStore(ILogger<EventStore> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            _mongoClient = new MongoClient(_config.GetSection(EVENTSTORE_CONNECTIONSTRING_KEY).Value);
            _mongoDatabase = _mongoClient.GetDatabase(_config.GetSection(EVENTSTORE_DBNAME_KEY).Value);

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
                .Ascending(x => x.EntityType)
                .Ascending(x => x.EntityId)
                .Descending(x => x.Version);
            var snapshotUniqueKeys = Builders<Snapshot>.IndexKeys.Combine(
                Builders<Snapshot>.IndexKeys.Ascending(x => x.EntityType),
                Builders<Snapshot>.IndexKeys.Ascending(x => x.EntityId),
                Builders<Snapshot>.IndexKeys.Descending(x => x.Version));

            _snapshotCollection.Indexes.CreateMany(
            [
                new CreateIndexModel<Snapshot>(snapshotIndexKeys, new CreateIndexOptions { Name = "SnapshotIndex" }),
                new CreateIndexModel<Snapshot>(snapshotUniqueKeys,
                new CreateIndexOptions { Unique = true, Name = "SnapshotUniqueIndex" })
            ]);
        }

        public async Task AppendEventAsync(DomainEvent document)
        {
            ArgumentNullException.ThrowIfNull(document);

            using var session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                _semaphore.Wait(5000);

                // Find the latest version of this entity's events
                var lastEvent = await _eventCollection
                    .Find(Builders<DomainEvent>.Filter.Eq(x => x.EntityId, document.EntityId))
                    .Sort(Builders<DomainEvent>.Sort.Descending(x => x.Version))
                    .Limit(1)
                    .FirstOrDefaultAsync();

                document.Version = lastEvent == null ? 1 : lastEvent.Version + 1;

                await _eventCollection.InsertOneAsync(document);

                await session.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while appending event.");
                await session.AbortTransactionAsync();
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IAsyncCursor<DomainEvent>> GetEventsAsync(EventTypes evType, string entityId, int startVersion)
        {
            return await _eventCollection.Find(Builders<DomainEvent>.Filter
                .And(
                Builders<DomainEvent>.Filter.Eq(x => x.EventType, evType),
                        Builders<DomainEvent>.Filter.Eq(x => x.EntityId, entityId),
                        Builders<DomainEvent>.Filter.Gte(x => x.Version, startVersion)
                    ))
                .Sort(Builders<DomainEvent>.Sort
                .Ascending(x => x.Version))
                .ToCursorAsync();
        }

        public async Task StoreSnapShotAsync(string eventType, string id, int version, byte[] data)
        {
            using var session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                _semaphore.Wait(5000);

                var snap = await GetSnapshotAsync(eventType, id, version);

                if (snap != null)
                    return;

                var snapshot = new Snapshot()
                {
                    EntityId = id,
                    EntityType = eventType,
                    Version = version,
                    Timestamp = DateTimeOffset.UtcNow,
                    Data = data
                };

                await _snapshotCollection.InsertOneAsync(snapshot);

                await session.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while storing a snapshot.");
                await session.AbortTransactionAsync();
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Snapshot> GetSnapshotAsync(string entityType, string entityId, int version)
        {
            var filter = Builders<Snapshot>.Filter
                            .And(
                                    Builders<Snapshot>.Filter.Eq(x => x.EntityType, entityType),
                                    Builders<Snapshot>.Filter.Eq(x => x.EntityId, entityId),
                                    Builders<Snapshot>.Filter.Eq(x => x.Version, version)
                                );

            return await _snapshotCollection.Find(filter)
                                             .Sort(Builders<Snapshot>.Sort.Descending(x => x.Version))
                                             .Limit(1)
                                             .FirstOrDefaultAsync();
        }

        public async Task<Snapshot> GetLatestSnapshotAsync(string entityType, string entityId)
        {
            var filter = Builders<Snapshot>.Filter
                .And(
                    Builders<Snapshot>.Filter.Eq(x => x.EntityType, entityType),
                    Builders<Snapshot>.Filter.Eq(x => x.EntityId, entityId)
                );

            return await _snapshotCollection.Find(filter)
                                             .Sort(Builders<Snapshot>.Sort.Descending(x => x.Version))
                                             .Limit(1)
                                             .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<(EventTypes EvType, string Id)>> GetUniqueEntityIdsAsync()
        {
            var uniqueEntities = await _eventCollection.Aggregate()
                .Group(new BsonDocument { { "_id", new BsonDocument { { "EventType", "$EventType" },
                    { "EntityId", "$EntityId" } } } })
                .ToListAsync();

            var result = new List<(EventTypes EvType, string EntityId)>();

            foreach (var uniqueEntity in uniqueEntities)
            {
                var eventTypeString = uniqueEntity["_id"]["EventType"]?.ToString();
                var entityId = uniqueEntity["_id"]["EntityId"].ToString();

                if (string.IsNullOrEmpty(eventTypeString) || string.IsNullOrEmpty(entityId))
                {
                    continue;
                }

                var eventType = Enum.Parse<EventTypes>(eventTypeString);

                result.Add((eventType, entityId));
            }

            return result;
        }
    }
}
