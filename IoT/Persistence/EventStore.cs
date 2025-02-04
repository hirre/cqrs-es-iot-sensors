using IoT.Persistence.Events;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace IoT.Persistence
{
    public class EventStore
    {
        private readonly ILogger<EventStore> _logger;
        private readonly IConfiguration _config;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<DomainEvent> _eventCollection;

        public EventStore(ILogger<EventStore> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            var mongoClient = new MongoClient(_config.GetSection("MongoDb:ConnectionString").Value);
            _mongoDatabase = mongoClient.GetDatabase(_config.GetSection("MongoDb:DatabaseName").Value);

            if (_mongoDatabase == null)
            {
                _logger.LogError("Database not found");
                throw new Exception("Database not found");
            }

            // Register the payload types
            BsonClassMap.RegisterClassMap<SensorPayload>();

            _mongoDatabase.CreateCollection(_config.GetSection("MongoDb:EventStoreCollectionName").Value);

            _eventCollection = _mongoDatabase.GetCollection<DomainEvent>(_config.GetSection("MongoDb:EventStoreCollectionName").Value);
            var eventIndexKeys = Builders<DomainEvent>.IndexKeys.Ascending("Timestamp").Ascending(x => x.Version);
            _eventCollection.Indexes.CreateOne(new CreateIndexModel<DomainEvent>(eventIndexKeys));
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

        public async Task<IAsyncCursor<DomainEvent>> GetEventsAsync()
        {
            return await _eventCollection.Find(Builders<DomainEvent>.Filter.Empty)
                                               .Sort(Builders<DomainEvent>.Sort.Ascending("Timestamp")
                                               .Ascending(x => x.Version))
                                               .ToCursorAsync();
        }
    }
}
