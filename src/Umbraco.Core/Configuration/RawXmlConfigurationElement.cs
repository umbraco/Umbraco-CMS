using System.Configuration;
using System.Xml;
using System.Xml.Linq;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// A configuration section that simply exposes the entire raw xml of the section itself which inheritors can use
    /// to do with as they please.
    /// </summary>
    internal abstract class RawXmlConfigurationElement : ConfigurationElement
    {
        protected RawXmlConfigurationElement()
        {
            
        }

        protected RawXmlConfigurationElement(XElement rawXml)
        {
            RawXml = rawXml;
        }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            RawXml = (XElement)XNode.ReadFrom(reader);
        }

        protected XElement RawXml { get; private set; }
    }
}