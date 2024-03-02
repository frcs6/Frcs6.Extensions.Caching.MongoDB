using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Frcs6.Extensions.Caching.MongoDB.Test.Integrated;

public class MongoCachingServicesExtensionsTest : IClassFixture<MongoDatabaseTest>
{
    private readonly MongoDatabaseTest _mongoDatabase;
    private readonly IServiceCollection _services = new ServiceCollection();

#pragma warning disable IDE0290 // Use primary constructor
    public MongoCachingServicesExtensionsTest(MongoDatabaseTest mongoDatabase)
#pragma warning restore IDE0290 // Use primary constructor
    {
        _mongoDatabase = mongoDatabase;
    }

    [Fact]
    public void GivenNullServices_WhenAddMongoCache_ThenThrow()
    {
        var act = () => MongoCachingServicesExtensions.AddMongoCache(null!, _mongoDatabase.GetConnectionString(), (options) => { });
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullActions_WhenAddMongoCache_ThenThrow()
    {
        var act = () => MongoCachingServicesExtensions.AddMongoCache(_services, _mongoDatabase.GetConnectionString(), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenConnectionString_WhenAddMongoCache_ThenAddSingleton()
    {
        _services.AddMongoCache(_mongoDatabase.GetConnectionString(), (options) =>
        {
            options.DatabaseName = BaseTest.DatabaseName;
            options.CollectionName = BaseTest.CollectionName;
        });
        AssertSingletonMongoCache();
    }

    [Fact]
    public void GivenMongoClientSettings_WhenAddMongoCache_ThenAddSingleton()
    {
        var settings = MongoClientSettings.FromConnectionString(_mongoDatabase.GetConnectionString());
        _services.AddMongoCache(settings, (options) =>
        {
            options.DatabaseName = BaseTest.DatabaseName;
            options.CollectionName = BaseTest.CollectionName;
        });
        AssertSingletonMongoCache();
    }

    [Fact]
    public void GivenMongoClient_WhenAddMongoCache_ThenAddSingleton()
    {
        var client = new MongoClient(_mongoDatabase.GetConnectionString());
        _services.AddMongoCache(client, (options) =>
        {
            options.DatabaseName = BaseTest.DatabaseName;
            options.CollectionName = BaseTest.CollectionName;
        });
        AssertSingletonMongoCache();
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
}