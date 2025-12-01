using Frcs6.Extensions.Caching.MongoDB;
using Frcs6.Extensions.Caching.MongoDB.Test.Base;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

using var mongoDatabase = new MongoDatabaseTest();
var serviceProvider = new ServiceCollection().AddMongoCache(mongoDatabase.GetConnectionString(), options =>
{
    options.DatabaseName = "mongocache-examples";
    options.CollectionName = "mongocache-consoleapp";
    options.RemoveExpiredDelay = TimeSpan.FromSeconds(10);
}).BuildServiceProvider();

var cache = serviceProvider.GetService<IDistributedCache>();
ArgumentNullException.ThrowIfNull(cache);

const string key = "key";
var value = Guid.NewGuid().ToString();
cache.SetString(key, value, new DistributedCacheEntryOptions
{
    SlidingExpiration = TimeSpan.FromSeconds(60)
});
value = cache.GetString(key);

Console.WriteLine($"Cached value: '{value}'");