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
            get => (string)this["alias"];
            set => this["alias"] = value;
        }

        [ConfigurationProperty("widthFieldAlias")]
        internal InnerTextConfigurationElement<string> WidthFieldAlias => GetOptionalTextElement("widthFieldAlias", "umbracoWidth");

        [ConfigurationProperty("heightFieldAlias")]
        internal InnerTextConfigurationElement<string> HeightFieldAlias => GetOptionalTextElement("heightFieldAlias", "umbracoHeight");

        [ConfigurationProperty("lengthFieldAlias")]
        internal InnerTextConfigurationElement<string> LengthFieldAlias => GetOptionalTextElement("lengthFieldAlias", "umbracoBytes");

        [ConfigurationProperty("extensionFieldAlias")]
        internal InnerTextConfigurationElement<string> ExtensionFieldAlias => GetOptionalTextElement("extensionFieldAlias", "umbracoExtension");

        string IImagingAutoFillUploadField.Alias => Alias;

        string IImagingAutoFillUploadField.WidthFieldAlias => WidthFieldAlias;

        string IImagingAutoFillUploadField.HeightFieldAlias => HeightFieldAlias;

        string IImagingAutoFillUploadField.LengthFieldAlias => LengthFieldAlias;

        string IImagingAutoFillUploadField.ExtensionFieldAlias => ExtensionFieldAlias;
    }
}
