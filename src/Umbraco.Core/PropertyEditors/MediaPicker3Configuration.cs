namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the media picker value editor.
/// </summary>
public class MediaPicker3Configuration : MediaPickerConfigurationBase
{
    /// <summary>
    /// Gets or sets a value indicating whether multiple media items can be selected.
    /// </summary>
    [Obsolete("A media picker holding a single item is now its own property editor. Scheduled for removal in Umbraco 21.")]
    [ConfigurationField("multiple")]
    public bool Multiple { get; set; }

    /// <summary>
    /// Gets or sets the validation limits for the number of selected items.
    /// </summary>
    [ConfigurationField("validationLimit")]
    public NumberRange ValidationLimit { get; set; } = new();

    /// <summary>
    /// Represents a numeric range with optional minimum and maximum values.
    /// </summary>
    public class NumberRange
    {
        /// <summary>
        /// Gets or sets the minimum value of the range.
        /// </summary>
        public int? Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum value of the range.
        /// </summary>
        public int? Max { get; set; }
    }
}
