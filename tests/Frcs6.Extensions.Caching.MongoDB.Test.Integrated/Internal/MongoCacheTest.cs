using Microsoft.Extensions.Caching.Distributed;

namespace Frcs6.Extensions.Caching.MongoDB.Test.Integrated.Internal;

public sealed class MongoCacheTest : BaseTest, IClassFixture<MongoDatabaseTest>, IDisposable
{
    private readonly MongoClient _mongoClient;
    private CacheItemRepository? _cacheItemRepository;

    public MongoCacheTest(MongoDatabaseTest mongoDatabase)
    {
        ArgumentNullException.ThrowIfNull(mongoDatabase);
        _mongoClient = new MongoClient(mongoDatabase.GetConnectionString());
    }

    public void Dispose()
    {
        _mongoClient.Dispose();
        _cacheItemRepository?.Dispose();
    }

    [Fact]
    public void CacheShouldKeepKeyValueWithNoExpiration()
    {
        var cache = GetMongoCache(options =>
        {
            options.AllowNoExpiration = true;
            options.RemoveExpiredDelay = TimeSpan.FromSeconds(10);
        });

        var key1 = Fixture.Create<string>();
        var value1 = Fixture.Create<string>();

        var key2 = Fixture.Create<string>();
        var value2 = Fixture.Create<string>();

        cache.SetString(key1, value1);
        cache.SetString(key2, value2,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20) });

        cache.GetString(key1).Should().Be(value1);
        cache.GetString(key2).Should().Be(value2);

        ConfigureUtcNow(UtcNow.AddMinutes(1));

        cache.GetString(key1).Should().Be(value1);
        cache.GetString(key2).Should().Be(null);

        cache.Remove(key1);
        cache.GetString(key1).Should().Be(null);
    }

    [Fact]
    public async Task CacheShouldKeepKeyValueWithNoExpirationAsync()
    {
        var cache = GetMongoCache(options =>
        {
            options.AllowNoExpiration = true;
            options.RemoveExpiredDelay = TimeSpan.FromSeconds(10);
        });

        var key1 = Fixture.Create<string>();
        var value1 = Fixture.Create<string>();

        var key2 = Fixture.Create<string>();
        var value2 = Fixture.Create<string>();

        await cache.SetStringAsync(key1, value1);
        await cache.SetStringAsync(key2, value2,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20) });

        (await cache.GetStringAsync(key1)).Should().Be(value1);
        (await cache.GetStringAsync(key2)).Should().Be(value2);

        ConfigureUtcNow(UtcNow.AddMinutes(1));

        (await cache.GetStringAsync(key1)).Should().Be(value1);
        (await cache.GetStringAsync(key2)).Should().Be(null);

        await cache.RemoveAsync(key1);
        (await cache.GetStringAsync(key1)).Should().Be(null);
    }

    [Fact]
    public void CacheShouldRefreshWithSlidingExpiration()
    {
        var cache = GetMongoCache(options =>
        {
            options.AllowNoExpiration = true;
            options.RemoveExpiredDelay = TimeSpan.FromSeconds(10);
        });

        var key = Fixture.Create<string>();
        var value = Fixture.Create<string>();

        cache.SetString(key, value, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(20) });

        for (int i = 0; i < 5; ++i)
        {
            ConfigureUtcNow(UtcNow.AddSeconds(10));
            cache.Refresh(key);
        }

        ConfigureUtcNow(UtcNow.AddMinutes(1));
        cache.GetString(key).Should().Be(null);
    }

    [Fact]
    public async Task CacheShouldRefreshWithSlidingExpirationAsync()
    {
        var cache = GetMongoCache(options =>
        {
            options.AllowNoExpiration = true;
            options.RemoveExpiredDelay = TimeSpan.FromSeconds(10);
        });

        var key = Fixture.Create<string>();
        var value = Fixture.Create<string>();

        await cache.SetStringAsync(key, value,
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(20) });

        for (int i = 0; i < 5; ++i)
        {
            ConfigureUtcNow(UtcNow.AddSeconds(10));
            await cache.RefreshAsync(key);
        }

        ConfigureUtcNow(UtcNow.AddMinutes(1));
        (await cache.GetStringAsync(key)).Should().Be(null);
    }

    private MongoCache GetMongoCache(Action<MongoCacheOptions> setupAction)
    {
        setupAction(MongoCacheOptions);
        _cacheItemRepository = new CacheItemRepository(_mongoClient, TimeProvider, MongoCacheOptions);
        return new MongoCache(new CacheItemBuilder(TimeProvider, MongoCacheOptions), _cacheItemRepository);
    }
}