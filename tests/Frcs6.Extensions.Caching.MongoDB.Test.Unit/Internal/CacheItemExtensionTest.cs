namespace Frcs6.Extensions.Caching.MongoDB.Test.Unit.Internal;

public class CacheItemExtensionTest
{
    [Fact]
    public void GivenAbsoluteExpirationNull_WhenGetAbsoluteExpiration_ThenReturnNullValue()
    {
        var cacheItem = new CacheItem();
        var result = cacheItem.GetAbsoluteExpiration();
        result.ShouldBeNull();
    }

    [Fact]
    public void GivenAbsoluteExpiration_WhenGetAbsoluteExpiration_ThenReturnValue()
    {
        var absoluteExpiration = DateTimeOffset.UtcNow;
        var cacheItem = new CacheItem { AbsoluteExpiration = absoluteExpiration.Ticks };
        var result = cacheItem.GetAbsoluteExpiration();
        result.ShouldBe(absoluteExpiration);
    }

    [Fact]
    public void GivenAbsoluteExpirationNull_WhenSetAbsoluteExpiration_ThenSetNullValueAndObject()
    {
        var cacheItem = new CacheItem { AbsoluteExpiration = DateTimeOffset.UtcNow.Ticks };
        var result = cacheItem.SetAbsoluteExpiration(null);
        result.AbsoluteExpiration.ShouldBeNull();
        result.ShouldBe(cacheItem);
    }

    [Fact]
    public void GivenAbsoluteExpiration_WhenSetAbsoluteExpiration_ThenSetValueAndObject()
    {
        var absoluteExpiration = DateTimeOffset.UtcNow;
        var cacheItem = new CacheItem();
        var result = cacheItem.SetAbsoluteExpiration(absoluteExpiration);
        result.AbsoluteExpiration.ShouldBe(absoluteExpiration.Ticks);
        result.ShouldBe(cacheItem);
    }

    [Fact]
    public void GivenSlidingExpirationNull_WhenGetSlidingExpiration_ThenReturnNullValue()
    {
        var cacheItem = new CacheItem();
        var result = cacheItem.GetSlidingExpiration();
        result.ShouldBeNull();
    }

    [Fact]
    public void GivenSlidingExpiration_WhenGetSlidingExpiration_ThenReturnValue()
    {
        var slidingExpiration = new TimeSpan(1234);
        var cacheItem = new CacheItem { SlidingExpiration = slidingExpiration.Ticks };
        var result = cacheItem.GetSlidingExpiration();
        result.ShouldBe(slidingExpiration);
    }

    [Fact]
    public void GivenSlidingExpirationNull_WhenSetSlidingExpiration_ThenSetNullValueAndObject()
    {
        var cacheItem = new CacheItem { SlidingExpiration = 1234 };
        var result = cacheItem.SetSlidingExpiration(null);
        result.SlidingExpiration.ShouldBeNull();
        result.ShouldBe(cacheItem);
    }

    [Fact]
    public void GivenSlidingExpiration_WhenSetSlidingExpiration_ThenSetValueAndObject()
    {
        var slidingExpiration = new TimeSpan(1234);
        var cacheItem = new CacheItem();
        var result = cacheItem.SetSlidingExpiration(slidingExpiration);
        result.SlidingExpiration.ShouldBe(slidingExpiration.Ticks);
        result.ShouldBe(cacheItem);
    }

    [Fact]
    public void GivenExpireAtNull_WhenSetExpireAt_ThenSetNullValueAndObject()
    {
        var cacheItem = new CacheItem { ExpireAt = DateTimeOffset.UtcNow.Ticks };
        var result = cacheItem.SetExpireAt(null);
        result.ExpireAt.ShouldBeNull();
        result.ShouldBe(cacheItem);
    }

    [Fact]
    public void GivenExpireAt_WhenSetExpireAt_ThenSetValueAndObject()
    {
        var expireAt = DateTimeOffset.UtcNow;
        var cacheItem = new CacheItem();
        var result = cacheItem.SetExpireAt(expireAt);
        result.ExpireAt.ShouldBe(expireAt.Ticks);
        result.ShouldBe(cacheItem);
    }
}