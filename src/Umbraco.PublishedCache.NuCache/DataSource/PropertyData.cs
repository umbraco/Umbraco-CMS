using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

[DataContract] // NOTE: Use DataContract annotations here to control how MessagePack serializes/deserializes the data to use INT keys
public class PropertyData
{
    private string? _culture;
    private string? _segment;

    [DataMember(Order = 0)]
    [JsonConverter(typeof(AutoInterningStringConverter))]
    [DefaultValue("")]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "c")]
    public string? Culture
    {
        get => _culture;
        set => _culture =
            value ?? throw new ArgumentNullException(
                nameof(value)); // TODO: or fallback to string.Empty? CANNOT be null
    }

    [DataMember(Order = 1)]
    [JsonConverter(typeof(AutoInterningStringConverter))]
    [DefaultValue("")]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "s")]
    public string? Segment
    {
        get => _segment;
        set => _segment =
            value ?? throw new ArgumentNullException(
                nameof(value)); // TODO: or fallback to string.Empty? CANNOT be null
    }

    [DataMember(Order = 2)]
    [JsonProperty("v")]
    public object? Value { get; set; }

    // Legacy properties used to deserialize existing nucache db entries
    [IgnoreDataMember]
    [JsonProperty("culture")]
    private string LegacyCulture
    {
        set => Culture = value;
    }

    [IgnoreDataMember]
    [JsonProperty("seg")]
    private string LegacySegment
    {
        set => Segment = value;
    }

    [IgnoreDataMember]
    [JsonProperty("val")]
    private object LegacyValue
    {
        set => Value = value;
    }
}
