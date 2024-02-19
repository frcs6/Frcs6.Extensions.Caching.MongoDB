namespace Frcs6.Extensions.Caching.MongoDB.Tests.Internal;

public partial class MongoCacheTest
{
    private readonly Fixture _fixture = new();

    private readonly string _key;
    private readonly byte[] _value;
    private readonly DistributedCacheEntryOptions _options;
    private readonly CacheItem _cacheItem;

    private readonly Mock<ICacheItemBuilder> _cacheItemBuilder = new();
    private readonly Mock<ICacheItemRepository> _cacheItemRepository = new();
    private readonly MongoCache _sut;

    public MongoCacheTest()
    {
        _key = _fixture.Create<string>();
        _value = _fixture.CreateMany<byte>().ToArray();
        _options = _fixture.Create<DistributedCacheEntryOptions>();
        _cacheItem = new CacheItem { Value = _value };

        _sut = new MongoCache(_cacheItemBuilder.Object, _cacheItemRepository.Object);
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
        _sut.Get(_key);
        _cacheItemRepository.Verify(r => r.Read(_key), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenGet_ThenRemoveExpired()
    {
        _sut.Get(_key);
        _cacheItemRepository.Verify(r => r.RemoveExpired(), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenGet_ThenRefreshCacheItem()
    {
        _cacheItemRepository.Setup(r => r.Read(_key)).Returns(_cacheItem);
        _sut.Get(_key);
        _cacheItemBuilder.Verify(r => r.Refresh(_cacheItem), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenGet_ThenSaveUnRefreshedCacheItem()
    {
        _cacheItemRepository.Setup(r => r.Read(_key)).Returns(_cacheItem);
        _cacheItemBuilder.Setup(r => r.Refresh(_cacheItem)).Returns(true);
        _sut.Get(_key);
        _cacheItemRepository.Verify(r => r.WritePartial(_cacheItem), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenGet_ThenNoSaveRefreshedCacheItem()
    {
        _cacheItemRepository.Setup(r => r.Read(_key)).Returns(_cacheItem);
        _cacheItemBuilder.Setup(r => r.Refresh(_cacheItem)).Returns(false);
        _sut.Get(_key);
        _cacheItemRepository.Verify(r => r.WritePartial(_cacheItem), Times.Never);
    }

    [Fact]
    public void GivenKey_WhenGet_ThenReturnValue()
    {
        _cacheItemRepository.Setup(r => r.Read(_key)).Returns(_cacheItem);
        var result = _sut.Get(_key);
        result.Should().BeEquivalentTo(_value);
    }

    [Fact]
    public void GivenNullKey_WhenGetAsync_ThenArgumentNullException()
    {
        var act = () => _sut.GetAsync(null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenRead()
    {
        var token = CancellationToken.None;
        await _sut.GetAsync(_key);
        _cacheItemRepository.Verify(r => r.ReadAsync(_key, token), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenRemoveExpired()
    {
        var token = CancellationToken.None;
        await _sut.GetAsync(_key, token);
        _cacheItemRepository.Verify(r => r.RemoveExpiredAsync(token), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenRefreshCacheItem()
    {
        _cacheItemRepository.Setup(r => r.ReadAsync(_key, default)).ReturnsAsync(_cacheItem);
        await _sut.GetAsync(_key, default);
        _cacheItemBuilder.Verify(r => r.Refresh(_cacheItem), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenSaveRefreshedCacheItem()
    {
        var token = CancellationToken.None;
        _cacheItemRepository.Setup(r => r.ReadAsync(_key, token)).ReturnsAsync(_cacheItem);
        _cacheItemBuilder.Setup(r => r.Refresh(_cacheItem)).Returns(true);
        await _sut.GetAsync(_key);
        _cacheItemRepository.Verify(r => r.WritePartialAsync(_cacheItem, token), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenNoSaveUnrefreshedCacheItem()
    {
        var token = CancellationToken.None;
        _cacheItemRepository.Setup(r => r.ReadAsync(_key, token)).ReturnsAsync(_cacheItem);
        _cacheItemBuilder.Setup(r => r.Refresh(_cacheItem)).Returns(false);
        await _sut.GetAsync(_key);
        _cacheItemRepository.Verify(r => r.WritePartialAsync(_cacheItem, token), Times.Never);
    }

    [Fact]
    public async Task GivenKey_WhenGetAsync_ThenReturnValue()
    {
        _cacheItemRepository.Setup(r => r.ReadAsync(_key, default)).ReturnsAsync(_cacheItem);
        var result = await _sut.GetAsync(_key);
        result.Should().BeEquivalentTo(_value);
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
        var key = keyIsNull ? null : _key;
        var value = valueIsNull ? null : _value;
        var options = optionsIsNull ? null : _options;

        var act = () => _sut.Set(key!, value!, options!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenArguments_WhenSet_ThenBuildCacheItem()
    {
        _sut.Set(_key, _value, _options);
        _cacheItemBuilder.Verify(r => r.Build(_key, _value, _options), Times.Once);
    }

    [Fact]
    public void GivenArguments_WhenSet_ThenRemoveExpired()
    {
        _sut.Set(_key, _value, _options);
        _cacheItemRepository.Verify(r => r.RemoveExpired(), Times.Once);
    }

    [Fact]
    public void GivenArguments_WhenSet_ThenWriteCacheItem()
    {
        _cacheItemBuilder.Setup(b => b.Build(_key, _value, _options)).Returns(_cacheItem);
        _sut.Set(_key, _value, _options);
        _cacheItemRepository.Verify(r => r.Write(_cacheItem), Times.Once);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void GivenNullArguments_WhenSetAsync_ThenArgumentNullException(
    bool keyIsNull,
    bool valueIsNull,
    bool optionsIsNull)
    {
        var key = keyIsNull ? null : _key;
        var value = valueIsNull ? null : _value;
        var options = optionsIsNull ? null : _options;

        var act = () => _sut.SetAsync(key!, value!, options!);

        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenArguments_WhenSetAsync_ThenBuildCacheItem()
    {
        await _sut.SetAsync(_key, _value, _options);
        _cacheItemBuilder.Verify(r => r.Build(_key, _value, _options), Times.Once);
    }

    [Fact]
    public async Task GivenArguments_WhenSetAsync_ThenRemoveExpired()
    {
        await _sut.SetAsync(_key, _value, _options);
        _cacheItemRepository.Verify(r => r.RemoveExpired(), Times.Once);
    }

    [Fact]
    public async Task GivenArguments_WhenSetAsync_ThenWriteCacheItem()
    {
        var token = CancellationToken.None;
        _cacheItemBuilder.Setup(b => b.Build(_key, _value, _options)).Returns(_cacheItem);
        await _sut.SetAsync(_key, _value, _options, token);
        _cacheItemRepository.Verify(r => r.WriteAsync(_cacheItem, token), Times.Once);
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
        _sut.Refresh(_key);
        _cacheItemRepository.Verify(r => r.ReadPartial(_key), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenRefresh_ThenRemoveExpired()
    {
        _sut.Refresh(_key);
        _cacheItemRepository.Verify(r => r.RemoveExpired(), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenRefresh_ThenRefreshCacheItem()
    {
        _cacheItemRepository.Setup(r => r.ReadPartial(_key)).Returns(_cacheItem);
        _sut.Refresh(_key);
        _cacheItemBuilder.Verify(r => r.Refresh(_cacheItem), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenRefresh_ThenNoSaveUnrefreshedCacheItem()
    {
        _cacheItemRepository.Setup(r => r.ReadPartial(_key)).Returns(_cacheItem);
        _cacheItemBuilder.Setup(r => r.Refresh(_cacheItem)).Returns(false);
        _sut.Refresh(_key);
        _cacheItemRepository.Verify(r => r.WritePartial(_cacheItem), Times.Never);
    }

    [Fact]
    public void GivenNullKey_WhenRefreshAsync_ThenArgumentNullException()
    {
        var act = () => _sut.RefreshAsync(null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenKey_WhenRefreshAsync_ThenReadPartial()
    {
        var token = CancellationToken.None;
        await _sut.RefreshAsync(_key, token);
        _cacheItemRepository.Verify(r => r.ReadPartialAsync(_key, token), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenRefreshAsync_ThenRemoveExpired()
    {
        var token = CancellationToken.None;
        await _sut.RefreshAsync(_key, token);
        _cacheItemRepository.Verify(r => r.RemoveExpiredAsync(token), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenRefreshAsync_ThenRefreshCacheItem()
    {
        _cacheItemRepository.Setup(r => r.ReadPartialAsync(_key, default)).ReturnsAsync(_cacheItem);
        await _sut.RefreshAsync(_key);
        _cacheItemBuilder.Verify(r => r.Refresh(_cacheItem), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenRefreshAsync_ThenNoSaveUnrefreshedCacheItem()
    {
        var token = CancellationToken.None;
        _cacheItemRepository.Setup(r => r.ReadPartialAsync(_key, token)).ReturnsAsync(_cacheItem);
        _cacheItemBuilder.Setup(r => r.Refresh(_cacheItem)).Returns(false);
        await _sut.RefreshAsync(_key);
        _cacheItemRepository.Verify(r => r.WritePartialAsync(_cacheItem, token), Times.Never);
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
        _sut.Remove(_key);
        _cacheItemRepository.Verify(r => r.Remove(_key), Times.Once);
    }

    [Fact]
    public void GivenKey_WhenRemove_ThenRemoveExpired()
    {
        _sut.Remove(_key);
        _cacheItemRepository.Verify(r => r.RemoveExpired(), Times.Once);
    }

    [Fact]
    public void GivenNullKey_WhenRemoveAsync_ThenArgumentNullException()
    {
        var act = () => _sut.RemoveAsync(null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenKey_WhenRemoveAsync_ThenRemove()
    {
        var token = CancellationToken.None;
        await _sut.RemoveAsync(_key);
        _cacheItemRepository.Verify(r => r.RemoveAsync(_key, token), Times.Once);
    }

    [Fact]
    public async Task GivenKey_WhenRemoveAsync_ThenRemoveExpired()
    {
        await _sut.RemoveAsync(_key);
        _cacheItemRepository.Verify(r => r.RemoveExpired(), Times.Once);
    }
}