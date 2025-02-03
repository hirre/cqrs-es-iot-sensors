using IoT.Domain.Sensor.Events;
using MongoDB.Driver;

namespace IoT.Persistence
{
    public class SensorDbContext
    {
        private readonly ILogger<SensorDbContext> _logger;
        private readonly IConfiguration _config;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<SensorDbData> _sensorDataCollection;
        private readonly IMongoCollection<SensorEvent> _sensorEventCollection;

        public SensorDbContext(ILogger<SensorDbContext> logger, IConfiguration config)
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

            _mongoDatabase.CreateCollection(_config.GetSection("MongoDb:SensorCollectionName").Value);
            _mongoDatabase.CreateCollection(_config.GetSection("MongoDb:EventStoreCollectionName").Value);

            _sensorDataCollection = _mongoDatabase.GetCollection<SensorDbData>(_config.GetSection("MongoDb:SensorCollectionName").Value);
            var sensorDataIndexKeys = Builders<SensorDbData>.IndexKeys.Ascending(x => x.Timestamp);
            _sensorDataCollection.Indexes.CreateOne(new CreateIndexModel<SensorDbData>(sensorDataIndexKeys));

            _sensorEventCollection = _mongoDatabase.GetCollection<SensorEvent>(_config.GetSection("MongoDb:EventStoreCollectionName").Value);
            var eventIndexKeys = Builders<SensorEvent>.IndexKeys.Ascending("Timestamp").Ascending(x => x.Version);
            _sensorEventCollection.Indexes.CreateOne(new CreateIndexModel<SensorEvent>(eventIndexKeys));

        }

        public async Task InsertSensorDataDocumentAsync(SensorDbData document, SensorEvent sEvent)
        {
            await _sensorDataCollection.InsertOneAsync(document);  // Insert sensor data

            if (document.Id != null)
                sEvent.SensorDbDataId = document.Id;

            await AppendEventAsync(sEvent);  // Insert event
        }

        public async Task<IAsyncCursor<SensorDbData>> GetSensorDataDocumentsAsync(string sensorId,
            DateTimeOffset latestReadTimestamp)
        {
            return await _sensorDataCollection.Find(x => x.SensorId == sensorId && x.Timestamp > latestReadTimestamp)
                                              .Sort(Builders<SensorDbData>.Sort.Ascending(x => x.Timestamp))
                                              .ToCursorAsync();
        }

        public async Task<IAsyncCursor<SensorEvent>> GetSensorEventsAsync()
        {
            return await _sensorEventCollection.Find(Builders<SensorEvent>.Filter.Empty)
                                               .Sort(Builders<SensorEvent>.Sort.Ascending("Timestamp").Ascending(x => x.Version))
                                               .ToCursorAsync();
        }

        private async Task AppendEventAsync(SensorEvent newEvent)
        {
            // Find the latest version of this aggregate's events
            var lastEvent = await _sensorEventCollection
                .Find(Builders<SensorEvent>.Filter.Eq(x => x.AggregateId, newEvent.AggregateId))
                .Sort(Builders<SensorEvent>.Sort.Descending(x => x.Version))
                .Limit(1)
                .FirstOrDefaultAsync();

            int newVersion = lastEvent == null ? 1 : lastEvent.Version + 1;

            newEvent.Version = newVersion;

            await _sensorEventCollection.InsertOneAsync(newEvent);
        }
    }
}
