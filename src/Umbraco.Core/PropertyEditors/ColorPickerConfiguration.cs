namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the color picker value editor.
/// </summary>
public class ColorPickerConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether to display color labels.
    /// </summary>
    [ConfigurationField("useLabel")]
    public bool UseLabel { get; set; }

    /// <summary>
    /// Gets or sets the list of available color items.
    /// </summary>
    [ConfigurationField("items")]
    public List<ColorPickerItem> Items { get; set; } = new();

    /// <summary>
    /// Represents a single color item in the color picker.
    /// </summary>
    public class ColorPickerItem
    {
        /// <summary>
        /// Gets or sets the color value (typically a hex color code).
        /// </summary>
        public required string Value { get; set; }

        /// <summary>
        /// Gets or sets the display label for the color.
        /// </summary>
        public required string Label { get; set; }
    }
}
