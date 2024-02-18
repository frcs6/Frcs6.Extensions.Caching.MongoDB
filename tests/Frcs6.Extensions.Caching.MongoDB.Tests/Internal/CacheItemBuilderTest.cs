using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Frcs6.Extensions.Caching.MongoDB.Tests.Internal;

public class CacheItemBuilderTest : BaseTest
{
    private DateTimeOffset _utcNow;
    private IOptions<MongoCacheOptions> _mongoCacheOptions;

    private readonly FakeTimeProvider _timeProvider = new();
    private readonly CacheItemBuilder _sut;

    public CacheItemBuilderTest()
    {
        ConfigureUtcNow(DateTimeOffset.UtcNow);
        _mongoCacheOptions = BuildMongoCacheOptions();
        _sut = new CacheItemBuilder(_timeProvider, _mongoCacheOptions);
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
        var key = keyIsNull ? null : DefaultKey;
        var value = valueIsNull ? null : DefaultValue;
        var options = optionsIsNull ? null : Fixture.Create<DistributedCacheEntryOptions>();

        var act = () => _sut.Build(key!, value!, options!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenInvalidAbsoluteExpiration_WhenBuild_ThrowArgumentOutOfRangeException()
    {
        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(_utcNow.AddHours(-1));

        var act = () => _sut.Build(DefaultKey, DefaultValue, options);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GiveKeyValueAbsoluteExpiration_WhenBuild_ThenReturnCacheItem()
    {
        var expiration = TimeSpan.FromHours(1);
        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(_utcNow.Add(expiration));

        var expected = new CacheItem
        {
            Key = DefaultKey,
            Value = DefaultValue,
            AbsoluteExpiration = options.AbsoluteExpiration!.Value.Ticks,
            ExpireAt = (_utcNow + expiration).Ticks
        };

        var result = _sut.Build(DefaultKey, DefaultValue, options);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GivenKeyValueNoOptions_WhenBuild_ThenReturnCacheItem()
    {
        var options = new DistributedCacheEntryOptions();

        var expected = new CacheItem
        {
            Key = DefaultKey,
            Value = DefaultValue
        };

        var result = _sut.Build(DefaultKey, DefaultValue, options);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GivenNoExpirationNotAllow_WhenBuild_ThrowInvalidOperationException()
    {
        _mongoCacheOptions.Value.AllowNoExpiration = false;
        var options = new DistributedCacheEntryOptions();
        var act = () => _sut.Build(DefaultKey, DefaultValue, options);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GiveKeyValueAbsoluteExpirationRelativeToNow_WhenBuild_ReturnCacheItem()
    {
        var expiration = TimeSpan.FromHours(1);
        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(expiration);

        var expected = new CacheItem
        {
            Key = DefaultKey,
            Value = DefaultValue,
            AbsoluteExpiration = (_utcNow + options.AbsoluteExpirationRelativeToNow!.Value).Ticks,
            ExpireAt = (_utcNow + expiration).Ticks
        };

        var result = _sut.Build(DefaultKey, DefaultValue, options);

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
            Key = DefaultKey,
            Value = DefaultValue,
            SlidingExpiration = options.SlidingExpiration!.Value.Ticks,
            ExpireAt = (_utcNow + expiration).Ticks
        };

        var result = _sut.Build(DefaultKey, DefaultValue, options);

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
            Key = DefaultKey,
            Value = DefaultValue,
            AbsoluteExpiration = (_utcNow + options.AbsoluteExpirationRelativeToNow!.Value).Ticks,
            SlidingExpiration = options.SlidingExpiration!.Value.Ticks,
            ExpireAt = (_utcNow + expiration1).Ticks
        };

        var result = _sut.Build(DefaultKey, DefaultValue, options);

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
            Key = DefaultKey,
            Value = DefaultValue,
            AbsoluteExpiration = (_utcNow + options.AbsoluteExpirationRelativeToNow!.Value).Ticks,
            SlidingExpiration = options.SlidingExpiration!.Value.Ticks,
            ExpireAt = (_utcNow + expiration2).Ticks
        };

        var result = _sut.Build(DefaultKey, DefaultValue, options);

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