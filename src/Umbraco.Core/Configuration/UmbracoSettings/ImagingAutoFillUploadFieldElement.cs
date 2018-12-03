using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ImagingAutoFillUploadFieldElement : UmbracoConfigurationElement, IImagingAutoFillUploadField
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
            get { return GetOptionalTextElement("widthFieldAlias", "umbracoWidth"); }
        }

        [ConfigurationProperty("heightFieldAlias")]
        internal InnerTextConfigurationElement<string> HeightFieldAlias
        {
            get { return GetOptionalTextElement("heightFieldAlias", "umbracoHeight"); }
        }

        [ConfigurationProperty("lengthFieldAlias")]
        internal InnerTextConfigurationElement<string> LengthFieldAlias
        {
            get { return GetOptionalTextElement("lengthFieldAlias", "umbracoBytes"); }
        }

        [ConfigurationProperty("extensionFieldAlias")]
        internal InnerTextConfigurationElement<string> ExtensionFieldAlias
        {
            get { return GetOptionalTextElement("extensionFieldAlias", "umbracoExtension"); }
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
