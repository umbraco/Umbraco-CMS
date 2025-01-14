namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the slider value editor.
/// </summary>
public class SliderConfiguration
{
    [ConfigurationField("enableRange")]
    public bool EnableRange { get; set; }

    [ConfigurationField("minVal")]
    public decimal MinimumValue { get; set; }

    [ConfigurationField("maxVal")]
    public decimal MaximumValue { get; set; }
}
