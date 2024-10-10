using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

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

    // Legacy properties used to deserialize existing nucache db entries
    [IgnoreDataMember]
    [JsonPropertyName("nam")]
    private string LegacyName { set => Name = value; }

    [IgnoreDataMember]
    [JsonPropertyName("urlSegment")]
    private string LegacyUrlSegment { set => UrlSegment = value; }

    [IgnoreDataMember]
    [JsonPropertyName("date")]
    [JsonConverter(typeof(JsonUniversalDateTimeConverter))]
    private DateTime LegacyDate { set => Date = value; }

    [IgnoreDataMember]
    [JsonPropertyName("isDraft")]
    private bool LegacyIsDraft { set => IsDraft = value; }
}
