using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the email address value editor.
    /// </summary>
    public class EmailAddressConfigurationEditor : ConfigurationEditor<EmailAddressConfiguration>
    {
        public EmailAddressConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
