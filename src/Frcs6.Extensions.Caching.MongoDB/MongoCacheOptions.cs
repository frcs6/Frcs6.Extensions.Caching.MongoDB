namespace Frcs6.Extensions.Caching.MongoDB;

/// <summary>
/// Configuration options.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class MongoCacheOptions : IOptions<MongoCacheOptions>
{
    /// <summary>
    /// Cache database name.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Cache collection name.
    /// </summary>
    public string? CollectionName { get; set; }

    /// <summary>
    /// Allow cached item without.
    /// </summary>
    public bool AllowNoExpiration { get; set; }

    /// <summary>
    /// Delay between remove task.
    /// </summary>
    public TimeSpan? RemoveExpiredDelay { get; set; }

    MongoCacheOptions IOptions<MongoCacheOptions>.Value => this;
}