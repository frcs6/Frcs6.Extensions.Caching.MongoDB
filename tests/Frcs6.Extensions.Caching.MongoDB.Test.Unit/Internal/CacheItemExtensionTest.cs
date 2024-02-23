namespace Frcs6.Extensions.Caching.MongoDB.Test.Unit.Internal;

public class CacheItemExtensionTest
{
    [Fact]
    public void GivenAbsoluteExpirationNull_WhenGetAbsoluteExpiration_ThenReturnNullValue()
    {
        var cacheItem = new CacheItem();
        var result = cacheItem.GetAbsoluteExpiration();
        result.Should().BeNull();
    }

    [Fact]
    public void GivenAbsoluteExpiration_WhenGetAbsoluteExpiration_ThenReturnValue()
    {
        var absoluteExpiration = DateTimeOffset.UtcNow;
        var cacheItem = new CacheItem { AbsoluteExpiration = absoluteExpiration.Ticks };
        var result = cacheItem.GetAbsoluteExpiration();
        result.Should().Be(absoluteExpiration);
    }

    [Fact]
    public void GivenAbsoluteExpirationNull_WhenSetAbsoluteExpiration_ThenSetNullValueAndObject()
    {
        var cacheItem = new CacheItem { AbsoluteExpiration = DateTimeOffset.UtcNow.Ticks };
        var result = cacheItem.SetAbsoluteExpiration(null);
        using (new AssertionScope())
        {
            result.AbsoluteExpiration.Should().BeNull();
            result.Should().Be(cacheItem);
        }
    }

    [Fact]
    public void GivenAbsoluteExpiration_WhenSetAbsoluteExpiration_ThenSetValueAndObject()
    {
        var absoluteExpiration = DateTimeOffset.UtcNow;
        var cacheItem = new CacheItem();
        var result = cacheItem.SetAbsoluteExpiration(absoluteExpiration);
        using (new AssertionScope())
        {
            result.AbsoluteExpiration.Should().Be(absoluteExpiration.Ticks);
            result.Should().Be(cacheItem);
        }
    }

    [Fact]
    public void GivenSlidingExpirationNull_WhenGetSlidingExpiration_ThenReturnNullValue()
    {
        var cacheItem = new CacheItem();
        var result = cacheItem.GetSlidingExpiration();
        result.Should().BeNull();
    }

    [Fact]
    public void GivenSlidingExpiration_WhenGetSlidingExpiration_ThenReturnValue()
    {
        var slidingExpiration = new TimeSpan(1234);
        var cacheItem = new CacheItem { SlidingExpiration = slidingExpiration.Ticks };
        var result = cacheItem.GetSlidingExpiration();
        result.Should().Be(slidingExpiration);
    }

    [Fact]
    public void GivenSlidingExpirationNull_WhenSetSlidingExpiration_ThenSetNullValueAndObject()
    {
        var cacheItem = new CacheItem { SlidingExpiration = 1234 };
        var result = cacheItem.SetSlidingExpiration(null);
        using (new AssertionScope())
        {
            result.SlidingExpiration.Should().BeNull();
            result.Should().Be(cacheItem);
        }
    }

    [Fact]
    public void GivenSlidingExpiration_WhenSetSlidingExpiration_ThenSetValueAndObject()
    {
        var slidingExpiration = new TimeSpan(1234);
        var cacheItem = new CacheItem();
        var result = cacheItem.SetSlidingExpiration(slidingExpiration);
        using (new AssertionScope())
        {
            result.SlidingExpiration.Should().Be(slidingExpiration.Ticks);
            result.Should().Be(cacheItem);
        }
    }

    [Fact]
    public void GivenExpireAtNull_WhenSetExpireAt_ThenSetNullValueAndObject()
    {
        var cacheItem = new CacheItem { ExpireAt = DateTimeOffset.UtcNow.Ticks };
        var result = cacheItem.SetExpireAt(null);
        using (new AssertionScope())
        {
            result.ExpireAt.Should().BeNull();
            result.Should().Be(cacheItem);
        }
    }

    [Fact]
    public void GivenExpireAt_WhenSetExpireAt_ThenSetValueAndObject()
    {
        var expireAt = DateTimeOffset.UtcNow;
        var cacheItem = new CacheItem();
        var result = cacheItem.SetExpireAt(expireAt);
        using (new AssertionScope())
        {
            result.ExpireAt.Should().Be(expireAt.Ticks);
            result.Should().Be(cacheItem);
        }
    }
}