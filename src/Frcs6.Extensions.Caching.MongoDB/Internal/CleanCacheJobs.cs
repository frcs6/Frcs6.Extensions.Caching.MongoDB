using Microsoft.Extensions.Hosting;

namespace Frcs6.Extensions.Caching.MongoDB.Internal;

internal sealed class CleanCacheJobs : IHostedService, IDisposable
{
    private readonly ICacheItemRepository _cacheItemRepository;
    private readonly TimeSpan _removeExpiredDelay;

    private readonly Timer _timer;

    public CleanCacheJobs(ICacheItemRepository cacheItemRepository, IOptions<MongoCacheOptions> mongoCacheOptions)
    {
        ArgumentNullException.ThrowIfNull(mongoCacheOptions.Value.RemoveExpiredDelay);

        _cacheItemRepository = cacheItemRepository;
        _removeExpiredDelay = mongoCacheOptions.Value.RemoveExpiredDelay.Value;
        _timer = new Timer(DoWork, null, Timeout.Infinite, Timeout.Infinite);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer.Change(_removeExpiredDelay, _removeExpiredDelay);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        return Task.CompletedTask;
    }

    public void Dispose()
        => _timer.Dispose();

    private void DoWork(object? state)
        => _cacheItemRepository.RemoveExpired(true);
}