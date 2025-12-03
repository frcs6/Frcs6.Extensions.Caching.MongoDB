namespace Frcs6.Extensions.Caching.MongoDB.Test.Integrated.Internal;

#pragma warning disable CA1063
public class CacheItemRepositoryTest : BaseTest, IClassFixture<MongoDatabaseTest>, IDisposable
#pragma warning restore CA1063
{
    private readonly MongoClient _mongoClient;
    private readonly IMongoCollection<CacheItem> _cacheItemCollection;

    public CacheItemRepositoryTest(MongoDatabaseTest mongoDatabase)
    {
        ArgumentNullException.ThrowIfNull(mongoDatabase);
        _mongoClient = new MongoClient(mongoDatabase.GetConnectionString());
        Fixture.Register<IMongoClient>(() => _mongoClient);
        _cacheItemCollection = _mongoClient.GetDatabase(DatabaseName).GetCollection<CacheItem>(CollectionName);
    }

#pragma warning disable CA1816
#pragma warning disable CA1063
    public void Dispose()
#pragma warning restore CA1063
#pragma warning restore CA1816
    {
        _mongoClient.Dispose();
    }

    [Fact]
    public void GivenCacheItem_WhenRead_ThenReadCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        cacheItem.Value = cacheItem.Value!.ToList();
        var result = sut.Read(cacheItem.Key!);
        result.ShouldBeNull();

        _cacheItemCollection.InsertOne(cacheItem);
        result = sut.Read(cacheItem.Key!);
        result.ShouldBeEquivalentTo(cacheItem);
    }

    [Fact]
    public async Task GivenCacheItem_WhenReadAsync_ThenReadCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        cacheItem.Value = cacheItem.Value!.ToList();
        var result = await sut.ReadAsync(cacheItem.Key!, CancellationToken.None);
        result.ShouldBeNull();

        await _cacheItemCollection.InsertOneAsync(cacheItem);
        result = await sut.ReadAsync(cacheItem.Key!, CancellationToken.None);
        result.ShouldBeEquivalentTo(cacheItem);
    }

    [Fact]
    public void GivenCacheItem_WhenWrite_ThenWriteCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        cacheItem.Value = cacheItem.Value!.ToList();

        sut.Write(cacheItem);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).Single();
        result.ShouldBeEquivalentTo(cacheItem);
    }

    [Fact]
    public async Task GivenCacheItem_WhenWriteSync_ThenWriteCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        cacheItem.Value = cacheItem.Value!.ToList();

        await sut.WriteAsync(cacheItem, CancellationToken.None);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleAsync();
        result.ShouldBeEquivalentTo(cacheItem);
    }

    [Fact]
    public void GivenCacheItem_WhenReadPartial_ThenReadCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        var result = sut.ReadPartial(cacheItem.Key!);
        result.ShouldBeNull();

        _cacheItemCollection.InsertOne(cacheItem);

        result = sut.ReadPartial(cacheItem.Key!);
        result.ShouldNotBeNull();
        result.Key.ShouldBe(cacheItem.Key);
        result.Value.ShouldBeNull();
        result.AbsoluteExpiration.ShouldBe(cacheItem.AbsoluteExpiration);
        result.ExpireAt.ShouldBe(cacheItem.ExpireAt);
        result.SlidingExpiration.ShouldBe(cacheItem.SlidingExpiration);
    }

    [Fact]
    public async Task GivenCacheItem_WhenReadPartialAsync_ThenReadCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        var result = await sut.ReadPartialAsync(cacheItem.Key!, CancellationToken.None);
        result.ShouldBeNull();

        await _cacheItemCollection.InsertOneAsync(cacheItem);

        result = await sut.ReadPartialAsync(cacheItem.Key!, CancellationToken.None);
        result.ShouldNotBeNull();
        result.Key.ShouldBe(cacheItem.Key);
        result.Value.ShouldBeNull();
        result.AbsoluteExpiration.ShouldBe(cacheItem.AbsoluteExpiration);
        result.ExpireAt.ShouldBe(cacheItem.ExpireAt);
        result.SlidingExpiration.ShouldBe(cacheItem.SlidingExpiration);
        
    }

    [Fact]
    public void GivenCacheItem_WhenWritePartial_ThenWriteCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        cacheItem.Value = cacheItem.Value!.ToList();
        _cacheItemCollection.InsertOne(cacheItem);
        var newCacheItem = Fixture
            .Build<CacheItem>()
            .With(i => i.Key, cacheItem.Key)
            .Without(i => i.Value)
            .Create();

        sut.WritePartial(newCacheItem);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).Single();
        result.ShouldNotBeNull();
        result.Key.ShouldBe(cacheItem.Key);
        result.Value.ShouldBeEquivalentTo(cacheItem.Value);
        result.AbsoluteExpiration.ShouldNotBe(cacheItem.AbsoluteExpiration);
        result.ExpireAt.ShouldNotBe(cacheItem.ExpireAt);
        result.SlidingExpiration.ShouldNotBe(cacheItem.SlidingExpiration);
    }

    [Fact]
    public async Task GivenCacheItem_WhenWritePartialAsync_ThenWriteCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        cacheItem.Value = cacheItem.Value!.ToList();
        await _cacheItemCollection.InsertOneAsync(cacheItem);
        var newCacheItem = Fixture
            .Build<CacheItem>()
            .With(i => i.Key, cacheItem.Key)
            .Without(i => i.Value)
            .Create();

        await sut.WritePartialAsync(newCacheItem, CancellationToken.None);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleAsync();
        result.ShouldNotBeNull();
        result.Key.ShouldBe(cacheItem.Key);
        result.Value.ShouldBeEquivalentTo(cacheItem.Value);
        result.AbsoluteExpiration.ShouldNotBe(cacheItem.AbsoluteExpiration);
        result.ExpireAt.ShouldNotBe(cacheItem.ExpireAt);
        result.SlidingExpiration.ShouldNotBe(cacheItem.SlidingExpiration);
    }

    [Fact]
    public void GivenCacheItemDeleted_WhenWritePartial_ThenNoWriteCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();

        sut.WritePartial(cacheItem);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefault();
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GivenCacheItemDeleted_WhenWritePartialSync_ThenNoWriteCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();

        await sut.WritePartialAsync(cacheItem, CancellationToken.None);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefaultAsync();
        result.ShouldBeNull();
    }

    [Fact]
    public void GivenCacheItem_WhenRemove_ThenRemoveCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        _cacheItemCollection.InsertOne(cacheItem);

        sut.Remove(cacheItem.Key!);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefault();
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GivenCacheItem_WhenRemoveAsync_ThenRemoveCollection()
    {
        using var sut = GetSut();
        var cacheItem = Fixture.Create<CacheItem>();
        await _cacheItemCollection.InsertOneAsync(cacheItem);

        await sut.RemoveAsync(cacheItem.Key!, CancellationToken.None);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefaultAsync();
        result.ShouldBeNull();
    }

    [Fact]
    public void GivenCacheItem_WhenRemoveExpired_ThenRemoveCollection()
    {
        using var sut = GetSut();
        var (cacheItems, expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        sut.RemoveExpired();

        cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(1));
        expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(0));
    }

    [Fact]
    public void GivenRemoveExpiredDelayNotReach_WhenRemoveExpired_ThenKeepCollection()
    {
        using var sut = GetSut(TimeSpan.FromHours(2));
        sut.RemoveExpired();
        var (cacheItems, expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        sut.RemoveExpired();

        cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(1));
        expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(1));
    }

    [Fact]
    public void GivenRemoveExpiredDelayReach_WhenRemoveExpired_ThenRemoveCollection()
    {
        using var sut = GetSut(TimeSpan.FromHours(2));
        sut.RemoveExpired();
        ConfigureUtcNow(UtcNow.AddHours(3));
        var (cacheItems, expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        sut.RemoveExpired();

        cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(1));
        expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(0));
    }

    [Fact]
    public void GivenRemoveExpiredDelayNotReachButForce_WhenRemoveExpired_ThenKeepCollection()
    {
        using var sut = GetSut(TimeSpan.FromHours(2));
        sut.RemoveExpired();
        var (cacheItems, expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        sut.RemoveExpired(true);

        cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(1));
        expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(0));
    }

    [Fact]
    public async Task GivenCacheItem_WhenRemoveExpiredAsync_ThenRemoveCollection()
    {
        using var sut = GetSut();
        var (cacheItems, expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        await sut.RemoveExpiredAsync(CancellationToken.None);

        cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(1));
        expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(0));
    }

    [Fact]
    public async Task GivenRemoveExpiredDelayNotReach_WhenRemoveExpiredAsync_ThenKeepCollection()
    {
        using var sut = GetSut(TimeSpan.FromHours(2));
        await sut.RemoveExpiredAsync(CancellationToken.None);
        var (cacheItems, expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        await sut.RemoveExpiredAsync(CancellationToken.None);

        cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(1));
        expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(1));
    }

    [Fact]
    public async Task GivenRemoveExpiredDelayReach_WhenRemoveExpiredAsync_ThenRemoveCollection()
    {
        using var sut = GetSut(TimeSpan.FromHours(2));
        await sut.RemoveExpiredAsync(CancellationToken.None);
        ConfigureUtcNow(UtcNow.AddHours(3));
        var (cacheItems, expiredCacheItems) = ArrangeCollectionWithExpiredItem();

        await sut.RemoveExpiredAsync(CancellationToken.None);

        cacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(1));
        expiredCacheItems.ForEach(c1 => _cacheItemCollection.CountDocuments(c2 => c2.Key == c1.Key).ShouldBe(0));
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