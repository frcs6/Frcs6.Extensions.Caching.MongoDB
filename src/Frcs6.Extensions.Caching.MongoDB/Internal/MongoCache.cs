namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal sealed class MongoCache(
    ICacheItemBuilder _cacheItemBuilder,
    ICacheItemRepository _cacheItemRepository) : IDistributedCache
{
    public byte[]? Get(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return GetAndRefresh(key, true);
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        return await GetAndRefreshAsync(key, true).ConfigureAwait(false);
    }

    public void Refresh(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        GetAndRefresh(key, false);
    }

    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        await GetAndRefreshAsync(key, false).ConfigureAwait(false);
    }

    public void Remove(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        CleanExpired();
        _cacheItemRepository.Remove(key);
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        CleanExpired();
        await _cacheItemRepository.RemoveAsync(key).ConfigureAwait(false);
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

        CleanExpired();
        var cacheItem = _cacheItemBuilder.Build(key, value, options);
        await _cacheItemRepository.WriteAsync(cacheItem).ConfigureAwait(false);
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

    private async Task<byte[]?> GetAndRefreshAsync(string key, bool withData)
    {
        ArgumentNullException.ThrowIfNull(key);

        CleanExpired();

        var cacheItem = await (withData ?
            _cacheItemRepository.ReadAsync(key) :
            _cacheItemRepository.ReadPartialAsync(key)).ConfigureAwait(false);

        if (cacheItem == null) return null;

        if (_cacheItemBuilder.Refresh(cacheItem))
        {
            await _cacheItemRepository.WritePartialAsync(cacheItem).ConfigureAwait(false);
        }

        return cacheItem.Value?.ToArray() ?? null;
    }

    private void CleanExpired()
    {
        _cacheItemRepository.RemoveExpired();
    }
}