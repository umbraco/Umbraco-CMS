using System;
using System.Xml.Serialization;

namespace umbraco.webservices.documents
{
    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    public class documentProperty
    {
        private string key;
        private object propertyValue;

        public documentProperty()
        {
        }

        public object PropertyValue
        {
            get { return propertyValue; }
            set { propertyValue = value; }
        }

        public string Key
        {
            get { return key; }
            set { key = value; }
        }
    }
}