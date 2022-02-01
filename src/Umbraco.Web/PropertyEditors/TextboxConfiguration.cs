using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the textbox value editor.
    /// </summary>
    public class TextboxConfiguration
    {
        [ConfigurationField("maxChars", "Maximum allowed characters", "textstringlimited", Description = "If empty, 512 character limit")]
        public int? MaxChars { get; set; }
    }
}
