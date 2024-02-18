namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal interface ICacheItemBuilder
{
    CacheItem Build(string key, byte[] value, DistributedCacheEntryOptions options);
    bool Refresh(CacheItem cacheItem);
}