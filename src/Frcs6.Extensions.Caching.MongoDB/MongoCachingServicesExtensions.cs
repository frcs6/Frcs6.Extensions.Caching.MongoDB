using Frcs6.Extensions.Caching.MongoDB.Internal;
using Microsoft.Extensions.DependencyInjection;

#if !NET8_0_OR_GREATER
using Microsoft.Extensions.Internal;
#endif

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
                new CacheItemBuilder(DefaultTimeProvider(), mongoCacheOptions),
                new CacheItemRepository(mongoClient, DefaultTimeProvider(), mongoCacheOptions)
            );
        }));

        return services;
    }

#if NET8_0_OR_GREATER
    private static TimeProvider DefaultTimeProvider() => TimeProvider.System;
#else
    private static SystemClock DefaultTimeProvider() => new();
#endif
}