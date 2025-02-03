using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace IoT.Infrastructure
{
    public class ChannelQueue<T> where T : class
    {
        private readonly Channel<T> _channel;

        public ChannelQueue()
        {
            var options = new BoundedChannelOptions(10000) // Buffer up to 100 messages
            {
                SingleWriter = false, // Multiple producers
                SingleReader = true   // Single consumer
            };

            _channel = Channel.CreateBounded<T>(options);
        }

        public async Task PublishAsync(T item)
        {
            await _channel.Writer.WriteAsync(item);
        }

        public async IAsyncEnumerable<T> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await _channel.Reader.WaitToReadAsync(cancellationToken); // Blocks until data is available

            await foreach (var item in _channel.Reader.ReadAllAsync(cancellationToken)) // Blocks until data is available
            {
                yield return item;
            }

            throw new OperationCanceledException("The operation was canceled.");
        }
    }
}
