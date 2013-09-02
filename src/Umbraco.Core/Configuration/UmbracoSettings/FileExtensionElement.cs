using System.Xml.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class FileExtensionElement : InnerTextConfigurationElement<string>
    {
        public FileExtensionElement()
        {
        }

        public FileExtensionElement(XElement rawXml) : base(rawXml)
        {
        }
    }
}