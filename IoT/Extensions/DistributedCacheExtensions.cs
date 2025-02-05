using Microsoft.Extensions.Caching.Distributed;

namespace IoT.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static Task SetDataAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions? options = null)
        {
            var bytes = MessagePack.MessagePackSerializer.Serialize(value);

            if (options != null)
            {
                return cache.SetAsync(key, bytes, options);
            }
            else
            {
                return cache.SetAsync(key, bytes);
            }
        }

        public static bool TryGetDataValue<T>(this IDistributedCache cache, string key, out T? value)
        {
            var val = cache.Get(key);
            value = default;

            if (val == null) return false;

            value = MessagePack.MessagePackSerializer.Deserialize<T>(val);

            return true;
        }

        public static async Task<T?> GetOrSetDataAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> task, DistributedCacheEntryOptions? options = null)
        {
            if (cache.TryGetDataValue(key, out T? value) && value is not null)
            {
                return value;
            }

            value = await task();

            if (value is not null)
            {
                await cache.SetDataAsync(key, value, options);
            }

            return value;
        }
    }
}
