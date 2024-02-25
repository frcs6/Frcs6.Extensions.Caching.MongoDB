namespace Frcs6.Extensions.Caching.MongoDB;

[ExcludeFromCodeCoverage]
public sealed class MongoCacheOptions : IOptions<MongoCacheOptions>
{
    public string? DatabaseName { get; set; }
    public string? CollectionName { get; set; }
    public bool AllowNoExpiration { get; set; } = true;
    public TimeSpan? RemoveExpiredDelay { get; set; }

    MongoCacheOptions IOptions<MongoCacheOptions>.Value => this;
}