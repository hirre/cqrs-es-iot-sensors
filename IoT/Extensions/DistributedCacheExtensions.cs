using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IoT.Extensions
{
    public static class DistributedCacheExtensions
    {
        private static readonly JsonSerializerOptions serializerOptions = new()
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static Task SetDataAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions? options = null)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, serializerOptions));

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

            value = JsonSerializer.Deserialize<T>(val, serializerOptions);

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
