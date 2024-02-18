namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal interface ICacheItemRepository
{
    CacheItem Read(string key);
    Task<CacheItem> ReadAsync(string key);
    CacheItem ReadPartial(string key);
    Task<CacheItem> ReadPartialAsync(string key);
    
    void Write(CacheItem cacheItem);
    Task WriteAsync(CacheItem cacheItem);
    void WritePartial(CacheItem cacheItem);
    Task WritePartialAsync(CacheItem cacheItem);

    void Remove(string key);
    Task RemoveAsync(string key);
    void RemoveExpired();
}