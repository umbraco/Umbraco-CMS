using System.Xml.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class FileExtensionElement : InnerTextConfigurationElement<string>, IFileExtension
    {
        public FileExtensionElement()
        {
        }

        internal FileExtensionElement(XElement rawXml)
            : base(rawXml)
        {
        }
        
        string IFileExtension.Extension
        {
            get { return Value; }
        }
    }
}