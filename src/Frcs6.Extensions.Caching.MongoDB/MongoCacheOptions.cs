using System.Diagnostics.CodeAnalysis;

namespace Frcs6.Extensions.Caching.MongoDB;

// TODO Bool to init collection ?
// TODO Allow no expire option
[ExcludeFromCodeCoverage]
public sealed class MongoCacheOptions
{
    public string? DatabaseName { get; set; }
    public string? CollectionName { get; set; }
}