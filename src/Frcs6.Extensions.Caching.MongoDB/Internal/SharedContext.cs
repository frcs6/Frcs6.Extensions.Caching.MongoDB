namespace Frcs6.Extensions.Caching.MongoDB.Internal;

#if NET8_0_OR_GREATER
internal sealed class SharedContext(IOptions<MongoCacheOptions> mongoCacheOptions)
#else
internal sealed class SharedContext
#endif
{
    public SemaphoreSlim LockNextRemoveExpired { get; } = new(1, 1);
    public DateTimeOffset? NextRemoveExpired { get; set; }

    public SemaphoreSlim LockIndexCreated { get; } = new(1, 1);
    public bool IndexCreated { get; set; }
#if NET8_0_OR_GREATER
    public TimeSpan? RemoveExpiredDelay { get; } = mongoCacheOptions.Value.RemoveExpiredDelay;
#else
    public TimeSpan? RemoveExpiredDelay { get; }

    public SharedContext(IOptions<MongoCacheOptions> mongoCacheOptions)
    {
        RemoveExpiredDelay = mongoCacheOptions.Value.RemoveExpiredDelay;
    }
#endif
}