namespace Frcs6.Extensions.Caching.MongoDB;

[ExcludeFromCodeCoverage]
public sealed class MongoCacheOptions
{
    public string? DatabaseName { get; set; }
    public string? CollectionName { get; set; }
    public bool AllowNoExpiration { get; set; } = true;
}