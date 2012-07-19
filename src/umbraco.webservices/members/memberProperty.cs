using System;
using System.Xml.Serialization;

namespace umbraco.webservices.members
{
    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    public class memberProperty
    {
        private string key;
        private object propertyValue;

        public memberProperty()
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