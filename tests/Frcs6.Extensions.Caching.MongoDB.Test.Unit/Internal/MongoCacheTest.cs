namespace Frcs6.Extensions.Caching.MongoDB.Test.Unit.Internal;

public class MongoCacheTest : BaseTest
{
    private readonly DistributedCacheEntryOptions _options;
    private readonly CacheItem _cacheItem;
    private readonly CacheItem _cacheItemNullValue;
    private readonly Mock<ICacheItemBuilder> _cacheItemBuilder;
    private readonly Mock<ICacheItemRepository> _cacheItemRepository;
    private readonly MongoCache _sut;

    public MongoCacheTest()
    {
        _options = Fixture.Create<DistributedCacheEntryOptions>();
        _cacheItem = new CacheItem { Value = DefaultValue };
        _cacheItemNullValue = new CacheItem { Value = null };
        _cacheItemBuilder = Fixture.Freeze<Mock<ICacheItemBuilder>>();
        _cacheItemRepository = Fixture.Freeze<Mock<ICacheItemRepository>>();
        _sut = Fixture.Create<MongoCache>();
    }

    [Fact]
    public void GivenNullKey_WhenGet_ThenArgumentNullException()
    {
        var act = () => _sut.Get(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenKey_WhenGet_ThenRead()
    {
        _sut.Get(DefaultKey);
        _cacheItemRepository.Verify(r => r.Read(DefaultKey), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenGet_ThenRemoveExpired()
    {
        _sut.Get(DefaultKey);
        _cacheItemRepository.Verify(r => r.RemoveExpired(false), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenGet_ThenRefreshCacheItem()
    {
        _cacheItemRepository.Setup(r => r.Read(DefaultKey)).Returns(_cacheItem);
        _sut.Get(DefaultKey);
        _cacheItemBuilder.Verify(r => r.Refresh(_cacheItem), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenGet_ThenSaveUnRefreshedCacheItem()
    {
        _cacheItemRepository.Setup(r => r.Read(DefaultKey)).Returns(_cacheItem);
        _cacheItemBuilder.Setup(r => r.Refresh(_cacheItem)).Returns(true);
        _sut.Get(DefaultKey);
        _cacheItemRepository.Verify(r => r.WritePartial(_cacheItem), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenGet_ThenNoSaveRefreshedCacheItem()
    {
        _cacheItemRepository.Setup(r => r.Read(DefaultKey)).Returns(_cacheItem);
        _cacheItemBuilder.Setup(r => r.Refresh(_cacheItem)).Returns(false);
        _sut.Get(DefaultKey);
        _cacheItemRepository.Verify(r => r.WritePartial(_cacheItem), Times.Never);
    }

    [Fact]
    public void GivenKey_WhenGet_ThenReturnValue()
    {
        _cacheItemRepository.Setup(r => r.Read(DefaultKey)).Returns(_cacheItem);
        var result = _sut.Get(DefaultKey);
        result.Should().BeEquivalentTo(DefaultValue);
    }

    [Fact]
    public void GivenKey_WhenGet_ThenReturnNullValue()
    {
        _cacheItemRepository.Setup(r => r.Read(DefaultKey)).Returns(_cacheItemNullValue);
        var result = _sut.Get(DefaultKey);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenNullKey_WhenGetAsync_ThenArgumentNullException()
    {
        var act = () => _sut.GetAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenRead()
    {
        await _sut.GetAsync(DefaultKey, DefaultToken);
        _cacheItemRepository.Verify(r => r.ReadAsync(DefaultKey, DefaultToken), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenRemoveExpired()
    {
        await _sut.GetAsync(DefaultKey, DefaultToken);
        _cacheItemRepository.Verify(r => r.RemoveExpiredAsync(DefaultToken), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenRefreshCacheItem()
    {
        _cacheItemRepository.Setup(r => r.ReadAsync(DefaultKey, DefaultToken)).ReturnsAsync(_cacheItem);
        await _sut.GetAsync(DefaultKey, DefaultToken);
        _cacheItemBuilder.Verify(r => r.Refresh(_cacheItem), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenSaveRefreshedCacheItem()
    {
        _cacheItemRepository.Setup(r => r.ReadAsync(DefaultKey, DefaultToken)).ReturnsAsync(_cacheItem);
        _cacheItemBuilder.Setup(r => r.Refresh(_cacheItem)).Returns(true);
        await _sut.GetAsync(DefaultKey);
        _cacheItemRepository.Verify(r => r.WritePartialAsync(_cacheItem, DefaultToken), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenNoSaveUnrefreshedCacheItem()
    {
        _cacheItemRepository.Setup(r => r.ReadAsync(DefaultKey, DefaultToken)).ReturnsAsync(_cacheItem);
        _cacheItemBuilder.Setup(r => r.Refresh(_cacheItem)).Returns(false);
        await _sut.GetAsync(DefaultKey);
        _cacheItemRepository.Verify(r => r.WritePartialAsync(_cacheItem, DefaultToken), Times.Never);
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenReturnValue()
    {
        _cacheItemRepository.Setup(r => r.ReadAsync(DefaultKey, DefaultToken)).ReturnsAsync(_cacheItem);
        var result = await _sut.GetAsync(DefaultKey, DefaultToken);
        result.Should().BeEquivalentTo(DefaultValue);
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenReturnValueNullValue()
    {
        _cacheItemRepository.Setup(r => r.ReadAsync(DefaultKey, DefaultToken)).ReturnsAsync(_cacheItemNullValue);
        var result = await _sut.GetAsync(DefaultKey, DefaultToken);
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void GivenNullArguments_WhenSet_ThenArgumentNullException(
        bool keyIsNull,
        bool valueIsNull,
        bool optionsIsNull)
    {
        var key = keyIsNull ? null : DefaultKey;
        var value = valueIsNull ? null : DefaultValue;
        var options = optionsIsNull ? null : _options;

        var act = () => _sut.Set(key!, value!, options!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenArguments_WhenSet_ThenBuildCacheItem()
    {
        _sut.Set(DefaultKey, DefaultValue, _options);
        _cacheItemBuilder.Verify(r => r.Build(DefaultKey, DefaultValue, _options), Times.Once);
    }

    [Fact]
    public void GivenArguments_WhenSet_ThenRemoveExpired()
    {
        _sut.Set(DefaultKey, DefaultValue, _options);
        _cacheItemRepository.Verify(r => r.RemoveExpired(false), Times.Once);
    }

    [Fact]
    public void GivenArguments_WhenSet_ThenWriteCacheItem()
    {
        _cacheItemBuilder.Setup(b => b.Build(DefaultKey, DefaultValue, _options)).Returns(_cacheItem);
        _sut.Set(DefaultKey, DefaultValue, _options);
        _cacheItemRepository.Verify(r => r.Write(_cacheItem), Times.Once);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public async Task GivenNullArguments_WhenSetAsync_ThenArgumentNullException(
        bool keyIsNull,
        bool valueIsNull,
        bool optionsIsNull)
    {
        var key = keyIsNull ? null : DefaultKey;
        var value = valueIsNull ? null : DefaultValue;
        var options = optionsIsNull ? null : _options;

        var act = () => _sut.SetAsync(key!, value!, options!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenArguments_WhenSetAsync_ThenBuildCacheItem()
    {
        await _sut.SetAsync(DefaultKey, DefaultValue, _options);
        _cacheItemBuilder.Verify(r => r.Build(DefaultKey, DefaultValue, _options), Times.Once);
    }

    [Fact]
    public async Task GivenArguments_WhenSetAsync_ThenRemoveExpired()
    {
        await _sut.SetAsync(DefaultKey, DefaultValue, _options, DefaultToken);
        _cacheItemRepository.Verify(r => r.RemoveExpiredAsync(DefaultToken), Times.Once);
    }

    [Fact]
    public async Task GivenArguments_WhenSetAsync_ThenWriteCacheItem()
    {
        _cacheItemBuilder.Setup(b => b.Build(DefaultKey, DefaultValue, _options)).Returns(_cacheItem);
        await _sut.SetAsync(DefaultKey, DefaultValue, _options, DefaultToken);
        _cacheItemRepository.Verify(r => r.WriteAsync(_cacheItem, DefaultToken), Times.Once);
    }

    [Fact]
    public void GivenNullKey_WhenRefresh_ThenArgumentNullException()
    {
        var act = () => _sut.Refresh(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenKey_WhenRefresh_ThenReadPartial()
    {
        _sut.Refresh(DefaultKey);
        _cacheItemRepository.Verify(r => r.ReadPartial(DefaultKey), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenRefresh_ThenRemoveExpired()
    {
        _sut.Refresh(DefaultKey);
        _cacheItemRepository.Verify(r => r.RemoveExpired(false), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenRefresh_ThenRefreshCacheItem()
    {
        _cacheItemRepository.Setup(r => r.ReadPartial(DefaultKey)).Returns(_cacheItem);
        _sut.Refresh(DefaultKey);
        _cacheItemBuilder.Verify(r => r.Refresh(_cacheItem), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenRefresh_ThenNoSaveUnrefreshedCacheItem()
    {
        _cacheItemRepository.Setup(r => r.ReadPartial(DefaultKey)).Returns(_cacheItem);
        _cacheItemBuilder.Setup(r => r.Refresh(_cacheItem)).Returns(false);
        _sut.Refresh(DefaultKey);
        _cacheItemRepository.Verify(r => r.WritePartial(_cacheItem), Times.Never);
    }

    [Fact]
    public async Task GivenNullKey_WhenRefreshAsync_ThenArgumentNullException()
    {
        var act = () => _sut.RefreshAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenKey_WhenRefreshAsync_ThenReadPartial()
    {
        await _sut.RefreshAsync(DefaultKey, DefaultToken);
        _cacheItemRepository.Verify(r => r.ReadPartialAsync(DefaultKey, DefaultToken), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenRefreshAsync_ThenRemoveExpired()
    {
        await _sut.RefreshAsync(DefaultKey, DefaultToken);
        _cacheItemRepository.Verify(r => r.RemoveExpiredAsync(DefaultToken), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenRefreshAsync_ThenRefreshCacheItem()
    {
        _cacheItemRepository.Setup(r => r.ReadPartialAsync(DefaultKey, DefaultToken)).ReturnsAsync(_cacheItem);
        await _sut.RefreshAsync(DefaultKey, DefaultToken);
        _cacheItemBuilder.Verify(r => r.Refresh(_cacheItem), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenRefreshAsync_ThenNoSaveUnrefreshedCacheItem()
    {
        _cacheItemRepository.Setup(r => r.ReadPartialAsync(DefaultKey, DefaultToken)).ReturnsAsync(_cacheItem);
        _cacheItemBuilder.Setup(r => r.Refresh(_cacheItem)).Returns(false);
        await _sut.RefreshAsync(DefaultKey, DefaultToken);
        _cacheItemRepository.Verify(r => r.WritePartialAsync(_cacheItem, DefaultToken), Times.Never);
    }

    [Fact]
    public void GivenNullKey_WhenRemove_ThenArgumentNullException()
    {
        var act = () => _sut.Remove(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenKey_WhenRemove_ThenRemove()
    {
        _sut.Remove(DefaultKey);
        _cacheItemRepository.Verify(r => r.Remove(DefaultKey), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenRemove_ThenRemoveExpired()
    {
        _sut.Remove(DefaultKey);
        _cacheItemRepository.Verify(r => r.RemoveExpired(false), Times.Once);
    }

    [Fact]
    public async Task GivenNullKey_WhenRemoveAsync_ThenArgumentNullException()
    {
        var act = () => _sut.RemoveAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenKey_WhenRemoveAsync_ThenRemove()
    {
        await _sut.RemoveAsync(DefaultKey, DefaultToken);
        _cacheItemRepository.Verify(r => r.RemoveAsync(DefaultKey, DefaultToken), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenRemoveAsync_ThenRemoveExpired()
    {
        await _sut.RemoveAsync(DefaultKey, DefaultToken);
        _cacheItemRepository.Verify(r => r.RemoveExpiredAsync(DefaultToken), Times.Once);
    }
}