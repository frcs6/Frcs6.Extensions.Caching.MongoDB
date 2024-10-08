namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal sealed class MongoCache : IDistributedCache
{
    private readonly ICacheItemBuilder _cacheItemBuilder;
    private readonly ICacheItemRepository _cacheItemRepository;

#pragma warning disable IDE0290 // Use primary constructor
    public MongoCache(ICacheItemBuilder cacheItemBuilder, ICacheItemRepository cacheItemRepository)
#pragma warning restore IDE0290 // Use primary constructor
    {
        _cacheItemBuilder = cacheItemBuilder;
        _cacheItemRepository = cacheItemRepository;
    }

    public byte[]? Get(string key)
        => GetAndRefresh(key, true);
    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        => await GetAndRefreshAsync(key, true, token).ConfigureAwait(false);

    public void Refresh(string key)
        => GetAndRefresh(key, false);

    public async Task RefreshAsync(string key, CancellationToken token = default)
        => await GetAndRefreshAsync(key, false, token).ConfigureAwait(false);

    public void Remove(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        CleanExpired();
        _cacheItemRepository.Remove(key);
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        await CleanExpiredAsync(token).ConfigureAwait(false);
        await _cacheItemRepository.RemoveAsync(key, token).ConfigureAwait(false);
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        CleanExpired();
        var cacheItem = _cacheItemBuilder.Build(key, value, options);
        _cacheItemRepository.Write(cacheItem);
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        await CleanExpiredAsync(token).ConfigureAwait(false);
        var cacheItem = _cacheItemBuilder.Build(key, value, options);
        await _cacheItemRepository.WriteAsync(cacheItem, token).ConfigureAwait(false);
    }

    private byte[]? GetAndRefresh(string key, bool withData)
    {
        ArgumentNullException.ThrowIfNull(key);

        CleanExpired();

        var cacheItem = withData ?
            _cacheItemRepository.Read(key) :
            _cacheItemRepository.ReadPartial(key);

        if (cacheItem == null) return null;

        if (_cacheItemBuilder.Refresh(cacheItem))
        {
            _cacheItemRepository.WritePartial(cacheItem);
        }

        return cacheItem.Value?.ToArray() ?? null;
    }

    private async Task<byte[]?> GetAndRefreshAsync(string key, bool withData, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(key);

        await CleanExpiredAsync(token).ConfigureAwait(false);

        var cacheItem = await (withData ?
            _cacheItemRepository.ReadAsync(key, token) :
            _cacheItemRepository.ReadPartialAsync(key, token)).ConfigureAwait(false);

        if (cacheItem == null) return null;

        if (_cacheItemBuilder.Refresh(cacheItem))
        {
            await _cacheItemRepository.WritePartialAsync(cacheItem, token).ConfigureAwait(false);
        }

        return cacheItem.Value?.ToArray() ?? null;
    }

    private void CleanExpired()
        => _cacheItemRepository.RemoveExpired();

    private async Task CleanExpiredAsync(CancellationToken token)
        => await _cacheItemRepository.RemoveExpiredAsync(token).ConfigureAwait(false);
}