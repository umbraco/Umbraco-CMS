namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the color picker value editor.
/// </summary>
public class ColorPickerConfiguration
{
    [ConfigurationField("useLabel")]
    public bool UseLabel { get; set; }

    [ConfigurationField("items")]
    public List<ColorPickerItem> Items { get; set; } = new();

    public class ColorPickerItem
    {
        public required string Value { get; set; }

        public required string Label { get; set; }
    }
}
