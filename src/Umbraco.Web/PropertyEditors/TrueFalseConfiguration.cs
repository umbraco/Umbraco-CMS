using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the boolean value editor.
    /// </summary>
    public class TrueFalseConfiguration
    {
        [ConfigurationField("default", "Default Value", "boolean")]
        public string Default { get; set; } // todo - well, true or false?!

        [ConfigurationField("labelOn", "Write a label text", "textstring")]
        public string Label { get; set; }
    }
}
