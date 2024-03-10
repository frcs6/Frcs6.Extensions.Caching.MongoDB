namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal interface ICacheItemRepository : IDisposable
{
    CacheItem Read(string key);
    Task<CacheItem> ReadAsync(string key, CancellationToken token);
    CacheItem ReadPartial(string key);
    Task<CacheItem> ReadPartialAsync(string key, CancellationToken token);

    void Write(CacheItem cacheItem);
    Task WriteAsync(CacheItem cacheItem, CancellationToken token);
    void WritePartial(CacheItem cacheItem);
    Task WritePartialAsync(CacheItem cacheItem, CancellationToken token);

    void Remove(string key);
    Task RemoveAsync(string key, CancellationToken token);
    void RemoveExpired(bool force = false);
    Task RemoveExpiredAsync(CancellationToken token);
}