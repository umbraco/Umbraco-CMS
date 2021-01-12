namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the eyedropper picker value editor.
    /// </summary>
    public class EyedropperColorPickerConfiguration : ValueListConfiguration
    {
        [ConfigurationField("showAlpha", "Show alpha", "boolean", Description = "Allow alpha transparency selection.")]
        public bool ShowAlpha { get; set; }
    }
}
