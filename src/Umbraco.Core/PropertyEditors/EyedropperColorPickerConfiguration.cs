namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the eyedropper picker value editor.
    /// </summary>
    public class EyedropperColorPickerConfiguration
    {
        [ConfigurationField("showAlpha", "Show alpha", "boolean", Description = "Allow alpha transparency selection.")]
        public bool ShowAlpha { get; set; }

        [ConfigurationField("showPalette", "Show palette", "boolean", Description = "Show a palette next to the color picker.")]
        public bool ShowPalette { get; set; }
    }
}
