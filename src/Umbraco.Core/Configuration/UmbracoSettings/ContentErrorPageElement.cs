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

        public bool HasContentId => ContentId != int.MinValue;

        public bool HasContentKey => ContentKey != Guid.Empty;

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

        public string ContentXPath => Value;

        public string Culture
        {
            get => (string) RawXml.Attribute("culture");
            set => RawXml.Attribute("culture").Value = value;
        }
    }
}
