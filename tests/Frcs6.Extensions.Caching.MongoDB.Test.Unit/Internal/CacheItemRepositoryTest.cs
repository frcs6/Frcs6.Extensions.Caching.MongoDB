using Microsoft.Extensions.Options;

namespace Frcs6.Extensions.Caching.MongoDB.Test.Unit.Internal;

public class CacheItemRepositoryTest : BaseTest
{
    private readonly Mock<IMongoClient> _mongoClient;
    private readonly Mock<IMongoDatabase> _mongoDatabase = new();
    private readonly Mock<IMongoCollection<CacheItem>> _mongoCollection = new();
    private readonly Mock<IMongoIndexManager<CacheItem>> _mongoIndexManager = new();
    private readonly CacheItemRepository _sut;

    public CacheItemRepositoryTest()
    {
        _mongoClient = Fixture.Freeze<Mock<IMongoClient>>();
        _mongoClient.Setup(c => c.GetDatabase(DatabaseName, null)).Returns(_mongoDatabase.Object);
        _mongoDatabase.Setup(d => d.GetCollection<CacheItem>(CollectionName, null)).Returns(_mongoCollection.Object);
        _mongoCollection.Setup(c => c.Indexes).Returns(_mongoIndexManager.Object);
        _sut = Fixture.Create<CacheItemRepository>();
    }

    [Theory]
    [InlineData("", "CollectionName")]
    [InlineData(null, "CollectionName")]
    [InlineData("DatabaseName", "")]
    [InlineData("DatabaseName", null)]
    public void GivenNoDatabaseParams_WhenCtor_ThenArgumentException(string? databaseName, string? collectionName)
    {
        var mongoCacheOptions = new MongoCacheOptions { DatabaseName = databaseName, CollectionName = collectionName };
        var act = () => new CacheItemRepository(_mongoClient.Object, TimeProvider, Options.Create(mongoCacheOptions));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Given_WhenCtor_ThenAddTwoIndex()
    {
        _mongoIndexManager
            .Verify(
                m => m.CreateOne(It.IsAny<CreateIndexModel<CacheItem>>(), null, default),
                Times.Exactly(2));
    }

    [Fact]
    public void GivenNullKey_WhenRead_ThenArgumentNullException()
    {
        var act = () => _sut.Read(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenNullKey_WhenReadAsync_ThenArgumentNullException()
    {
        var act = () => _sut.ReadAsync(null!, default);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullKey_WhenReadPartial_ThenArgumentNullException()
    {
        var act = () => _sut.ReadPartial(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenNullKey_WhenReadPartialAsync_ThenArgumentNullException()
    {
        var act = () => _sut.ReadPartialAsync(null!, default);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullCacheItem_WhenWrite_ThenArgumentNullException()
    {
        var act = () => _sut.Write(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenNullCacheItem_WhenWriteAsync_ThenArgumentNullException()
    {
        var act = () => _sut.WriteAsync(null!, default);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullCacheItem_WhenWritePartial_ThenArgumentNullException()
    {
        var act = () => _sut.WritePartial(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenNullCacheItem_WhenWritePartialAsync_ThenArgumentNullException()
    {
        var act = () => _sut.WritePartialAsync(null!, default);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullKey_WhenRemove_ThenArgumentNullException()
    {
        var act = () => _sut.Remove(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenNullKey_WhenRemoveAsync_ThenArgumentNullException()
    {
        var act = () => _sut.RemoveAsync(null!, default);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}