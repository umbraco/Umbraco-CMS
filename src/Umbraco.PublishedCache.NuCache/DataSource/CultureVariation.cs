using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

/// <summary>
///     Represents the culture variation information on a content item
/// </summary>
[DataContract] // NOTE: Use DataContract annotations here to control how MessagePack serializes/deserializes the data to use INT keys
public class CultureVariation
{
    [DataMember(Order = 0)]
    [JsonProperty("nm")]
    [JsonPropertyName("nm")]
    public string? Name { get; set; }

    [DataMember(Order = 1)]
    [JsonProperty("us")]
    [JsonPropertyName("us")]
    public string? UrlSegment { get; set; }

    [DataMember(Order = 2)]
    [JsonProperty("dt")]
    [JsonPropertyName("dt")]
    [System.Text.Json.Serialization.JsonConverter(typeof(ForceUtcDateTimeConverter))]
    public DateTime Date { get; set; }

    [DataMember(Order = 3)]
    [JsonProperty("isd")]
    [JsonPropertyName("isd")]
    public bool IsDraft { get; set; }

    // Legacy properties used to deserialize existing nucache db entries
    [IgnoreDataMember]
    [JsonProperty("name")]
    [JsonPropertyName("nam")]
    private string LegacyName { set => Name = value; }

    [IgnoreDataMember]
    [JsonProperty("urlSegment")]
    [JsonPropertyName("urlSegment")]
    private string LegacyUrlSegment { set => UrlSegment = value; }

    [IgnoreDataMember]
    [JsonProperty("date")]
    [JsonPropertyName("date")]
    [System.Text.Json.Serialization.JsonConverter(typeof(ForceUtcDateTimeConverter))]
    private DateTime LegacyDate { set => Date = value; }

    [IgnoreDataMember]
    [JsonProperty("isDraft")]
    [JsonPropertyName("isDraft")]
    private bool LegacyIsDraft { set => IsDraft = value; }
}
