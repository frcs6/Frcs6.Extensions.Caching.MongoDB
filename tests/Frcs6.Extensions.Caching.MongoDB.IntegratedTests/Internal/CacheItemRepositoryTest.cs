using FluentAssertions.Execution;
using Frcs6.Extensions.Caching.MongoDB.Tests.Common;
using Microsoft.Extensions.Options;

namespace Frcs6.Extensions.Caching.MongoDB.IntegratedTests.Internal;

public class CacheItemRepositoryTest : IClassFixture<MongoDatabaseFixture>
{
    private const string DatabaseName = "TestDatabase";

    private readonly Fixture _fixture = new();

    private DateTimeOffset _utcNow;

    private readonly MongoClient _mongoClient;
    private readonly IMongoCollection<CacheItem> _cacheItemCollection;
    private readonly string _collectionName;

#if NET8_0_OR_GREATER
    private readonly FakeTimeProvider _timeProvider = new();
#else
    private readonly Mock<ISystemClock> _timeProvider = new();
#endif

    public CacheItemRepositoryTest(MongoDatabaseFixture mongoDatabase)
    {
        _collectionName = _fixture.Create<string>();
#pragma warning disable CA1062 // Validate arguments of public methods
        _mongoClient = new MongoClient(mongoDatabase.GetConnectionString());
#pragma warning restore CA1062 // Validate arguments of public methods
        _cacheItemCollection = _mongoClient.GetDatabase(DatabaseName).GetCollection<CacheItem>(_collectionName);
        ConfigureUtcNow(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void GivenCacheItem_WhenRead_ThenReadCollection()
    {
        var sut = GetSut();
        var cacheItem = _fixture.Create<CacheItem>();
        var result = sut.Read(cacheItem.Key!);
        result.Should().BeNull();

        _cacheItemCollection.InsertOne(cacheItem);
        result = sut.Read(cacheItem.Key!);
        result.Should().BeEquivalentTo(cacheItem);
    }

    [Fact]
    public async Task GivenCacheItem_WhenReadAsync_ThenReadCollection()
    {
        var sut = GetSut();
        var cacheItem = _fixture.Create<CacheItem>();
        var result = await sut.ReadAsync(cacheItem.Key!, default);
        result.Should().BeNull();

        await _cacheItemCollection.InsertOneAsync(cacheItem);
        result = await sut.ReadAsync(cacheItem.Key!, default);
        result.Should().BeEquivalentTo(cacheItem);
    }

    [Fact]
    public void GivenCacheItem_WhenWrite_ThenWriteCollection()
    {
        var sut = GetSut();
        var cacheItem = _fixture.Create<CacheItem>();

        sut.Write(cacheItem);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).Single();
        result.Should().BeEquivalentTo(cacheItem);
    }

    [Fact]
    public async Task GivenCacheItem_WhenWriteSync_ThenWriteCollection()
    {
        var sut = GetSut();
        var cacheItem = _fixture.Create<CacheItem>();

        await sut.WriteAsync(cacheItem, default);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleAsync();
        result.Should().BeEquivalentTo(cacheItem);
    }

    [Fact]
    public void GivenCacheItem_WhenReadPartial_ThenReadCollection()
    {
        var sut = GetSut();
        var cacheItem = _fixture.Create<CacheItem>();
        var result = sut.ReadPartial(cacheItem.Key!);
        result.Should().BeNull();

        _cacheItemCollection.InsertOne(cacheItem);

        result = sut.ReadPartial(cacheItem.Key!);
        using (new AssertionScope())
        {
            result.Value.Should().BeNull();
            result.Should().BeEquivalentTo(cacheItem, option => option.Excluding(x => x.Value));
        }
    }

    [Fact]
    public async Task GivenCacheItem_WhenReadPartialAsync_ThenReadCollection()
    {
        var sut = GetSut();
        var cacheItem = _fixture.Create<CacheItem>();
        var result = await sut.ReadPartialAsync(cacheItem.Key!, default);
        result.Should().BeNull();

        await _cacheItemCollection.InsertOneAsync(cacheItem);

        result = await sut.ReadPartialAsync(cacheItem.Key!, default);
        using (new AssertionScope())
        {
            result.Value.Should().BeNull();
            result.Should().BeEquivalentTo(cacheItem, option => option.Excluding(x => x.Value));
        }
    }

    [Fact]
    public void GivenCacheItem_WhenWritePartial_ThenWriteCollection()
    {
        var sut = GetSut();
        var cacheItem = _fixture.Create<CacheItem>();
        _cacheItemCollection.InsertOne(cacheItem);
        var newCacheItem = _fixture
            .Build<CacheItem>()
            .With(i => i.Key, cacheItem.Key)
            .Without(i => i.Value)
            .Create();

        sut.WritePartial(newCacheItem);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).Single();
        using (new AssertionScope())
        {
            result.Value.Should().BeEquivalentTo(cacheItem.Value);
            result
                .Should()
                .BeEquivalentTo(cacheItem, option => option
                    .Excluding(x => x.AbsoluteExpiration)
                    .Excluding(x => x.ExpireAt)
                    .Excluding(x => x.SlidingExpiration));
        }
    }

