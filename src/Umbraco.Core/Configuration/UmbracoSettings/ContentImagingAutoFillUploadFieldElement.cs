using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentImagingAutoFillUploadFieldElement : ConfigurationElement
    {
        [ConfigurationProperty("alias", IsKey = true)]
        internal string Alias
        {
            get { return (string)this["alias"]; }
        }

        [ConfigurationProperty("widthFieldAlias")]
        internal InnerTextConfigurationElement<string> WidthFieldAlias
        {
            get { return (InnerTextConfigurationElement<string>)this["widthFieldAlias"]; }
        }

        [ConfigurationProperty("heightFieldAlias")]
        internal InnerTextConfigurationElement<string> HeightFieldAlias
        {
            get { return (InnerTextConfigurationElement<string>)this["heightFieldAlias"]; }
        }

        [ConfigurationProperty("lengthFieldAlias")]
        internal InnerTextConfigurationElement<string> LengthFieldAlias
        {
            get { return (InnerTextConfigurationElement<string>)this["lengthFieldAlias"]; }
        }

        [ConfigurationProperty("extensionFieldAlias")]
        internal InnerTextConfigurationElement<string> ExtensionFieldAlias
        {
            get { return (InnerTextConfigurationElement<string>)this["extensionFieldAlias"]; }
        }
    }
}