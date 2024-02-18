using Microsoft.Extensions.Time.Testing;

namespace Frcs6.Extensions.Caching.MongoDB.Tests.Internal;

public class CacheItemBuilderTest
{
    private readonly Fixture _fixture = new();

    private readonly string _key;
    private readonly byte[] _value;
    private DateTimeOffset _utcNow;

    private readonly FakeTimeProvider _timeProvider = new();
    private readonly CacheItemBuilder _sut;

    public CacheItemBuilderTest()
    {
        _key = _fixture.Create<string>();
        _value = _fixture.CreateMany<byte>().ToArray();

        ConfigureUtcNow(DateTimeOffset.UtcNow);

        _sut = new CacheItemBuilder(_timeProvider);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void GivenNullArguments_WhenBuild_ThenArgumentNullException(
        bool keyIsNull,
        bool valueIsNull,
        bool optionsIsNull)
    {
        var key = keyIsNull ? null : _key;
        var value = valueIsNull ? null : _value;
        var options = optionsIsNull ? null : _fixture.Create<DistributedCacheEntryOptions>();

        var act = () => _sut.Build(key!, value!, options!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenInvalidAbsoluteExpiration_WhenBuild_ThrowArgumentOutOfRangeException()
    {
        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(_utcNow.AddHours(-1));

        var act = () => _sut.Build(_key, _value, options);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GiveKeyValueAbsoluteExpiration_WhenBuild_ReturnCacheItem()
    {
        var expiration = TimeSpan.FromHours(1);
        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(_utcNow.Add(expiration));

        var expected = new CacheItem
        {
            Key = _key,
            Value = _value,
            AbsoluteExpiration = options.AbsoluteExpiration!.Value.Ticks,
            ExpireAt = (_utcNow + expiration).Ticks
        };

        var result = _sut.Build(_key, _value, options);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GivenKeyValueNoOptions_WhenBuild_ThrowArgumentOutOfRangeException()
    {
        var options = new DistributedCacheEntryOptions();

        var expected = new CacheItem
        {
            Key = _key,
            Value = _value
        };

        var result = _sut.Build(_key, _value, options);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GiveKeyValueAbsoluteExpirationRelativeToNow_WhenBuild_ReturnCacheItem()
    {
        var expiration = TimeSpan.FromHours(1);
        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(expiration);

        var expected = new CacheItem
        {
            Key = _key,
            Value = _value,
            AbsoluteExpiration = (_utcNow + options.AbsoluteExpirationRelativeToNow!.Value).Ticks,
            ExpireAt = (_utcNow + expiration).Ticks
        };

        var result = _sut.Build(_key, _value, options);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GiveKeyValueSlidingExpiration_WhenBuild_ReturnCacheItem()
    {
        var expiration = TimeSpan.FromHours(1);
        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(expiration);

        var expected = new CacheItem
        {
            Key = _key,
            Value = _value,
            SlidingExpiration = options.SlidingExpiration!.Value.Ticks,
            ExpireAt = (_utcNow + expiration).Ticks
        };

        var result = _sut.Build(_key, _value, options);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GiveKeyValueSlidingExpirationLessThanAbsoluteExpirationRelativeToNow_WhenBuild_ReturnCacheItem()
    {
        var expiration1 = TimeSpan.FromHours(1);
        var expiration2 = TimeSpan.FromHours(2);
        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(expiration1)
            .SetAbsoluteExpiration(expiration2);

        var expected = new CacheItem
        {
            Key = _key,
            Value = _value,
            AbsoluteExpiration = (_utcNow + options.AbsoluteExpirationRelativeToNow!.Value).Ticks,
            SlidingExpiration = options.SlidingExpiration!.Value.Ticks,
            ExpireAt = (_utcNow + expiration1).Ticks
        };

        var result = _sut.Build(_key, _value, options);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GiveKeyValueAbsoluteExpirationRelativeToNowLessThanSlidingExpiration_WhenBuild_ReturnCacheItem()
    {
        var expiration1 = TimeSpan.FromHours(2);
        var expiration2 = TimeSpan.FromHours(1);
        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(expiration1)
            .SetAbsoluteExpiration(expiration2);

        var expected = new CacheItem
        {
            Key = _key,
            Value = _value,
            AbsoluteExpiration = (_utcNow + options.AbsoluteExpirationRelativeToNow!.Value).Ticks,
            SlidingExpiration = options.SlidingExpiration!.Value.Ticks,
            ExpireAt = (_utcNow + expiration2).Ticks
        };

        var result = _sut.Build(_key, _value, options);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GivenNoSlidingExpiration_WhenRefresh_ThenDoNothingAndReturnFalse()
    {
        var cacheItem = new CacheItem();
        var expected = new CacheItem();

        var result = _sut.Refresh(cacheItem);

        using (new AssertionScope())
        {
            cacheItem.Should().BeEquivalentTo(expected);
            result.Should().BeFalse();
        }
    }

    [Fact]
    public void GivenOnlySlidingExpiration_WhenRefresh_ThenUpdateAndReturnTrue()
    {
        var slidingExpiration = TimeSpan.FromMinutes(10);
        var expected = new CacheItem()
            .SetSlidingExpiration(slidingExpiration)
            .SetExpireAt(_utcNow + slidingExpiration);

        var cacheItem = new CacheItem().SetSlidingExpiration(slidingExpiration);

        var result = _sut.Refresh(cacheItem);

        using (new AssertionScope())
        {
            cacheItem.Should().BeEquivalentTo(expected);
            result.Should().BeTrue();
        }
    }

    [Fact]
    public void GivenSlidingExpirationLessThenAbsoluteExpiration_WhenRefresh_ThenUpdateAndReturnTrue()
    {
        var slidingExpiration = TimeSpan.FromMinutes(10);
        var absoluteExpiration = _utcNow.AddMinutes(5);
        var expected = new CacheItem()
            .SetSlidingExpiration(slidingExpiration)
            .SetAbsoluteExpiration(absoluteExpiration)
            .SetExpireAt(absoluteExpiration);

        var cacheItem = new CacheItem()
            .SetSlidingExpiration(slidingExpiration)
            .SetAbsoluteExpiration(absoluteExpiration);

        var result = _sut.Refresh(cacheItem);

        using (new AssertionScope())
        {
            cacheItem.Should().BeEquivalentTo(expected);
            result.Should().BeTrue();
        }
    }

    [Fact]
    public void GivenAbsoluteExpirationThenSlidingExpirationLess_WhenRefresh_ThenUpdateAndReturnTrue()
    {
        var slidingExpiration = TimeSpan.FromMinutes(10);
        var absoluteExpiration = _utcNow.AddMinutes(20);
        var expected = new CacheItem()
            .SetSlidingExpiration(slidingExpiration)
            .SetAbsoluteExpiration(absoluteExpiration)
            .SetExpireAt(_utcNow + slidingExpiration);

        var cacheItem = new CacheItem()
            .SetSlidingExpiration(slidingExpiration)
            .SetAbsoluteExpiration(absoluteExpiration);

        var result = _sut.Refresh(cacheItem);

        using (new AssertionScope())
        {
            cacheItem.Should().BeEquivalentTo(expected);
            result.Should().BeTrue();
        }
    }

    private void ConfigureUtcNow(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
        _timeProvider.SetUtcNow(utcNow);
    }
}