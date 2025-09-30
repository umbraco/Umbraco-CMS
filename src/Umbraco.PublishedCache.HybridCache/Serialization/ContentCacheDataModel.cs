using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MessagePack;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Infrastructure.HybridCache.Serialization;

/// <summary>
///     The content model stored in the content cache database table serialized as JSON
/// </summary>
[DataContract] // NOTE: Use DataContract annotations here to control how MessagePack serializes/deserializes the data to use INT keys
public sealed class ContentCacheDataModel
{
    [DataMember(Order = 0)]
    [JsonPropertyName("pd")]
    [JsonConverter(typeof(JsonDictionaryStringInternIgnoreCaseConverter<PropertyData[]>))]
    [MessagePackFormatter(typeof(MessagePackDictionaryStringInternIgnoreCaseFormatter<PropertyData[]>))]
    public Dictionary<string, PropertyData[]>? PropertyData { get; set; }

    [DataMember(Order = 1)]
    [JsonPropertyName("cd")]
    [JsonConverter(typeof(JsonDictionaryStringInternIgnoreCaseConverter<CultureVariation>))]
    [MessagePackFormatter(typeof(MessagePackDictionaryStringInternIgnoreCaseFormatter<CultureVariation>))]
    public Dictionary<string, CultureVariation>? CultureData { get; set; }

    // TODO: Remove this when routing cache is in place
    [DataMember(Order = 2)]
    [JsonPropertyName("us")]
    public string? UrlSegment { get; set; }
}
