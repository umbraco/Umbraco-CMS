using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the boolean value editor.
    /// </summary>
    public class TrueFalseConfiguration
    {
        [ConfigurationField("default", "Default Value", "boolean")]
        public string Default { get; set; } // fixme - well, true or false?!
    }
}