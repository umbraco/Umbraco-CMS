using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the email address value editor.
    /// </summary>
    public class EmailAddressConfiguration
    {
        [ConfigurationField("IsRequired", "Required?", "boolean")]
        public bool IsRequired { get; set; }
    }
}
