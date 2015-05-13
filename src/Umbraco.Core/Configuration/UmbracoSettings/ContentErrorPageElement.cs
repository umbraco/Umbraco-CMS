using System;
using System.Xml.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentErrorPageElement : InnerTextConfigurationElement<string>, IContentErrorPage
    {
        public ContentErrorPageElement(XElement rawXml)
            : base(rawXml)
        {
        }

        public ContentErrorPageElement()
        {
            
        }

        public bool HasContentId
        {
            get { return ContentId != int.MinValue; }
        }

        public bool HasContentKey
        {
            get { return ContentKey != Guid.Empty; }
        }

        public int ContentId
        {
            get
            {
                int parsed;
                if (int.TryParse(Value, out parsed))
                {
                    return parsed;
                }
                return int.MinValue;
            }
        }

        public Guid ContentKey
        {
            get
            {
                Guid parsed;
                if (Guid.TryParse(Value, out parsed))
                {
                    return parsed;
                }
                return Guid.Empty;
            }
        }

        public string ContentXPath
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