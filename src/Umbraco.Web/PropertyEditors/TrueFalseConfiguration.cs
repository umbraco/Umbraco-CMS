using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the boolean value editor.
    /// </summary>
    public class TrueFalseConfiguration
    {
        [ConfigurationField("default","Initial State", "boolean",Description = "The initial state for the checkbox, when this is displayed in the backoffice for the first time. NB changing this initial value does not alter the value on existing published content items.")]
        public string Default { get; set; } // TODO: well, true or false?!

        [ConfigurationField("labelOn", "Write a label text", "textstring")]
        public string Label { get; set; }
    }
}
