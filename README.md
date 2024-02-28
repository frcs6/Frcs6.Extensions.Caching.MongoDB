# Frcs6.Extensions.Caching.MongoDB

[![NuGet](https://img.shields.io/nuget/v/Frcs6.Extensions.Caching.MongoDB.svg)](https://www.nuget.org/packages/Frcs6.Extensions.Caching.MongoDB)
[![CI/CD](https://github.com/frcs6/Frcs6.Extensions.Caching.MongoDB/actions/workflows/build_release.yml/badge.svg)](https://github.com/frcs6/Frcs6.Extensions.Caching.MongoDB/actions/workflows/build_release.yml)
[![Coverage](https://codecov.io/gh/frcs6/Frcs6.Extensions.Caching.MongoDB/graph/badge.svg?token=5RBQZ75VTR)](https://codecov.io/gh/frcs6/Frcs6.Extensions.Caching.MongoDB)
[![Mutation score](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Ffrcs6%2FFrcs6.Extensions.Caching.MongoDB%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/frcs6/Frcs6.Extensions.Caching.MongoDB/main)

[Distributed cache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed) implemented with [MongoDB](https://www.mongodb.com/) using [Official .NET driver for MongoDB](https://www.nuget.org/packages/MongoDB.Driver).

This implementation is based on the official version for Sql Server and Redis available [here](https://github.com/dotnet/aspnetcore/tree/main/src/Caching).

## Installation

- Add package [Frcs6.Extensions.Caching.MongoDB](https://www.nuget.org/packages/Frcs6.Extensions.Caching.MongoDB/).
- Inject MongoCache using ```MongoCachingServicesExtensions.AddMongoCache``` method.
- Use ```IDistributedCache``` where you need it.

Some examples are available [here](./examples/).

## Configuration

You can configure database connection in 3 ways :

- With connection string.
- By passing a ```MongoClientSettings```.
- By passing a ```IMongoClient```.

```cssharp
const string connectionString = "mongodb://localhost:27017";
builder.Services.AddMongoCache(connectionString, options =>
{
    options.DatabaseName = "MyCacheDatabase";
    options.CollectionName = "MyCacheCollection";
    options.RemoveExpiredDelay = TimeSpan.FromSeconds(10);
});
```

MongoClientSettings can be useful if you need to pass a certificate. 

```cssharp
var cert = new X509Certificate2("client.p12", "mySuperSecretPassword");
var settings = new MongoClientSettings
{
   SslSettings = new SslSettings
   {
      ClientCertificates = new[] { cert }
   },
   UseTls = true
};

builder.Services.AddMongoCache(settings, options =>
{
    options.DatabaseName = "MyCacheDatabase";
    options.CollectionName = "MyCacheCollection";
    options.RemoveExpiredDelay = TimeSpan.FromSeconds(10);
});
```

You can read [Official Mongo documentation](https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/connection/tls/).

## Cache option

- DatabaseName: Name of the cache database.
- CollectionName: Name of the cache collection.
- AllowNoExpiration: Allow item without expiration.
- RemoveExpiredDelay: Delay between each cache clean.

## Cache cleanup

Cache cleaning will be launched on each cache call (Get, Set, Refresh) if the timeout is reached.
