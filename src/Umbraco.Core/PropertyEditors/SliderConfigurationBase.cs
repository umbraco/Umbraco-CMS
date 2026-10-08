namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration shared by the slider value editors.
/// </summary>
/// <remarks>
///     There is one slider editor per shape of value a slider holds - a single value or a range of two - so that the
///     type a slider property yields follows from the editor rather than from configuration. What can be selected is
///     configured the same way for both.
/// </remarks>
public abstract class SliderConfigurationBase
{
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
