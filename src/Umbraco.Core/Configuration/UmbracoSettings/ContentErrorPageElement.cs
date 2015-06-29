using System.Xml.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentErrorPageElement : InnerTextConfigurationElement<int>, IContentErrorPage
    {
        public ContentErrorPageElement(XElement rawXml)
            : base(rawXml)
        {
        }

        public ContentErrorPageElement()
        {
            
        }

        public int ContentId
        {
            get { return Value; }
        }

        public string Culture
        {
            get { return (string) RawXml.Attribute("culture"); }
            set { RawXml.Attribute("culture").Value = value; }
        }
    }
}