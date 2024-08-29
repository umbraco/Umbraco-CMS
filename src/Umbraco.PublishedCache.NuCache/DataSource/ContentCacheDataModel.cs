using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MessagePack;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

/// <summary>
///     The content model stored in the content cache database table serialized as JSON
/// </summary>
[DataContract] // NOTE: Use DataContract annotations here to control how MessagePack serializes/deserializes the data to use INT keys
public class ContentCacheDataModel
{
    // TODO: We don't want to allocate empty arrays
    // dont serialize empty properties
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

    [DataMember(Order = 2)]
    [JsonPropertyName("us")]
    public string? UrlSegment { get; set; }

    // Legacy properties used to deserialize existing nucache db entries
    [IgnoreDataMember]
    [JsonPropertyName("properties")]
    [JsonConverter(typeof(JsonDictionaryStringIgnoreCaseConverter<PropertyData[]>))]
    private Dictionary<string, PropertyData[]> LegacyPropertyData { set => PropertyData = value; }

    [IgnoreDataMember]
    [JsonPropertyName("cultureData")]
    [JsonConverter(typeof(JsonDictionaryStringIgnoreCaseConverter<CultureVariation>))]
    private Dictionary<string, CultureVariation> LegacyCultureData { set => CultureData = value; }

    [IgnoreDataMember]
    [JsonPropertyName("urlSegment")]
    private string LegacyUrlSegment { set => UrlSegment = value; }
}
