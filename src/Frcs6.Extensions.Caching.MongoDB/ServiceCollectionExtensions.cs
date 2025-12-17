using Frcs6.Extensions.Caching.MongoDB.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Frcs6.Extensions.Caching.MongoDB;

/// <summary>
/// Service collection extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="services">Service collection.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Register cache.
        /// </summary>
        /// <param name="mongoConnectionString">Mongo connection string.</param>
        /// <param name="setupAction">Options configuration actions.</param>
        /// <returns>Service collection.</returns>
        public IServiceCollection AddMongoCache(string mongoConnectionString,
            Action<MongoCacheOptions> setupAction)
        {
            return services
                .AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString))
                .AddMongoCache(setupAction);
        }

        /// <summary>
        /// Register cache.
        /// </summary>
        /// <param name="mongoClientSettings">Mongo connection settings.</param>
        /// <param name="setupAction">Options configuration actions.</param>
        /// <returns>Service collection.</returns>
        public IServiceCollection AddMongoCache(MongoClientSettings mongoClientSettings,
            Action<MongoCacheOptions> setupAction)
        {
            return services
                .AddSingleton<IMongoClient>(_ => new MongoClient(mongoClientSettings))
                .AddMongoCache(setupAction);
        }

        /// <summary>
        /// Register cache.
        /// </summary>
        /// <remarks>
        /// <see cref="IMongoClient"/> must be available in <see cref="IServiceCollection"/>.
        /// </remarks>
        /// <param name="setupAction">Options configuration actions.</param>
        /// <returns>Service collection.</returns>
        public IServiceCollection AddMongoCache(Action<MongoCacheOptions> setupAction)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(setupAction);

            services.AddOptions();
            services.Configure(setupAction);

            services.Add(ServiceDescriptor.Singleton<IDistributedCache, MongoCache>(serviceProvider =>
            {
                var mongoCacheOptions = GetMongoCacheOptions(serviceProvider);
                var mongoClient = GetMongoClient(serviceProvider);
                return new MongoCache(
                    new CacheItemBuilder(DefaultTimeProvider(), mongoCacheOptions),
                    new CacheItemRepository(mongoClient, DefaultTimeProvider(), mongoCacheOptions)
                );
            }));

            var mongoCacheOptions = new MongoCacheOptions();
            setupAction(mongoCacheOptions);

            if (mongoCacheOptions.UseCleanCacheJobs)
            {
                services.AddHostedService((serviceProvider) =>
                {
                    var cacheOptions = GetMongoCacheOptions(serviceProvider);
                    var mongoClient = GetMongoClient(serviceProvider);
                    return new CleanCacheJobs(
                        new CacheItemRepository(mongoClient, DefaultTimeProvider(), cacheOptions),
                        cacheOptions);
                });
            }

            return services;
        }
    }

    private static TimeProvider DefaultTimeProvider() => TimeProvider.System;
    
    private static IMongoClient GetMongoClient(IServiceProvider serviceProvider) =>
             serviceProvider.GetService<IMongoClient>() ??
             throw new InvalidOperationException("No MongoClient found.");

    private static IOptions<MongoCacheOptions> GetMongoCacheOptions(IServiceProvider serviceProvider) =>
             serviceProvider.GetService<IOptions<MongoCacheOptions>>() ??
             throw new InvalidOperationException("No MongoCache options found.");
}