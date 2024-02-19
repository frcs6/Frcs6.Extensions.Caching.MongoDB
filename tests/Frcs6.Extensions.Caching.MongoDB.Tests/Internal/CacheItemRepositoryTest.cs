﻿namespace Frcs6.Extensions.Caching.MongoDB.Tests.Internal;

public class CacheItemRepositoryTest : BaseTest
{
    private readonly Mock<IMongoClient> _mongoClient = new();
    private readonly Mock<IMongoDatabase> _mongoDatabase = new();
    private readonly Mock<IMongoCollection<CacheItem>> _mongoCollection = new();
    private readonly CacheItemRepository _sut;

    public CacheItemRepositoryTest()
    {
        _mongoClient.Setup(c => c.GetDatabase(DatabaseName, null)).Returns(_mongoDatabase.Object);
        _mongoDatabase.Setup(d => d.GetCollection<CacheItem>(CollectionName, null)).Returns(_mongoCollection.Object);
#if NET8_0_OR_GREATER
        _sut = new CacheItemRepository(_mongoClient.Object, new FakeTimeProvider(), BuildMongoCacheOptions());
#else
        _sut = new CacheItemRepository(_mongoClient.Object, new Mock<ISystemClock>().Object, BuildMongoCacheOptions());
#endif
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
        var act = () => _sut.ReadAsync(null!, default);
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
        var act = () => _sut.ReadPartialAsync(null!, default);
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
        var act = () => _sut.WriteAsync(null!, default);
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
        var act = () => _sut.WritePartialAsync(null!, default);
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
        var act = () => _sut.RemoveAsync(null!, default);
        act.Should().ThrowAsync<ArgumentNullException>();
    }
}