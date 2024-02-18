using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Frcs6.Extensions.Caching.MongoDB.Tests.Internal;

public class CacheItemRepositoryTest
{
    private const string DatabaseName = "TestDatabase";
    private const string CollectionName = "CacheCollection";

    private readonly Mock<IMongoClient> _mongoClient = new();
    private readonly Mock<IMongoDatabase> _mongoDatabase = new();
    private readonly Mock<IMongoCollection<CacheItem>> _mongoCollection = new();
    private readonly CacheItemRepository _sut;

    public CacheItemRepositoryTest()
    {
        _mongoClient.Setup(c => c.GetDatabase(DatabaseName, null)).Returns(_mongoDatabase.Object);
        _mongoDatabase.Setup(d => d.GetCollection<CacheItem>(CollectionName, null)).Returns(_mongoCollection.Object);

        var mongoCacheOptions = Options.Create(new MongoCacheOptions { DatabaseName = DatabaseName, CollectionName = CollectionName });
        _sut = new CacheItemRepository(_mongoClient.Object, new FakeTimeProvider(), mongoCacheOptions);
    }

    [Fact]
    public void GivenNullKey_WhenRead_ThenArgumentNullException()
    {
        var act = () => _sut.Read(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullKey_WhenReadAsync_ThenArgumentNullException()
    {
        var act = () => _sut.ReadAsync(null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullKey_WhenReadPartial_ThenArgumentNullException()
    {
        var act = () => _sut.ReadPartial(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullKey_WhenReadPartialAsync_ThenArgumentNullException()
    {
        var act = () => _sut.ReadPartialAsync(null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullCacheItem_WhenWrite_ThenArgumentNullException()
    {
        var act = () => _sut.Write(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullCacheItem_WhenWriteAsync_ThenArgumentNullException()
    {
        var act = () => _sut.WriteAsync(null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullCacheItem_WhenWritePartial_ThenArgumentNullException()
    {
        var act = () => _sut.WritePartial(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullCacheItem_WhenWritePartialAsync_ThenArgumentNullException()
    {
        var act = () => _sut.WritePartialAsync(null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }    
    
    [Fact]
    public void GivenNullKey_WhenRemove_ThenArgumentNullException()
    {
        var act = () => _sut.Remove(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullKey_WhenRemoveAsync_ThenArgumentNullException()
    {
        var act = () => _sut.RemoveAsync(null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }
}