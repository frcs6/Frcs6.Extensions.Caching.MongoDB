namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal sealed class MongoCache(ICacheItemBuilder cacheItemBuilder, ICacheItemRepository cacheItemRepository)
    : IDistributedCache
{
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
        cacheItemRepository.Remove(key);
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        await CleanExpiredAsync(token).ConfigureAwait(false);
        await cacheItemRepository.RemoveAsync(key, token).ConfigureAwait(false);
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        CleanExpired();
        var cacheItem = cacheItemBuilder.Build(key, value, options);
        cacheItemRepository.Write(cacheItem);
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        await CleanExpiredAsync(token).ConfigureAwait(false);
        var cacheItem = cacheItemBuilder.Build(key, value, options);
        await cacheItemRepository.WriteAsync(cacheItem, token).ConfigureAwait(false);
    }

    private byte[]? GetAndRefresh(string key, bool withData)
    {
        ArgumentNullException.ThrowIfNull(key);

        CleanExpired();

        var cacheItem = withData ? cacheItemRepository.Read(key) : cacheItemRepository.ReadPartial(key);

        if (cacheItem == null) return null;

        if (cacheItemBuilder.Refresh(cacheItem))
        {
            cacheItemRepository.WritePartial(cacheItem);
        }

        return cacheItem.Value?.ToArray() ?? null;
    }

    private async Task<byte[]?> GetAndRefreshAsync(string key, bool withData, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(key);

        await CleanExpiredAsync(token).ConfigureAwait(false);

        var cacheItem =
            await (withData
                ? cacheItemRepository.ReadAsync(key, token)
                : cacheItemRepository.ReadPartialAsync(key, token)).ConfigureAwait(false);

        if (cacheItem == null) return null;

        if (cacheItemBuilder.Refresh(cacheItem))
        {
            await cacheItemRepository.WritePartialAsync(cacheItem, token).ConfigureAwait(false);
        }

        return cacheItem.Value?.ToArray() ?? null;
    }

    private void CleanExpired()
        => cacheItemRepository.RemoveExpired();

    private async Task CleanExpiredAsync(CancellationToken token)
        => await cacheItemRepository.RemoveExpiredAsync(token).ConfigureAwait(false);
}