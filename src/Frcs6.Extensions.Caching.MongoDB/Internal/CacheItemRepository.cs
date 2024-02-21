using Microsoft.Extensions.Options;

namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal sealed class CacheItemRepository : ICacheItemRepository
{
    private static readonly ReplaceOptions DefaultReplaceOptions = new() { IsUpsert = true };
    private static readonly UpdateOptions DefaultUpdateOptions = new() { IsUpsert = false };

    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static DateTimeOffset? _nextRemoveExpired;

    private readonly TimeSpan? _removeExpiredDelay;
    private readonly IMongoCollection<CacheItem> _cacheItemCollection;

#if NET8_0_OR_GREATER
    private readonly TimeProvider _timeProvider;
#else
    private readonly ISystemClock _timeProvider;
#endif

    public CacheItemRepository(
            IMongoClient mongoClient,
#if NET8_0_OR_GREATER
            TimeProvider timeProvider,
#else
            ISystemClock timeProvider,
#endif
            IOptions<MongoCacheOptions> mongoCacheOptions)
    {
        ValidateOptions(mongoCacheOptions.Value);
        _cacheItemCollection = GetCollection(mongoClient, mongoCacheOptions.Value);
        _removeExpiredDelay = mongoCacheOptions.Value.RemoveExpiredDelay;
#if NET8_0_OR_GREATER
        _timeProvider = timeProvider;
#else
        _timeProvider = timeProvider;
#endif
    }

    public CacheItem Read(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _cacheItemCollection
            .Find(FindByKey(key))
            .SingleOrDefault();
    }

    public async Task<CacheItem> ReadAsync(string key, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(key);
        return await _cacheItemCollection
            .Find(FindByKey(key))
            .SingleOrDefaultAsync(token)
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

    public async Task<CacheItem> ReadPartialAsync(string key, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(key);
        return await _cacheItemCollection
            .Find(FindByKey(key))
            .Project<CacheItem>(Builders<CacheItem>.Projection.Exclude(x => x.Value))
            .SingleOrDefaultAsync(token)
            .ConfigureAwait(false);
    }

    public void Write(CacheItem cacheItem)
    {
        ArgumentNullException.ThrowIfNull(cacheItem);
        _cacheItemCollection.ReplaceOne(FindByKey(cacheItem.Key!), cacheItem, DefaultReplaceOptions);
    }

    public async Task WriteAsync(CacheItem cacheItem, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(cacheItem);
        await _cacheItemCollection
            .ReplaceOneAsync(FindByKey(cacheItem.Key!), cacheItem, DefaultReplaceOptions, token)
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

    public async Task WritePartialAsync(CacheItem cacheItem, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(cacheItem);
        await _cacheItemCollection.UpdateOneAsync(
            FindByKey(cacheItem.Key!),
            Builders<CacheItem>.Update
                .Set(c => c.AbsoluteExpiration, cacheItem.AbsoluteExpiration)
                .Set(c => c.ExpireAt, cacheItem.ExpireAt)
                .Set(c => c.SlidingExpiration, cacheItem.SlidingExpiration),
            DefaultUpdateOptions,
            token).ConfigureAwait(false);
    }

    public void Remove(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        _cacheItemCollection.DeleteOne(FindByKey(key));
    }

    public async Task RemoveAsync(string key, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(key);
        await _cacheItemCollection
            .DeleteOneAsync(FindByKey(key), token)
            .ConfigureAwait(false);
    }

    public void RemoveExpired()
    {
#pragma warning disable IDE0039 // Use local function
        var removeExpired = () => _cacheItemCollection.DeleteMany(Builders<CacheItem>.Filter.Lt(i => i.ExpireAt, _timeProvider.GetUtcNow().Ticks));
#pragma warning restore IDE0039 // Use local function

        if (!_removeExpiredDelay.HasValue)
        {
            removeExpired();
            return;
        }

        _lock.Wait();
        try
        {
            var utcNow = _timeProvider.GetUtcNow();
            if (!_nextRemoveExpired.HasValue || utcNow >= _nextRemoveExpired)
            {
                removeExpired();
                _nextRemoveExpired = utcNow.Add(_removeExpiredDelay.Value);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task RemoveExpiredAsync(CancellationToken token)
    {
#pragma warning disable IDE0039 // Use local function
        var removeExpiredAsync = () => _cacheItemCollection.DeleteManyAsync(Builders<CacheItem>.Filter.Lt(i => i.ExpireAt, _timeProvider.GetUtcNow().Ticks), token);
#pragma warning restore IDE0039 // Use local function

        if (!_removeExpiredDelay.HasValue)
        {
            await removeExpiredAsync().ConfigureAwait(true);
            return;
        }

        await _lock.WaitAsync(token).ConfigureAwait(true);
        try
        {
            var utcNow = _timeProvider.GetUtcNow();
            if (!_nextRemoveExpired.HasValue || utcNow >= _nextRemoveExpired)
            {
                await removeExpiredAsync().ConfigureAwait(true);
                _nextRemoveExpired = utcNow.Add(_removeExpiredDelay.Value);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private static FilterDefinition<CacheItem> FindByKey(string key)
         => Builders<CacheItem>.Filter.Eq(i => i.Key, key);

    private static IMongoCollection<CacheItem> GetCollection(IMongoClient mongoClient, MongoCacheOptions mongoCacheOptions)
        => mongoClient
            .GetDatabase(mongoCacheOptions.DatabaseName)
            .GetCollection<CacheItem>(mongoCacheOptions.CollectionName);

    private static void ValidateOptions(MongoCacheOptions mongoCacheOptions)
    {
        ArgumentThrowHelper.ThrowIfNullOrWhiteSpace(mongoCacheOptions.DatabaseName);
        ArgumentThrowHelper.ThrowIfNullOrWhiteSpace(mongoCacheOptions.CollectionName);
    }
}