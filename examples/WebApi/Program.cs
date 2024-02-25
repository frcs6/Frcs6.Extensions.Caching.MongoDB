using Frcs6.Extensions.Caching.MongoDB;
using Frcs6.Extensions.Caching.MongoDB.Test.Base;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

using var mongoDatabase = new MongoDatabaseTest();
builder.Services.AddMongoCache(mongoDatabase.GetConnectionString(), options =>
{
    options.DatabaseName = "mongocache-examples";
    options.CollectionName = "mongocache-webapi";
    options.RemoveExpiredDelay = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/generate", (IDistributedCache cache) =>
{
    const string key = "key";
    var value = cache.GetString(key);
    if (value == null)
    {
        value = Guid.NewGuid().ToString();
        cache.SetString(key, value, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(10) });
        return new GeneratedGuid(false, value!);
    }
    return new GeneratedGuid(true, value!);
})
.WithName("GenerateGuidWithCache")
.WithOpenApi();

app.Run();

record GeneratedGuid(bool FromCache, string Value)
{
}
