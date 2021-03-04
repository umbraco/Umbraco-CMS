// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors
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
