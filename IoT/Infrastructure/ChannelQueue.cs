using IoT.Interfaces;
using System.Threading.Channels;

namespace IoT.Infrastructure
{
    public class ChannelQueue<T> where T : ISensorEvent
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

        public async Task<T> WaitAndReadAsync(CancellationToken cancellationToken)
        {
            while (await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                if (_channel.Reader.TryRead(out var item))
                {
                    return item;
                }
            }

            throw new OperationCanceledException("The operation was canceled.");
        }
    }
}
