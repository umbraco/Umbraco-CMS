namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the Eye Dropper picker value editor.
/// </summary>
public class EyeDropperColorPickerConfiguration
{
    [ConfigurationField("showAlpha", "Show alpha", "boolean", Description = "Allow alpha transparency selection.")]
    public bool ShowAlpha { get; set; }

    [ConfigurationField("showPalette", "Show palette", "boolean", Description = "Show a palette next to the color picker.")]
    public bool ShowPalette { get; set; }
}
