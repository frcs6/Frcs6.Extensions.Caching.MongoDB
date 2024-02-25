namespace Frcs6.Extensions.Caching.MongoDB.Internal.Compat;

[ExcludeFromCodeCoverage]
internal static class ArgumentThrowHelper
{
    public static void ThrowIfNullOrWhiteSpace(string? argument, string? paramName = null)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName);
#else
        ArgumentNullException.ThrowIfNull(argument);
        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException("The value cannot be an empty or whitespace string.", paramName);
        }
#endif
    }

    public static void ThrowIfLessThanOrEqual<T>(T value, T other, string? paramName = null)
        where T : IComparable<T>
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, other, paramName);
#else
        if (value.CompareTo(other) <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, $"'{value}' must be greater than '{other}'.");
        }
#endif
    }
}