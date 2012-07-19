using System.Xml.Serialization;

namespace umbraco.webservices.media
{
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    public class mediaProperty
    {
        public mediaProperty()
        {
        }

        public object PropertyValue
        {
            get;
            set;
        }

        public string Key
        {
            get;
            set;
        }
    }
}