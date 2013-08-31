using System.Xml.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentErrorPageElement : InnerTextConfigurationElement<int>
    {
        public ContentErrorPageElement(XElement rawXml)
            : base(rawXml)
        {
        }

        public ContentErrorPageElement()
        {
            
        }

        internal string Culture
        {
            get { return (string) RawXml.Attribute("culture"); }
            set { RawXml.Attribute("culture").Value = value; }
        }
    }
}