    [Fact]
    public async Task GivenCacheItem_WhenWritePartialAsync_ThenWriteCollection()
    {
        var sut = GetSut();
        var cacheItem = _fixture.Create<CacheItem>();
        await _cacheItemCollection.InsertOneAsync(cacheItem);
        var newCacheItem = _fixture
            .Build<CacheItem>()
            .With(i => i.Key, cacheItem.Key)
            .Without(i => i.Value)
            .Create();

        await sut.WritePartialAsync(newCacheItem, default);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleAsync();
        using (new AssertionScope())
        {
            result.Value.Should().BeEquivalentTo(cacheItem.Value);
            result
                .Should()
                .BeEquivalentTo(cacheItem, option => option
                    .Excluding(x => x.AbsoluteExpiration)
                    .Excluding(x => x.ExpireAt)
                    .Excluding(x => x.SlidingExpiration));
        }
    }

    [Fact]
    public void GivenCacheItemDeleted_WhenWritePartial_ThenNoWriteCollection()
    {
        var sut = GetSut();
        var cacheItem = _fixture.Create<CacheItem>();

        sut.WritePartial(cacheItem);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefault();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenCacheItemDeleted_WhenWritePartialSync_ThenNoWriteCollection()
    {
        var sut = GetSut();
        var cacheItem = _fixture.Create<CacheItem>();

        await sut.WritePartialAsync(cacheItem, default);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefaultAsync();
        result.Should().BeNull();
    }

    [Fact]
    public void GivenCacheItem_WhenRemove_ThenRemoveCollection()
    {
        var sut = GetSut();
        var cacheItem = _fixture.Create<CacheItem>();
        _cacheItemCollection.InsertOne(cacheItem);

        sut.Remove(cacheItem.Key!);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefault();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenCacheItem_WhenRemoveAsync_ThenRemoveCollection()
    {
        var sut = GetSut();
        var cacheItem = _fixture.Create<CacheItem>();
        await _cacheItemCollection.InsertOneAsync(cacheItem);

        await sut.RemoveAsync(cacheItem.Key!, default);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefaultAsync();
        result.Should().BeNull();
    }

    [Fact]
    public void GivenCacheItem_WhenRemoveExpired_ThenRemoveCollection()
    {
        var sut = GetSut();
        (var cacheItems, var expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        sut.RemoveExpired();

        using (new AssertionScope())
        {
            cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(1));
            expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(0));
        }
    }

    [Fact]
    public void GivenRemoveExpiredDelayNotReach_WhenRemoveExpired_ThenKeepCollection()
    {
        var sut = GetSut(TimeSpan.FromHours(2));
        sut.RemoveExpired();
        (var cacheItems, var expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        sut.RemoveExpired();

        using (new AssertionScope())
        {
            cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(1));
            expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(1));
        }
    }

    [Fact]
    public void GivenRemoveExpiredDelayReach_WhenRemoveExpired_ThenRemoveCollection()
    {
        var sut = GetSut(TimeSpan.FromHours(2));
        sut.RemoveExpired();
        ConfigureUtcNow(_utcNow.AddHours(3));
        (var cacheItems, var expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        sut.RemoveExpired();

        using (new AssertionScope())
        {
            cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(1));
            expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(0));
        }
    }

    [Fact]
    public async Task GivenCacheItem_WhenRemoveExpiredAsync_ThenRemoveCollection()
    {
        var sut = GetSut();
        (var cacheItems, var expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        await sut.RemoveExpiredAsync(default);

        using (new AssertionScope())
        {
            cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(1));
            expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(0));
        }
    }

    [Fact]
    public async Task GivenRemoveExpiredDelayNotReach_WhenRemoveExpiredAsync_ThenKeepCollection()
    {
        var sut = GetSut(TimeSpan.FromHours(2));
        await sut.RemoveExpiredAsync(default);
        (var cacheItems, var expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        await sut.RemoveExpiredAsync(default);

        using (new AssertionScope())
        {
            cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(1));
            expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(1));
        }
    }

    [Fact]
    public async Task GivenRemoveExpiredDelayReach_WhenRemoveExpiredAsync_ThenRemoveCollection()
    {
        var sut = GetSut(TimeSpan.FromHours(2));
        await sut.RemoveExpiredAsync(default);
        ConfigureUtcNow(_utcNow.AddHours(3));
        (var cacheItems, var expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        await sut.RemoveExpiredAsync(default);

        using (new AssertionScope())
        {
            cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(1));
            expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(0));
        }
    }

    private void ConfigureUtcNow(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
#if NET8_0_OR_GREATER
        _timeProvider.SetUtcNow(utcNow);
#else
        _timeProvider.SetupGet(p => p.UtcNow).Returns(utcNow);
#endif
    }

    private CacheItemRepository GetSut(TimeSpan? removeExpiredDelay = null)
    {
        var mongoCacheOptions = Options.Create(new MongoCacheOptions
        {
            DatabaseName = DatabaseName,
            CollectionName = _collectionName,
            RemoveExpiredDelay = removeExpiredDelay
        });
#if NET8_0_OR_GREATER
        return  new(_mongoClient, _timeProvider, mongoCacheOptions);
#else
        return new(_mongoClient, _timeProvider.Object, mongoCacheOptions);
#endif
    }

    private (List<CacheItem>, List<CacheItem>) ArrangeCollectionWithExpiredItem()
    {
        var cacheItems = _fixture
            .Build<CacheItem>()
            .With(i => i.ExpireAt, _utcNow.AddHours(10).Ticks)
            .CreateMany(12)
            .ToList();
        _cacheItemCollection.InsertMany(cacheItems);

        var expiredCacheItems = _fixture
            .Build<CacheItem>()
            .With(i => i.ExpireAt, _utcNow.AddHours(-10).Ticks)
            .CreateMany(12)
            .ToList();
        _cacheItemCollection.InsertMany(expiredCacheItems);

        return (cacheItems, expiredCacheItems);
    }
}