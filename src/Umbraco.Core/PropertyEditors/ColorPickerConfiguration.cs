namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the color picker value editor.
/// </summary>
public class ColorPickerConfiguration : ValueListConfiguration
{
    [ConfigurationField(
        "useLabel",
        "Include labels?",
        "boolean",
        Description = "Stores colors as a Json object containing both the color hex string and label, rather than just the hex string.")]
    public bool UseLabel { get; set; }
}
