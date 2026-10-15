namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the range slider value editor.
/// </summary>
public class RangeSliderConfiguration : SliderConfigurationBase
{
    /// <summary>
    /// Gets or sets the minimum required difference between the low and high values.
    /// A value of 0 allows both handles to select the same value.
    /// </summary>
    [ConfigurationField("minimumRange")]
    public decimal MinimumRange { get; set; }
}
