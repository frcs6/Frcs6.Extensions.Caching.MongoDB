using Microsoft.Extensions.Options;

namespace Frcs6.Extensions.Caching.MongoDB.Tests;

public class BaseTest
{
    protected const string DatabaseName = "TestDatabase";
    protected const string CollectionName = "CacheCollection";

    protected Fixture Fixture { get; } = new();
    protected string DefaultKey { get; }
    protected byte[] DefaultValue { get; }

    protected BaseTest()
    {
        DefaultKey = Fixture.Create<string>();
        DefaultValue = Fixture.CreateMany<byte>().ToArray();
    }

    protected static IOptions<MongoCacheOptions> BuildMongoCacheOptions()
        => Options.Create(new MongoCacheOptions { DatabaseName = DatabaseName, CollectionName = CollectionName });
}