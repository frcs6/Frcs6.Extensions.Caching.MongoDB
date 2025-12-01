namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal static class CacheItemExtension
{
    extension(CacheItem cacheItem)
    {
        public DateTimeOffset? GetAbsoluteExpiration()
            => cacheItem.AbsoluteExpiration.HasValue
                ? new DateTimeOffset(cacheItem.AbsoluteExpiration.Value, TimeSpan.Zero)
                : null;

        public CacheItem SetAbsoluteExpiration(DateTimeOffset? value)
        {
            cacheItem.AbsoluteExpiration = value?.Ticks;
            return cacheItem;
        }

        public TimeSpan? GetSlidingExpiration()
            => cacheItem.SlidingExpiration.HasValue ? new TimeSpan(cacheItem.SlidingExpiration.Value) : null;

        public CacheItem SetSlidingExpiration(TimeSpan? value)
        {
            cacheItem.SlidingExpiration = value?.Ticks;
            return cacheItem;
        }

        public CacheItem SetExpireAt(DateTimeOffset? value)
        {
            cacheItem.ExpireAt = value?.Ticks;
            return cacheItem;
        }
    }
}