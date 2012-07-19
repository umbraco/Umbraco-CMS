using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace umbraco.webservices.media
{
    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    public class mediaCarrier
    {

        public int Id
        {
            get;
            set;
        }
        public string Text
        {
            get;
            set;

        }

        public string TypeAlias
        {
            get;
            set;
        }

        public int TypeId
        {
            get;
            set;
        }

        public DateTime CreateDateTime
        {
            get;
            set;
        }

        public Boolean HasChildren
        {
            get;
            set;
        }

        public int Level
        {
            get;
            set;
        }

        public int ParentId
        {
            get;
            set;

        }

        public string Path
        {
            get;
            set;
        }

        public int SortOrder
        {
            get;
            set;
        }

        public List<mediaProperty> MediaProperties
        {
            get;
            set;
        }

        public mediaCarrier()
        {
            MediaProperties = new List<mediaProperty>();
        }
    }
}