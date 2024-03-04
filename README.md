# Frcs6.Extensions.Caching.MongoDB

[![NuGet](https://img.shields.io/nuget/v/Frcs6.Extensions.Caching.MongoDB.svg)](https://www.nuget.org/packages/Frcs6.Extensions.Caching.MongoDB)
[![CI/CD](https://github.com/frcs6/Frcs6.Extensions.Caching.MongoDB/actions/workflows/build_release.yml/badge.svg)](https://github.com/frcs6/Frcs6.Extensions.Caching.MongoDB/actions/workflows/build_release.yml)
[![Coverage](https://codecov.io/gh/frcs6/Frcs6.Extensions.Caching.MongoDB/graph/badge.svg?token=5RBQZ75VTR)](https://codecov.io/gh/frcs6/Frcs6.Extensions.Caching.MongoDB)
[![Mutation score](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Ffrcs6%2FFrcs6.Extensions.Caching.MongoDB%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/frcs6/Frcs6.Extensions.Caching.MongoDB/main)

[Distributed cache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed) implemented with [MongoDB](https://www.mongodb.com/) using [Official .NET driver for MongoDB](https://www.nuget.org/packages/MongoDB.Driver).

This implementation is based on the official version for Sql Server and Redis available [here](https://github.com/dotnet/aspnetcore/tree/main/src/Caching).

## Installation / Usage

- Add package [Frcs6.Extensions.Caching.MongoDB](https://www.nuget.org/packages/Frcs6.Extensions.Caching.MongoDB/).
- Inject Mongo cache using ```MongoCachingServicesExtensions.AddMongoCache```.
- Use ```IDistributedCache``` where you need it.

Some examples are available [here](./examples/).

## MongoCache injection

You cand inject Mongo cache using ```MongoCachingServicesExtensions.AddMongoCache``` method with one of these parameters :
  - ```ConnectionString```.
  - ```MongoClientSettings```.
  - ```IMongoClient```.

```MongoClientSettings``` can be useful if you need to pass a certificate. You can read the official Mongo documentation [Enable TLS on a Connection](https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/connection/tls/).

### Examples

#### With connection string

```cs
const string connectionString = "mongodb://localhost:27017";
builder.Services.AddMongoCache(connectionString, options =>
{
    options.DatabaseName = "MyCacheDatabase";
    options.CollectionName = "MyCacheCollection";
    options.RemoveExpiredDelay = TimeSpan.FromSeconds(10);
});
```

#### With MongoClientSettings

```cs
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

## MongoCacheOptions

- ```DatabaseName```: Name of the cache database (**required**).
- ```CollectionName```: Name of the cache collection (**required**).
- ```AllowNoExpiration```: Allow item without expiration (**default false**).
- ```RemoveExpiredDelay```: Delay between each cache clean (**default null**).

## Removing expired elements

Removing expired elements is automatic. The only option you have to set is ```RemoveExpiredDelay```.

If ```RemoveExpiredDelay``` is not set, cleaning will launch on each cache access (Get, Set, Refresh).

**TODO**: Add a [jobs](https://github.com/frcs6/Frcs6.Extensions.Caching.MongoDB/issues/38) to remove expired item.
