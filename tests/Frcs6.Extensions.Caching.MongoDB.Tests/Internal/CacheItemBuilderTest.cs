namespace Frcs6.Extensions.Caching.MongoDB.Tests.Internal;

public class CacheItemBuilderTest : BaseTest
{
    private readonly CacheItemBuilder _sut;

    public CacheItemBuilderTest()
    {
        _sut = Fixture.Create<CacheItemBuilder>();
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
            .SetAbsoluteExpiration(UtcNow.AddHours(-1));

        var act = () => _sut.Build(DefaultKey, DefaultValue, options);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GiveKeyValueAbsoluteExpiration_WhenBuild_ThenReturnCacheItem()
    {
        var expiration = TimeSpan.FromHours(1);
        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(UtcNow.Add(expiration));

        var expected = new CacheItem
        {
            Key = DefaultKey,
            Value = DefaultValue,
            AbsoluteExpiration = options.AbsoluteExpiration!.Value.Ticks,
            ExpireAt = (UtcNow + expiration).Ticks
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
        MongoCacheOptions.AllowNoExpiration = false;
        var options = new DistributedCacheEntryOptions();
        var act = () => _sut.Build(DefaultKey, DefaultValue, options);
        act.Should().Throw<InvalidOperationException>().WithMessage("Cache without expiration is not allowed");
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
            AbsoluteExpiration = (UtcNow + options.AbsoluteExpirationRelativeToNow!.Value).Ticks,
            ExpireAt = (UtcNow + expiration).Ticks
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
            ExpireAt = (UtcNow + expiration).Ticks
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
            AbsoluteExpiration = (UtcNow + options.AbsoluteExpirationRelativeToNow!.Value).Ticks,
            SlidingExpiration = options.SlidingExpiration!.Value.Ticks,
            ExpireAt = (UtcNow + expiration1).Ticks
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
            AbsoluteExpiration = (UtcNow + options.AbsoluteExpirationRelativeToNow!.Value).Ticks,
            SlidingExpiration = options.SlidingExpiration!.Value.Ticks,
            ExpireAt = (UtcNow + expiration2).Ticks
        };

        var result = _sut.Build(DefaultKey, DefaultValue, options);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GivenNullCacheItem_WhenRefresh_ThenArgumentNullException()
    {
        var act = () => _sut.Refresh(null!);
        act.Should().Throw<ArgumentNullException>();
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
            .SetExpireAt(UtcNow + slidingExpiration);

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
        var absoluteExpiration = UtcNow.AddMinutes(5);
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
    public void GivenAbsoluteExpirationLessThenSlidingExpiration_WhenRefresh_ThenUpdateAndReturnTrue()
    {
        var slidingExpiration = TimeSpan.FromMinutes(10);
        var absoluteExpiration = UtcNow.AddMinutes(20);
        var expected = new CacheItem()
            .SetSlidingExpiration(slidingExpiration)
            .SetAbsoluteExpiration(absoluteExpiration)
            .SetExpireAt(UtcNow + slidingExpiration);

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
}