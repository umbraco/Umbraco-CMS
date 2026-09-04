namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the slider value editor.
/// </summary>
public class SliderConfiguration : SliderConfigurationBase
{
    /// <summary>
    /// Gets or sets a value indicating whether range selection is enabled (two handles).
    /// </summary>
    /// <remarks>
    ///     No longer used. A slider that holds a range is the separate <c>Umbraco.RangeSlider</c> editor, so that the
    ///     type a slider property yields cannot change with its configuration.
    /// </remarks>
    [Obsolete("No longer used. Use the Umbraco.RangeSlider property editor for a slider holding a range. Scheduled for removal in Umbraco 21.")]
    [ConfigurationField("enableRange")]
    public bool EnableRange { get; set; }

    /// <summary>
    /// Gets or sets the minimum required difference between the low and high values when range is enabled.
    /// A value of 0 allows both handles to select the same value.
    /// </summary>
    /// <remarks>
    ///     No longer used, as a single-value slider has no range to span.
    ///     See <see cref="RangeSliderConfiguration.MinimumRange" />.
    /// </remarks>
    [Obsolete("No longer used. See RangeSliderConfiguration.MinimumRange. Scheduled for removal in Umbraco 21.")]
    [ConfigurationField("minimumRange")]
    public decimal MinimumRange { get; set; }
}
