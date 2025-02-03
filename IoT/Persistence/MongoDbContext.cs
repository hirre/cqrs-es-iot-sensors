using IoT.Domain.Sensor.Repository;
using MongoDB.Driver;

namespace IoT.Persistence
{
    public class MongoDbContext
    {
        private readonly ILogger<MongoDbContext> _logger;
        private readonly IConfiguration _config;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<SensorData> _sensorDataCollection;

        public MongoDbContext(ILogger<MongoDbContext> logger, IConfiguration config)
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

            _sensorDataCollection = _mongoDatabase.GetCollection<SensorData>(_config.GetSection("MongoDb:SensorCollectionName").Value);
        }

        public async Task InsertSensorDataDocumentAsync(SensorData document)
        {
            await _sensorDataCollection.InsertOneAsync(document);  // Insert a single document
        }

        public async Task InsertSensorDataDocumentsAsync(IEnumerable<SensorData> documents)
        {
            await _sensorDataCollection.InsertManyAsync(documents);  // Insert multiple documents
        }

        public async Task<IAsyncCursor<SensorData>> GetSensorDataDocumentsAsync(string sensorId, DateTimeOffset startTime, DateTimeOffset stopTime, bool ascending = true)
        {
            if (string.IsNullOrEmpty(sensorId))
            {
                throw new ArgumentNullException(nameof(sensorId));
            }

            if (startTime > stopTime)
            {
                throw new ArgumentException("startTime must be less than stopTime");
            }

            return await _sensorDataCollection.Find(x => x.SensorId == sensorId && x.Timestamp >= startTime && x.Timestamp <= stopTime)
                                              .Sort(Builders<SensorData>.Sort.Descending(x => x.Timestamp))
                                              .ToCursorAsync();
        }
    }
}
