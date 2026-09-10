namespace Frcs6.Extensions.Caching.MongoDB.Test.Unit.Internal;

public class CacheItemTest
{
    [Fact]
    public void GivenCacheItem_WhenCreateInstance_ThenAllPropertiesAreNull()
    {
        var cacheItem = new CacheItem();

        cacheItem.Key.ShouldBeNull();
        cacheItem.Value.ShouldBeNull();
        cacheItem.AbsoluteExpiration.ShouldBeNull();
        cacheItem.SlidingExpiration.ShouldBeNull();
        cacheItem.ExpireAt.ShouldBeNull();
    }

    [Fact]
    public void GivenCacheItem_WhenSetKey_ThenKeyIsSet()
    {
        var cacheItem = new CacheItem();
        var key = "test-key";

        cacheItem.Key = key;

        cacheItem.Key.ShouldBe(key);
    }

    [Fact]
    public void GivenCacheItem_WhenSetValue_ThenValueIsSet()
    {
        var cacheItem = new CacheItem();
        var value = new byte[] { 1, 2, 3 };

        cacheItem.Value = value;

        cacheItem.Value.ShouldBe(value);
    }

    [Fact]
    public void GivenCacheItem_WhenSetAbsoluteExpiration_ThenAbsoluteExpirationIsSet()
    {
        var cacheItem = new CacheItem();
        var expiration = DateTime.UtcNow.AddHours(1).Ticks;

        cacheItem.AbsoluteExpiration = expiration;

        cacheItem.AbsoluteExpiration.ShouldBe(expiration);
    }

    [Fact]
    public void GivenCacheItem_WhenSetSlidingExpiration_ThenSlidingExpirationIsSet()
    {
        var cacheItem = new CacheItem();
        var expiration = TimeSpan.FromMinutes(30).Ticks;

        cacheItem.SlidingExpiration = expiration;

        cacheItem.SlidingExpiration.ShouldBe(expiration);
    }

    [Fact]
    public void GivenCacheItem_WhenSetExpireAt_ThenExpireAtIsSet()
    {
        var cacheItem = new CacheItem();
        var expireAt = DateTime.UtcNow.AddMinutes(10).Ticks;

        cacheItem.ExpireAt = expireAt;

        cacheItem.ExpireAt.ShouldBe(expireAt);
    }
}
