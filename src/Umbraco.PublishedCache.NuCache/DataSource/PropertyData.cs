using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

[DataContract] // NOTE: Use DataContract annotations here to control how MessagePack serializes/deserializes the data to use INT keys
public class PropertyData
{
    private string? _culture;
    private string? _segment;

    [DataMember(Order = 0)]
    [JsonConverter(typeof(JsonStringInternConverter))]
    [DefaultValue("")]
    [JsonPropertyName("c")]
    public string? Culture
    {
        get => _culture;
        set => _culture =
            value ?? throw new ArgumentNullException(
                nameof(value)); // TODO: or fallback to string.Empty? CANNOT be null
    }

    [DataMember(Order = 1)]
    [JsonConverter(typeof(JsonStringInternConverter))]
    [DefaultValue("")]
    [JsonPropertyName("s")]
    public string? Segment
    {
        get => _segment;
        set => _segment =
            value ?? throw new ArgumentNullException(
                nameof(value)); // TODO: or fallback to string.Empty? CANNOT be null
    }

    [DataMember(Order = 2)]
    [JsonPropertyName("v")]
    public object? Value { get; set; }

    // Legacy properties used to deserialize existing nucache db entries
    [IgnoreDataMember]
    private string LegacyCulture
    {
        set => Culture = value;
    }

    [IgnoreDataMember]
    [JsonPropertyName("seg")]
    private string LegacySegment
    {
        set => Segment = value;
    }

    [IgnoreDataMember]
    [JsonPropertyName("val")]
    private object LegacyValue
    {
        set => Value = value;
    }
}
