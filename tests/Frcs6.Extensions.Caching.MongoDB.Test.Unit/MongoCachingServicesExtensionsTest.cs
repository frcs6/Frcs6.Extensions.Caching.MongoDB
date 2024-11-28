using Microsoft.Extensions.DependencyInjection;

namespace Frcs6.Extensions.Caching.MongoDB.Test.Unit;

public class MongoCachingServicesExtensionsTest : BaseTest
{
    private const string MongoConnectionString = "mongodb://localhost:27017";

    private readonly IServiceCollection _testService = new TestServiceCollection();

    [Fact]
    public void GivenNullServices_WhenAddMongoCache_ThenThrow()
    {
        var act = () => MongoCachingServicesExtensions.AddMongoCache(null!, MongoConnectionString, _ => { });
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenNullActions_WhenAddMongoCache_ThenThrow()
    {
        var act = () => _testService.AddMongoCache(MongoConnectionString, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    private sealed class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
    }
}