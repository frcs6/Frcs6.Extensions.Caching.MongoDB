using System.Runtime.CompilerServices;

namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal sealed class CacheItemRepository : ICacheItemRepository
{
    private static readonly ReplaceOptions DefaultReplaceOptions = new() { IsUpsert = true };
    private static readonly UpdateOptions DefaultUpdateOptions = new() { IsUpsert = false };

    private readonly IMongoCollection<CacheItem> _cacheItemCollection;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan? _removeExpiredDelay;

    private readonly SemaphoreSlim _lockNextRemoveExpired = new(1, 1);
    private DateTimeOffset? _nextRemoveExpired;

    public CacheItemRepository(
        IMongoClient mongoClient,
        TimeProvider timeProvider,
        IOptions<MongoCacheOptions> mongoCacheOptions)
    {
        ValidateOptions(mongoCacheOptions.Value);
        _cacheItemCollection = GetCollection(mongoClient, mongoCacheOptions.Value);
        _timeProvider = timeProvider;
        _removeExpiredDelay = mongoCacheOptions.Value.RemoveExpiredDelay;
        InitializeIndex();
    }

    public void Dispose()
        => _lockNextRemoveExpired.Dispose();

    public CacheItem? Read(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _cacheItemCollection
            .Find(FindByKey(key))
            .SingleOrDefault();
    }

    public async Task<CacheItem?> ReadAsync(string key, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(key);
        return await _cacheItemCollection
            .Find(FindByKey(key))
            .SingleOrDefaultAsync(token)
            .ConfigureAwait(false);
    }

    public CacheItem? ReadPartial(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _cacheItemCollection
            .Find(FindByKey(key))
            .Project<CacheItem>(Builders<CacheItem>.Projection.Exclude(x => x.Value))
            .SingleOrDefault();
    }

    public async Task<CacheItem?> ReadPartialAsync(string key, CancellationToken token)
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
            DefaultUpdateOptions, token).ConfigureAwait(false);
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

    public void RemoveExpired(bool force = false)
    {
        if (force)
        {
            DoRemoveExpired();
            return;
        }

        _lockNextRemoveExpired.Wait();
        try
        {
            if (!_removeExpiredDelay.HasValue)
            {
                DoRemoveExpired();
            }
            else
            {
                var utcNow = _timeProvider.GetUtcNow();
                if (!_nextRemoveExpired.HasValue || utcNow >= _nextRemoveExpired.Value)
                {
                    DoRemoveExpired();
                    _nextRemoveExpired = utcNow.Add(_removeExpiredDelay.Value);
                }
            }
        }
        finally
        {
            _lockNextRemoveExpired.Release();
        }
    }

    private void DoRemoveExpired() =>
        _cacheItemCollection.DeleteMany(Builders<CacheItem>.Filter.Lt(i => i.ExpireAt,
            _timeProvider.GetUtcNow().Ticks));

    public async Task RemoveExpiredAsync(CancellationToken token)
    {
        await _lockNextRemoveExpired.WaitAsync(token).ConfigureAwait(false);
        try
        {
            if (!_removeExpiredDelay.HasValue)
            {
                await DoRemoveExpiredAsync(token);
            }
            else
            {
                var utcNow = _timeProvider.GetUtcNow();
                if (!_nextRemoveExpired.HasValue || utcNow >= _nextRemoveExpired.Value)
                {
                    await DoRemoveExpiredAsync(token);
                    _nextRemoveExpired = utcNow.Add(_removeExpiredDelay.Value);
                }
            }
        }
        finally
        {
            _lockNextRemoveExpired.Release();
        }
    }

    private ConfiguredTaskAwaitable<DeleteResult> DoRemoveExpiredAsync(CancellationToken token) =>
        _cacheItemCollection
            .DeleteManyAsync(Builders<CacheItem>.Filter.Lt(i => i.ExpireAt, _timeProvider.GetUtcNow().Ticks), token)
            .ConfigureAwait(false);


    private void InitializeIndex()
    {
        var notificationLogBuilder = Builders<CacheItem>.IndexKeys;

        var indexModelByKey = new CreateIndexModel<CacheItem>(notificationLogBuilder.Ascending(x => x.Key));
        _cacheItemCollection.Indexes.CreateOne(indexModelByKey);

        var indexModelExpireAt = new CreateIndexModel<CacheItem>(notificationLogBuilder.Ascending(x => x.ExpireAt));
        _cacheItemCollection.Indexes.CreateOne(indexModelExpireAt);
    }

    private static FilterDefinition<CacheItem> FindByKey(string key)
        => Builders<CacheItem>.Filter.Eq(i => i.Key, key);

    private static IMongoCollection<CacheItem> GetCollection(
        IMongoClient mongoClient,
        MongoCacheOptions mongoCacheOptions)
        => mongoClient
            .GetDatabase(mongoCacheOptions.DatabaseName)
            .GetCollection<CacheItem>(mongoCacheOptions.CollectionName);

    private static void ValidateOptions(MongoCacheOptions mongoCacheOptions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mongoCacheOptions.DatabaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(mongoCacheOptions.CollectionName);
    }
}