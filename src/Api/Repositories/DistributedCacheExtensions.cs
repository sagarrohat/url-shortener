using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Api;

public static class DistributedCacheExtensions
{
    public static async Task SetRecordAsync<T>(this IDistributedCache cache,
        string key,
        T data,
        TimeSpan? absoluteExpireTime = null,
        TimeSpan? slidingExpireTime = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromDays(15),
            SlidingExpiration = slidingExpireTime
        };

        var jsonContent = JsonSerializer.Serialize(data);
        
        await cache.SetStringAsync(key, jsonContent, options);
    }

    public static async Task<T?> GetRecordAsync<T>(this IDistributedCache cache, string key)
    {
        var jsonContent = await cache.GetStringAsync(key);

        return jsonContent is null ? default(T) : JsonSerializer.Deserialize<T>(jsonContent);
    }
}