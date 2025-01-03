using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the slider value editor.
/// </summary>
public class SliderConfiguration
{
    [ConfigurationField("enableRange")]
    public bool EnableRange { get; set; }

    [ConfigurationField("minVal")]
    [JsonPropertyName("minVal")]
    public decimal MinimumValue { get; set; }

    [ConfigurationField("maxVal")]
    [JsonPropertyName("maxVal")]
    public decimal MaximumValue { get; set; }

    [ConfigurationField("initVal1")]
    [JsonPropertyName("initVal1")]
    public decimal InitialValue1 { get; set; }

    [ConfigurationField("initVal2")]
    [JsonPropertyName("initVal2")]
    public decimal InitialValue2 { get; set; }
}
