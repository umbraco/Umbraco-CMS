using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <summary>
///     Represents the culture variation information on a content item
/// </summary>
[DataContract] // NOTE: Use DataContract annotations here to control how MessagePack serializes/deserializes the data to use INT keys
public class CultureVariation
{
    [DataMember(Order = 0)]
    [JsonPropertyName("nm")]
    public string? Name { get; set; }

    [DataMember(Order = 1)]
    [JsonPropertyName("us")]
    public string? UrlSegment { get; set; }

    [DataMember(Order = 2)]
    [JsonPropertyName("dt")]
    [JsonConverter(typeof(JsonUniversalDateTimeConverter))]
    public DateTime Date { get; set; }

    [DataMember(Order = 3)]
    [JsonPropertyName("isd")]
    public bool IsDraft { get; set; }
}
