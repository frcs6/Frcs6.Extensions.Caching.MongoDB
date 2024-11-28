using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Frcs6.Extensions.Caching.MongoDB.Test.Integrated;

public class MongoCachingServicesExtensionsTest : IClassFixture<MongoDatabaseTest>
{
    private readonly MongoDatabaseTest _mongoDatabase;
    private readonly ServiceCollection _services = new();

#pragma warning disable IDE0290 // Use primary constructor
    public MongoCachingServicesExtensionsTest(MongoDatabaseTest mongoDatabase)
#pragma warning restore IDE0290 // Use primary constructor
    {
        _mongoDatabase = mongoDatabase;
    }

    [Fact]
    public void GivenConnectionString_WhenAddMongoCache_ThenAddSingleton()
    {
        _services.AddMongoCache(_mongoDatabase.GetConnectionString(), (options) =>
        {
            options.DatabaseName = BaseTest.DatabaseName;
            options.CollectionName = BaseTest.CollectionName;
        });
        _services.Count.Should().Be(8);
        AssertSingletonMongoCache();
        AssertNoCleanCacheJobs();
    }

    [Fact]
    public void GivenMongoClientSettings_WhenAddMongoCache_ThenAddSingleton()
    {
        var settings = MongoClientSettings.FromConnectionString(_mongoDatabase.GetConnectionString());
        _services.AddMongoCache(settings, options =>
        {
            options.DatabaseName = BaseTest.DatabaseName;
            options.CollectionName = BaseTest.CollectionName;
        });
        _services.Count.Should().Be(8);
        AssertSingletonMongoCache();
        AssertNoCleanCacheJobs();
    }

    [Fact]
    public void GivenMongoClient_WhenAddMongoCache_ThenAddSingleton()
    {
        _services.AddSingleton<IMongoClient>(_ => new MongoClient(_mongoDatabase.GetConnectionString()));
        _services.AddMongoCache(options =>
        {
            options.DatabaseName = BaseTest.DatabaseName;
            options.CollectionName = BaseTest.CollectionName;
        });
        _services.Count.Should().Be(8);
        AssertSingletonMongoCache();
        AssertNoCleanCacheJobs();
    }

    [Fact]
    public void GivenMongoClient_WhenAddMongoCacheWithJobs_ThenAddSingleton()
    {
        _services.AddSingleton<IMongoClient>(_ => new MongoClient(_mongoDatabase.GetConnectionString()));
        _services.AddMongoCache(options =>
        {
            options.DatabaseName = BaseTest.DatabaseName;
            options.CollectionName = BaseTest.CollectionName;
            options.RemoveExpiredDelay = TimeSpan.FromSeconds(10);
            options.UseCleanCacheJobs = true;
        });
        _services.Count.Should().Be(9);
        AssertSingletonMongoCache();
        AssertCleanCacheJobs();
    }

    private void AssertSingletonMongoCache()
    {
        var provider = _services.BuildServiceProvider();

        var cache1 = provider.GetRequiredService<IDistributedCache>();
        var cache2 = provider.GetRequiredService<IDistributedCache>();

        using (new AssertionScope())
        {
            (cache1 as MongoCache).Should().NotBeNull();
            cache1.Should().Be(cache2);
        }
    }

    private void AssertNoCleanCacheJobs()
    {
        var provider = _services.BuildServiceProvider();
        var job = provider.GetService<IHostedService>();
        (job as CleanCacheJobs).Should().BeNull();
    }

    private void AssertCleanCacheJobs()
    {
        var provider = _services.BuildServiceProvider();
        var job = provider.GetRequiredService<IHostedService>();
        (job as CleanCacheJobs).Should().NotBeNull();
    }
}