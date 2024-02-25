using Frcs6.Extensions.Caching.MongoDB.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Frcs6.Extensions.Caching.MongoDB;

public static class MongoCachingServicesExtensions
{
    public static IServiceCollection AddMongoCache(this IServiceCollection services, string mongoConnectionString, Action<MongoCacheOptions> setupAction)
        => services.AddMongoCache(new MongoClient(mongoConnectionString), setupAction);

    public static IServiceCollection AddMongoCache(this IServiceCollection services, MongoClientSettings mongoClientSettings, Action<MongoCacheOptions> setupAction)
        => services.AddMongoCache(new MongoClient(mongoClientSettings), setupAction);

    public static IServiceCollection AddMongoCache(this IServiceCollection services, IMongoClient mongoClient, Action<MongoCacheOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(setupAction);

        services.AddOptions();
        services.Configure(setupAction);
        services.Add(ServiceDescriptor.Singleton<IDistributedCache, MongoCache>((serviceProvider) =>
        {
            var mongoCacheOptions = serviceProvider.GetService<IOptions<MongoCacheOptions>>() ??
                                    throw new InvalidOperationException("No MongoCache options found.");

            return new MongoCache(
                new CacheItemBuilder(
#if NET8_0_OR_GREATER
                    TimeProvider.System,
#else
                    new SystemClock(),
#endif
                    mongoCacheOptions),
                new CacheItemRepository(
                    mongoClient,
#if NET8_0_OR_GREATER
                    TimeProvider.System,
#else
                    new SystemClock(),
#endif
                    new SharedContext(mongoCacheOptions),
                    mongoCacheOptions)
            );
        }));

        return services;
    }
}