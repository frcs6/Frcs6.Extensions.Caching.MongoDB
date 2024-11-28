namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal sealed class CacheItemBuilder(TimeProvider timeProvider, IOptions<MongoCacheOptions> mongoCacheOptions)
    : ICacheItemBuilder
{
    public CacheItem Build(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        var creationTime = timeProvider.GetUtcNow();
        var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);
        var expirationInSeconds = GetExpirationInSeconds(creationTime, absoluteExpiration, options);
        DateTimeOffset? expireAt =
            expirationInSeconds.HasValue ? creationTime.AddSeconds(expirationInSeconds.Value) : null;

        return new CacheItem { Key = key, Value = value }
            .SetAbsoluteExpiration(absoluteExpiration)
            .SetSlidingExpiration(options.SlidingExpiration)
            .SetExpireAt(expireAt);
    }

    public bool Refresh(CacheItem cacheItem)
    {
        ArgumentNullException.ThrowIfNull(cacheItem);

        var utcNow = timeProvider.GetUtcNow();
        var absoluteExpiration = cacheItem.GetAbsoluteExpiration();
        var slidingExpiration = cacheItem.GetSlidingExpiration();

        if (slidingExpiration.HasValue)
        {
            TimeSpan expiration;
            if (absoluteExpiration.HasValue)
            {
                var relativeExpiration = absoluteExpiration.Value - utcNow;
                expiration = relativeExpiration <= slidingExpiration.Value
                    ? relativeExpiration
                    : slidingExpiration.Value;
            }
            else
            {
                expiration = slidingExpiration.Value;
            }

            cacheItem.SetExpireAt(utcNow + expiration);

            return true;
        }

        return false;
    }

    private double? GetExpirationInSeconds(
        DateTimeOffset creationTime,
        DateTimeOffset? absoluteExpiration,
        DistributedCacheEntryOptions options)
    {
        return absoluteExpiration.HasValue && options.SlidingExpiration.HasValue
            ? Math.Min(
                (absoluteExpiration.Value - creationTime).TotalSeconds,
                options.SlidingExpiration.Value.TotalSeconds)
            : absoluteExpiration.HasValue
                ? (absoluteExpiration.Value - creationTime).TotalSeconds
                : options.SlidingExpiration?.TotalSeconds ?? (!mongoCacheOptions.Value.AllowNoExpiration
                    ? throw new InvalidOperationException("Cache without expiration is not allowed")
                    : null);
    }

    private static DateTimeOffset? GetAbsoluteExpiration(
        DateTimeOffset creationTime,
        DistributedCacheEntryOptions options)
    {
        if (options.AbsoluteExpiration.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.AbsoluteExpiration.Value, creationTime);
        }

        return options.AbsoluteExpirationRelativeToNow.HasValue
            ? creationTime + options.AbsoluteExpirationRelativeToNow.Value
            : options.AbsoluteExpiration;
    }
}