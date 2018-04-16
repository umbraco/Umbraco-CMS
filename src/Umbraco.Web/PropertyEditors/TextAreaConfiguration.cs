using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the textarea value editor.
    /// </summary>
    public class TextAreaConfiguration
    {
        [ConfigurationField("maxChars", "Maximum allowed characters", "number", Description = "If empty - no character limit")]
        public int MaxChars { get; set; }
    }
}