using Microsoft.Extensions.Options;

namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal sealed class CacheItemRepository : ICacheItemRepository
{
    private static readonly ReplaceOptions DefaultReplaceOptions = new() { IsUpsert = true };
    private static readonly UpdateOptions DefaultUpdateOptions = new() { IsUpsert = false };

    private readonly IMongoCollection<CacheItem> _cacheItemCollection;
    private readonly TimeProvider _timeProvider;

    public CacheItemRepository(
            IMongoClient mongoClient,
            TimeProvider timeProvider,
            IOptions<MongoCacheOptions> mongoCacheOptions)
    {
        ValidateOptions(mongoCacheOptions.Value);
        _cacheItemCollection = GetCollection(mongoClient, mongoCacheOptions.Value);
        _timeProvider = timeProvider;
    }

    public CacheItem Read(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _cacheItemCollection
            .Find(FindByKey(key))
            .SingleOrDefault();
    }

    public async Task<CacheItem> ReadAsync(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return await _cacheItemCollection
            .Find(FindByKey(key))
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public CacheItem ReadPartial(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _cacheItemCollection
            .Find(FindByKey(key))
            .Project<CacheItem>(Builders<CacheItem>.Projection.Exclude(x => x.Value))
            .SingleOrDefault();
    }

    public async Task<CacheItem> ReadPartialAsync(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return await _cacheItemCollection
            .Find(FindByKey(key))
            .Project<CacheItem>(Builders<CacheItem>.Projection.Exclude(x => x.Value))
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public void Write(CacheItem cacheItem)
    {
        ArgumentNullException.ThrowIfNull(cacheItem);
        _cacheItemCollection.ReplaceOne(FindByKey(cacheItem.Key!), cacheItem, DefaultReplaceOptions);
    }

    public async Task WriteAsync(CacheItem cacheItem)
    {
        ArgumentNullException.ThrowIfNull(cacheItem);
        await _cacheItemCollection
            .ReplaceOneAsync(FindByKey(cacheItem.Key!), cacheItem, DefaultReplaceOptions)
            .ConfigureAwait(false);
    }

    public void WritePartial(CacheItem cacheItem)
    {
        ArgumentNullException.ThrowIfNull(cacheItem);
        _cacheItemCollection.UpdateOne(
            FindByKey(cacheItem.Key!),
            Builders<CacheItem>.Update
                .Set(c => c.AbsoluteExpiration, cacheItem.AbsoluteExpiration)
                .Set(c => c.ExpireAt, cacheItem.ExpireAt)
                .Set(c => c.SlidingExpiration, cacheItem.SlidingExpiration),
            DefaultUpdateOptions);
    }

    public async Task WritePartialAsync(CacheItem cacheItem)
    {
        ArgumentNullException.ThrowIfNull(cacheItem);
        await _cacheItemCollection.UpdateOneAsync(
            FindByKey(cacheItem.Key!),
            Builders<CacheItem>.Update
                .Set(c => c.AbsoluteExpiration, cacheItem.AbsoluteExpiration)
                .Set(c => c.ExpireAt, cacheItem.ExpireAt)
                .Set(c => c.SlidingExpiration, cacheItem.SlidingExpiration),
            DefaultUpdateOptions).ConfigureAwait(false);
    }

    public void Remove(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        _cacheItemCollection.DeleteOne(FindByKey(key));
    }

    public async Task RemoveAsync(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        await _cacheItemCollection
            .DeleteOneAsync(FindByKey(key))
            .ConfigureAwait(false);
    }

    public void RemoveExpired()
        => _cacheItemCollection.DeleteMany(Builders<CacheItem>.Filter.Lt(i => i.ExpireAt, _timeProvider.GetUtcNow().Ticks));

    private static FilterDefinition<CacheItem> FindByKey(string key)
         => Builders<CacheItem>.Filter.Eq(i => i.Key, key);

    private static IMongoCollection<CacheItem> GetCollection(IMongoClient mongoClient, MongoCacheOptions mongoCacheOptions)
        => mongoClient
            .GetDatabase(mongoCacheOptions.DatabaseName)
            .GetCollection<CacheItem>(mongoCacheOptions.CollectionName);

    private static void ValidateOptions(MongoCacheOptions mongoCacheOptions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mongoCacheOptions.DatabaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(mongoCacheOptions.CollectionName);
    }
}