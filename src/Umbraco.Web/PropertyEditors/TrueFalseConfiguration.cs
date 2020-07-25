using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the boolean value editor.
    /// </summary>
    public class TrueFalseConfiguration
    {
        [ConfigurationField("default","Initial State", "boolean",Description = "The initial state for this checkbox, when it is displayed for the first time in the backoffice, eg. for a new content item.")]
        public string Default { get; set; } // TODO: well, true or false?!

        [ConfigurationField("labelOn", "Label text when enabled", "textstring")]
        public string LabelOn { get; set; }

        [ConfigurationField("labelOff", "Label text when disabled", "textstring")]
        public string LabelOff { get; set; }
    }
}
