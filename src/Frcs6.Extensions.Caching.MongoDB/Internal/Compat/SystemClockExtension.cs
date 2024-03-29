#if !NET8_0_OR_GREATER

using Microsoft.Extensions.Internal;

namespace Frcs6.Extensions.Caching.MongoDB.Internal.Compat;

[ExcludeFromCodeCoverage]
internal static class SystemClockExtension
{
    public static DateTimeOffset GetUtcNow(this ISystemClock systemClock) => systemClock.UtcNow;
}
#endif