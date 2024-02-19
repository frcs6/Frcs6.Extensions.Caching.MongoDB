using MongoDB.Bson.Serialization.Attributes;

namespace Frcs6.Extensions.Caching.MongoDB.Internal;

[ExcludeFromCodeCoverage]
internal sealed class CacheItem
{
    [BsonId]
    public string? Key { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("value")]
    public IEnumerable<byte>? Value { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("absoluteExpiration")]
    public long? AbsoluteExpiration { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("slidingExpiration")]
    public long? SlidingExpiration { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("expireAt")]
    public long? ExpireAt { get; set; }
}