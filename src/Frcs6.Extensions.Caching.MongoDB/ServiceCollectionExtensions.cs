// ReSharper disable ConvertToExtensionBlock
using Frcs6.Extensions.Caching.MongoDB.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Frcs6.Extensions.Caching.MongoDB;

/// <summary>
/// Service collection extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register cache.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="mongoConnectionString">Mongo connection string.</param>
    /// <param name="setupAction">Options configuration actions.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddMongoCache(
        this IServiceCollection services,
        string mongoConnectionString,
        Action<MongoCacheOptions> setupAction)
    {
        return services
            .AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString))
            .AddMongoCache(setupAction);
    }

    /// <summary>
    /// Register cache.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="mongoClientSettings">Mongo connection settings.</param>
    /// <param name="setupAction">Options configuration actions.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddMongoCache(
        this IServiceCollection services,
        MongoClientSettings mongoClientSettings,
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
    /// <param name="services">Service collection.</param>
    /// <param name="setupAction">Options configuration actions.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddMongoCache(
        this IServiceCollection services,
        Action<MongoCacheOptions> setupAction)
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

    private static TimeProvider DefaultTimeProvider() => TimeProvider.System;
    
    [ExcludeFromCodeCoverage]
    private static IMongoClient GetMongoClient(IServiceProvider serviceProvider) =>
             serviceProvider.GetService<IMongoClient>() ??
             throw new InvalidOperationException("No MongoClient found.");

    [ExcludeFromCodeCoverage]
    private static IOptions<MongoCacheOptions> GetMongoCacheOptions(IServiceProvider serviceProvider) =>
             serviceProvider.GetService<IOptions<MongoCacheOptions>>() ??
             throw new InvalidOperationException("No MongoCache options found.");
}