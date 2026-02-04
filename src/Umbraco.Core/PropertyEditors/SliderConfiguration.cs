namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the slider value editor.
/// </summary>
public class SliderConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether range selection is enabled (two handles).
    /// </summary>
    [ConfigurationField("enableRange")]
    public bool EnableRange { get; set; }

    /// <summary>
    /// Gets or sets the minimum value of the slider.
    /// </summary>
    [ConfigurationField("minVal")]
    public decimal MinimumValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum value of the slider.
    /// </summary>
    [ConfigurationField("maxVal")]
    public decimal MaximumValue { get; set; }

    /// <summary>
    /// Gets or sets the step increment value for the slider.
    /// </summary>
    [ConfigurationField("step")]
    public decimal Step { get; set; }
}
