// ReSharper disable ConvertToExtensionBlock

namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal static class CacheItemExtension
{
    public static DateTimeOffset? GetAbsoluteExpiration(this CacheItem cacheItem)
        => cacheItem.AbsoluteExpiration.HasValue
            ? new DateTimeOffset(cacheItem.AbsoluteExpiration.Value, TimeSpan.Zero)
            : null;

    public static CacheItem SetAbsoluteExpiration(this CacheItem cacheItem, DateTimeOffset? value)
    {
        cacheItem.AbsoluteExpiration = value?.Ticks;
        return cacheItem;
    }

    public static TimeSpan? GetSlidingExpiration(this CacheItem cacheItem)
        => cacheItem.SlidingExpiration.HasValue ? new TimeSpan(cacheItem.SlidingExpiration.Value) : null;

    public static CacheItem SetSlidingExpiration(this CacheItem cacheItem, TimeSpan? value)
    {
        cacheItem.SlidingExpiration = value?.Ticks;
        return cacheItem;
    }

    public static CacheItem SetExpireAt(this CacheItem cacheItem, DateTimeOffset? value)
    {
        cacheItem.ExpireAt = value?.Ticks;
        return cacheItem;
    }
}