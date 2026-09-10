namespace Frcs6.Extensions.Caching.MongoDB.Test.Unit;

public class MongoCacheOptionsTest
{
    [Fact]
    public void GivenMongoCacheOptions_WhenCreateInstance_ThenAllPropertiesHaveDefaultValues()
    {
        var options = new MongoCacheOptions();

        options.DatabaseName.ShouldBeNull();
        options.CollectionName.ShouldBeNull();
        options.AllowNoExpiration.ShouldBeFalse();
        options.RemoveExpiredDelay.ShouldBeNull();
        options.UseCleanCacheJobs.ShouldBeFalse();
    }

    [Fact]
    public void GivenMongoCacheOptions_WhenSetDatabaseName_ThenDatabaseNameIsSet()
    {
        var options = new MongoCacheOptions();
        var databaseName = "CacheDb";

        options.DatabaseName = databaseName;

        options.DatabaseName.ShouldBe(databaseName);
    }

    [Fact]
    public void GivenMongoCacheOptions_WhenSetCollectionName_ThenCollectionNameIsSet()
    {
        var options = new MongoCacheOptions();
        var collectionName = "CacheCollection";

        options.CollectionName = collectionName;

        options.CollectionName.ShouldBe(collectionName);
    }

    [Fact]
    public void GivenMongoCacheOptions_WhenSetAllowNoExpiration_ThenAllowNoExpirationIsSet()
    {
        var options = new MongoCacheOptions
        {
            AllowNoExpiration = true
        };

        options.AllowNoExpiration.ShouldBeTrue();
    }

    [Fact]
    public void GivenMongoCacheOptions_WhenSetRemoveExpiredDelay_ThenRemoveExpiredDelayIsSet()
    {
        var options = new MongoCacheOptions();
        var delay = TimeSpan.FromSeconds(30);

        options.RemoveExpiredDelay = delay;

        options.RemoveExpiredDelay.ShouldBe(delay);
    }

    [Fact]
    public void GivenMongoCacheOptions_WhenSetUseCleanCacheJobs_ThenUseCleanCacheJobsIsSet()
    {
        var options = new MongoCacheOptions
        {
            UseCleanCacheJobs = true
        };

        options.UseCleanCacheJobs.ShouldBeTrue();
    }

    [Fact]
    public void GivenMongoCacheOptions_WhenGetValueProperty_ThenReturnsSelf()
    {
        var options = new MongoCacheOptions { DatabaseName = "TestDb", CollectionName = "TestCollection" };

        var result = ((IOptions<MongoCacheOptions>)options).Value;

        result.ShouldBe(options);
        result.DatabaseName.ShouldBe("TestDb");
        result.CollectionName.ShouldBe("TestCollection");
    }
}
