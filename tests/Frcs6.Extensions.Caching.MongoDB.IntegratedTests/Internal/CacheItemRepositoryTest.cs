using FluentAssertions.Execution;
using Frcs6.Extensions.Caching.MongoDB.Tests.Common;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Frcs6.Extensions.Caching.MongoDB.IntegratedTests.Internal;

public class CacheItemRepositoryTest : IClassFixture<MongoDatabaseFixture>
{
    private const string DatabaseName = "TestDatabase";
    private const string CollectionName = "TestCollection";

    private readonly Fixture _fixture = new();

    private DateTimeOffset _utcNow;

    private readonly IMongoCollection<CacheItem> _cacheItemCollection;
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly CacheItemRepository _sut;

    public CacheItemRepositoryTest(MongoDatabaseFixture mongoDatabase)
    {
#pragma warning disable CA1062 // Validate arguments of public methods
        var mongoClient = new MongoClient(mongoDatabase.GetConnectionString());
#pragma warning restore CA1062 // Validate arguments of public methods
        _cacheItemCollection = mongoClient.GetDatabase(DatabaseName).GetCollection<CacheItem>(CollectionName);

        ConfigureUtcNow(DateTimeOffset.UtcNow);

        var mongoCacheOptions = Options.Create(new MongoCacheOptions
        {
            DatabaseName = DatabaseName,
            CollectionName = CollectionName
        });
        _sut = new CacheItemRepository(mongoClient, _timeProvider, mongoCacheOptions);
    }

    [Fact]
    public void GiventCacheItem_WhenRead_ThenReadCollection()
    {
        var cacheItem = _fixture.Create<CacheItem>();
        var result = _sut.Read(cacheItem.Key!);
        result.Should().BeNull();

        _cacheItemCollection.InsertOne(cacheItem);
        result = _sut.Read(cacheItem.Key!);
        result.Should().BeEquivalentTo(cacheItem);
    }

    [Fact]
    public async Task GiventCacheItem_WhenReadAsync_ThenReadCollection()
    {
        var cacheItem = _fixture.Create<CacheItem>();
        var result = await _sut.ReadAsync(cacheItem.Key!, default);
        result.Should().BeNull();

        await _cacheItemCollection.InsertOneAsync(cacheItem);
        result = await _sut.ReadAsync(cacheItem.Key!, default);
        result.Should().BeEquivalentTo(cacheItem);
    }

    [Fact]
    public void GiventCacheItem_WhenWrite_ThenWriteCollection()
    {
        var cacheItem = _fixture.Create<CacheItem>();

        _sut.Write(cacheItem);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).Single();
        result.Should().BeEquivalentTo(cacheItem);
    }

    [Fact]
    public async Task GiventCacheItem_WhenWriteSync_ThenWriteCollection()
    {
        var cacheItem = _fixture.Create<CacheItem>();

        await _sut.WriteAsync(cacheItem, default);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleAsync();
        result.Should().BeEquivalentTo(cacheItem);
    }

    [Fact]
    public void GiventCacheItem_WhenReadPartial_ThenReadCollection()
    {
        var cacheItem = _fixture.Create<CacheItem>();
        var result = _sut.ReadPartial(cacheItem.Key!);
        result.Should().BeNull();

        _cacheItemCollection.InsertOne(cacheItem);

        result = _sut.ReadPartial(cacheItem.Key!);
        using (new AssertionScope())
        {
            result.Value.Should().BeNull();
            result
                .Should()
                .BeEquivalentTo(cacheItem, option => option.Excluding(x => x.Value));
        }
    }

    [Fact]
    public async Task GiventCacheItem_WhenReadPartialAsync_ThenReadCollection()
    {
        var cacheItem = _fixture.Create<CacheItem>();
        var result = await _sut.ReadPartialAsync(cacheItem.Key!, default);
        result.Should().BeNull();

        await _cacheItemCollection.InsertOneAsync(cacheItem);

        result = await _sut.ReadPartialAsync(cacheItem.Key!, default);
        using (new AssertionScope())
        {
            result.Value.Should().BeNull();
            result
                .Should()
                .BeEquivalentTo(cacheItem, option => option.Excluding(x => x.Value));
        }
    }

    [Fact]
    public void GiventCacheItem_WhenWritePartial_ThenWriteCollection()
    {
        var cacheItem = _fixture.Create<CacheItem>();
        _cacheItemCollection.InsertOne(cacheItem);
        var newCacheItem = _fixture
            .Build<CacheItem>()
            .With(i => i.Key, cacheItem.Key)
            .Without(i => i.Value)
            .Create();

        _sut.WritePartial(newCacheItem);

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
    public async Task GiventCacheItem_WhenWritePartialAsync_ThenWriteCollection()
    {
        var cacheItem = _fixture.Create<CacheItem>();
        await _cacheItemCollection.InsertOneAsync(cacheItem);
        var newCacheItem = _fixture
            .Build<CacheItem>()
            .With(i => i.Key, cacheItem.Key)
            .Without(i => i.Value)
            .Create();

        await _sut.WritePartialAsync(newCacheItem, default);

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
    public void GiventCacheItemDeleted_WhenWritePartial_ThenNoWriteCollection()
    {
        var cacheItem = _fixture.Create<CacheItem>();

        _sut.WritePartial(cacheItem);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefault();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GiventCacheItemDeleted_WhenWritePartialSync_ThenNoWriteCollection()
    {
        var cacheItem = _fixture.Create<CacheItem>();

        await _sut.WritePartialAsync(cacheItem, default);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefaultAsync();
        result.Should().BeNull();
    }

    [Fact]
    public void GiventCacheItem_WhenRemove_ThenRemoveCollection()
    {
        var cacheItem = _fixture.Create<CacheItem>();
        _cacheItemCollection.InsertOne(cacheItem);

        _sut.Remove(cacheItem.Key!);

        var result = _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefault();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GiventCacheItem_WhenRemoveAsync_ThenRemoveCollection()
    {
        var cacheItem = _fixture.Create<CacheItem>();
        await _cacheItemCollection.InsertOneAsync(cacheItem);

        await _sut.RemoveAsync(cacheItem.Key!, default);

        var result = await _cacheItemCollection.Find(i => i.Key == cacheItem.Key).SingleOrDefaultAsync();
        result.Should().BeNull();
    }

    [Fact]
    public void GiventCacheItem_WhenRemoveExpired_ThenRemoveCollection()
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

        _sut.RemoveExpired();

        using (new AssertionScope())
        {
            foreach (var cacheItem in cacheItems)
                _cacheItemCollection.CountDocuments(i => i.Key == cacheItem.Key, default).Should().Be(1);

            foreach (var cacheItem in expiredCacheItems)
                _cacheItemCollection.CountDocuments(i => i.Key == cacheItem.Key, default).Should().Be(0);
        }
    }

    private void ConfigureUtcNow(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
        _timeProvider.SetUtcNow(utcNow);
    }
}