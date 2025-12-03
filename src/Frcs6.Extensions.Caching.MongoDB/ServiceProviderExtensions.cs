using Microsoft.Extensions.DependencyInjection;

namespace Frcs6.Extensions.Caching.MongoDB;

internal static class ServiceProviderExtensions
{
    extension(IServiceProvider serviceProvider)
    {
        [ExcludeFromCodeCoverage]
        internal IMongoClient GetMongoClient() =>
            serviceProvider.GetService<IMongoClient>() ??
            throw new InvalidOperationException("No MongoClient found.");

        [ExcludeFromCodeCoverage]
        internal IOptions<MongoCacheOptions> GetMongoCacheOptions() =>
            serviceProvider.GetService<IOptions<MongoCacheOptions>>() ??
            throw new InvalidOperationException("No MongoCache options found.");
    }
}