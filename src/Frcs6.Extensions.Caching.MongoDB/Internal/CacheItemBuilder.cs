
using Microsoft.Extensions.Options;

namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal sealed class CacheItemBuilder(
    TimeProvider timeProvider,
    IOptions<MongoCacheOptions> mongoCacheOptions) : ICacheItemBuilder
{
    public CacheItem Build(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        var creationTime = timeProvider.GetUtcNow();
        var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);
        var expirationInSeconds = GetExpirationInSeconds(creationTime, absoluteExpiration, options);
        DateTimeOffset? expireAt = expirationInSeconds.HasValue ? creationTime.AddSeconds(expirationInSeconds.Value) : null;

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
                expiration = relativeExpiration <= slidingExpiration.Value ? relativeExpiration : slidingExpiration.Value;
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

    private double? GetExpirationInSeconds(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration, DistributedCacheEntryOptions options)
    {
        if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
        {
            return Math.Min((absoluteExpiration.Value - creationTime).TotalSeconds, options.SlidingExpiration.Value.TotalSeconds);
        }
        else if (absoluteExpiration.HasValue)
        {
            return (absoluteExpiration.Value - creationTime).TotalSeconds;
        }
        else if (options.SlidingExpiration.HasValue)
        {
            return options.SlidingExpiration.Value.TotalSeconds;
        }

        if(!mongoCacheOptions.Value.AllowNoExpiration)
        {
            throw new InvalidOperationException("Cache without expiration is not allowed");
        }

        return null;
    }

    private static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
    {
        if (options.AbsoluteExpiration.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.AbsoluteExpiration.Value, creationTime);
        }
        if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            return creationTime + options.AbsoluteExpirationRelativeToNow.Value;
        }
        return options.AbsoluteExpiration;
    }
}