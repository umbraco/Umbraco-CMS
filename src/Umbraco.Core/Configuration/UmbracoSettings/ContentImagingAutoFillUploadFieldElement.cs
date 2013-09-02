using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentImagingAutoFillUploadFieldElement : ConfigurationElement
    {
        [ConfigurationProperty("alias", IsKey = true, IsRequired = true)]
        internal string Alias
        {
            get { return (string)this["alias"]; }
        }

        [ConfigurationProperty("widthFieldAlias")]
        internal InnerTextConfigurationElement<string> WidthFieldAlias
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                       (InnerTextConfigurationElement<string>)this["widthFieldAlias"],
                    //set the default
                       "umbracoWidth");
            }
        }

        [ConfigurationProperty("heightFieldAlias")]
        internal InnerTextConfigurationElement<string> HeightFieldAlias
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                       (InnerTextConfigurationElement<string>)this["heightFieldAlias"],
                    //set the default
                       "umbracoHeight");
            }
        }

        [ConfigurationProperty("lengthFieldAlias")]
        internal InnerTextConfigurationElement<string> LengthFieldAlias
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                       (InnerTextConfigurationElement<string>)this["lengthFieldAlias"],
                    //set the default
                       "umbracoBytes");
            }
        }

        [ConfigurationProperty("extensionFieldAlias")]
        internal InnerTextConfigurationElement<string> ExtensionFieldAlias
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                       (InnerTextConfigurationElement<string>)this["extensionFieldAlias"],
                    //set the default
                       "umbracoExtension");
            }
        }
    }
}