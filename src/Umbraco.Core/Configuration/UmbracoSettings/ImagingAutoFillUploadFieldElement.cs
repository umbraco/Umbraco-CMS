using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ImagingAutoFillUploadFieldElement : ConfigurationElement, IImagingAutoFillUploadField
    {
        /// <summary>
        /// Allow setting internally so we can create a default
        /// </summary>
        [ConfigurationProperty("alias", IsKey = true, IsRequired = true)]
        public string Alias
        {
            get { return (string)this["alias"]; }
            set { this["alias"] = value; }
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

        string IImagingAutoFillUploadField.Alias
        {
            get { return Alias; }
            
        }

        string IImagingAutoFillUploadField.WidthFieldAlias
        {
            get { return WidthFieldAlias; }
        }

        string IImagingAutoFillUploadField.HeightFieldAlias
        {
            get { return HeightFieldAlias; }
        }

        string IImagingAutoFillUploadField.LengthFieldAlias
        {
            get { return LengthFieldAlias; }
        }

        string IImagingAutoFillUploadField.ExtensionFieldAlias
        {
            get { return ExtensionFieldAlias; }
        }
    }
}