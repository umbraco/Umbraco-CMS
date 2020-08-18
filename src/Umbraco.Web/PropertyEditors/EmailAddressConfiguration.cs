using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the email address value editor.
    /// </summary>
    public class EmailAddressConfiguration
    {
        [ConfigurationField("IsRequired", "Required?", "hidden", Description = "Deprecated; Make this required by selecting mandatory when adding to the document type")]
        public bool IsRequired { get; set; }
    }
}
