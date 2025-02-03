using IoT.Extensions;
using Microsoft.Extensions.Caching.Distributed;

namespace IoT.Infrastructure
{
    public class ReadModelEventWorker(ILogger<ReadModelEventWorker> logger, IDistributedCache distributedCache) : BackgroundService
    {
        private readonly ILogger<ReadModelEventWorker> _logger = logger;
        private readonly IDistributedCache _distributedCache = distributedCache;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReadModelEventWorker running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await _distributedCache.SetAsync<object>("sa", new object());


                // Do work
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("ReadModelEventWorker stopping.");
        }
    }
}
