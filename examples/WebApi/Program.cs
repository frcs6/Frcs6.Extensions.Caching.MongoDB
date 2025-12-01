using Frcs6.Extensions.Caching.MongoDB;
using Frcs6.Extensions.Caching.MongoDB.Test.Base;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

using var mongoDatabase = new MongoDatabaseTest();
builder.Services.AddMongoCache(mongoDatabase.GetConnectionString(), options =>
{
    options.DatabaseName = "mongocache-examples";
    options.CollectionName = "mongocache-webapi";
    options.RemoveExpiredDelay = TimeSpan.FromSeconds(10);
    options.UseCleanCacheJobs = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.MapGet("/generate", (IDistributedCache cache) =>
{
    var key = Guid.NewGuid().ToString();
    var value = cache.GetString(key);
    if (value != null) return new CacheData(true, key, value);
    value = Guid.NewGuid().ToString();
    cache.SetString(key, value, new DistributedCacheEntryOptions 
    { 
        SlidingExpiration = TimeSpan.FromSeconds(60) 
    });
    return new CacheData(false, key, value);
})
.WithName("Generate");

app.MapGet("/{key}", (string key, IDistributedCache cache) =>
{
    var value = cache.GetString(key);
    return new CacheData(value != null, key, value!);
})
.WithName("GetByKey");

app.Run();

internal record CacheData(bool FromCache, string Key, string Value);