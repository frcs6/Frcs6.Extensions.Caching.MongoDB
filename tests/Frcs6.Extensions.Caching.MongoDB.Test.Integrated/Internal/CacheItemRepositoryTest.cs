namespace Frcs6.Extensions.Caching.MongoDB.Test.Integrated.Internal;

public class CacheItemRepositoryTest : BaseTest, IClassFixture<MongoDatabaseTest>
{
    private readonly MongoClient _mongoClient;
    private readonly IMongoCollection<CacheItem> _cacheItemCollection;

    public CacheItemRepositoryTest(MongoDatabaseTest mongoDatabase)
    {
#pragma warning disable CA1062 // Validate arguments of public methods
        _mongoClient = new MongoClient(mongoDatabase.GetConnectionString());
#pragma warning restore CA1062 // Validate arguments of public methods
        Fixture.Register<IMongoClient>(() => _mongoClient);
        _cacheItemCollection = _mongoClient.GetDatabase(DatabaseName).GetCollection<CacheItem>(CollectionName);
    }

    [Fact]
    public void GivenCacheItem_WhenRead_ThenReadCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        var result = sut.Read(cacheItem.Key!);
        result.Should().BeNull();

        _cacheItemCollection.InsertOne(cacheItem);
        result = sut.Read(cacheItem.Key!);
        result.Should().BeEquivalentTo(cacheItem);
    }

    [Fact]
    public async Task GivenCacheItem_WhenReadAsync_ThenReadCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        var result = await sut.ReadAsync(cacheItem.Key!, default);
        result.Should().BeNull();

        await _cacheItemCollection.InsertOneAsync(cacheItem);
        result = await sut.ReadAsync(cacheItem.Key!, default);
        result.Should().BeEquivalentTo(cacheItem);
    }

    [Fact]
    public void GivenCacheItem_WhenWrite_ThenWriteCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();

        sut.Write(cacheItem);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).Single();
        result.Should().BeEquivalentTo(cacheItem);
    }

    [Fact]
    public async Task GivenCacheItem_WhenWriteSync_ThenWriteCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();

        await sut.WriteAsync(cacheItem, default);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleAsync();
        result.Should().BeEquivalentTo(cacheItem);
    }

    [Fact]
    public void GivenCacheItem_WhenReadPartial_ThenReadCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
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
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
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
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        _cacheItemCollection.InsertOne(cacheItem);
        var newCacheItem = Fixture
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
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        await _cacheItemCollection.InsertOneAsync(cacheItem);
        var newCacheItem = Fixture
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
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();

        sut.WritePartial(cacheItem);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefault();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenCacheItemDeleted_WhenWritePartialSync_ThenNoWriteCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();

        await sut.WritePartialAsync(cacheItem, default);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefaultAsync();
        result.Should().BeNull();
    }

    [Fact]
    public void GivenCacheItem_WhenRemove_ThenRemoveCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        _cacheItemCollection.InsertOne(cacheItem);

        sut.Remove(cacheItem.Key!);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefault();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenCacheItem_WhenRemoveAsync_ThenRemoveCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        await _cacheItemCollection.InsertOneAsync(cacheItem);

        await sut.RemoveAsync(cacheItem.Key!, default);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefaultAsync();
        result.Should().BeNull();
    }

    [Fact]
    public void GivenCacheItem_WhenRemoveExpired_ThenRemoveCollection()
    {
        using var sut = GetSut();
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
        using var sut = GetSut(TimeSpan.FromHours(2));
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
        using var sut = GetSut(TimeSpan.FromHours(2));
        sut.RemoveExpired();
        ConfigureUtcNow(UtcNow.AddHours(3));
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
        using var sut = GetSut();
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
        using var sut = GetSut(TimeSpan.FromHours(2));
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
        using var sut = GetSut(TimeSpan.FromHours(2));
        await sut.RemoveExpiredAsync(default);
        ConfigureUtcNow(UtcNow.AddHours(3));
        (var cacheItems, var expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        await sut.RemoveExpiredAsync(default);

        using (new AssertionScope())
        {
            cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(1));
            expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key, default).Should().Be(0));
        }
    }

    private CacheItemRepository GetSut(TimeSpan? removeExpiredDelay = null)
    {
        MongoCacheOptions.RemoveExpiredDelay = removeExpiredDelay;
        return Fixture.Create<CacheItemRepository>();
    }

    private (List<CacheItem>, List<CacheItem>) ArrangeCollectionWithExpiredItem()
    {
        var cacheItems = Fixture
            .Build<CacheItem>()
            .With(i => i.ExpireAt, UtcNow.AddHours(10).Ticks)
            .CreateMany(12)
            .ToList();
        _cacheItemCollection.InsertMany(cacheItems);

        var expiredCacheItems = Fixture
            .Build<CacheItem>()
            .With(i => i.ExpireAt, UtcNow.AddHours(-10).Ticks)
            .CreateMany(12)
            .ToList();
        _cacheItemCollection.InsertMany(expiredCacheItems);

        return (cacheItems, expiredCacheItems);
    }
}