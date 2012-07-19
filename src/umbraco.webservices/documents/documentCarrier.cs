using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace umbraco.webservices.documents
{
    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    public class documentCarrier
    {

        public enum EPublishAction
        {
            Ignore,
            Publish,
            Unpublish
        };

        public enum EPublishStatus
        {
            Published,
            NotPublished
        };

        public documentCarrier()
        {
            DocumentProperties = new List<documentProperty>();
        }

        private int id;
        private string name;
        private List<documentProperty> documentProperties;
        private int documentTypeID;
        private int parentID;
        private bool hasChildren;

        private EPublishAction publishAction;
        private bool published;
        private DateTime releaseDate;
        private DateTime expireDate;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public List<documentProperty> DocumentProperties
        {
            get { return documentProperties; }
            set { documentProperties = value; }
        }

        public int DocumentTypeID
        {
            get { return documentTypeID; }
            set { documentTypeID = value; }
        }

        public int ParentID
        {
            get { return parentID; }
            set { parentID = value; }
        }

        public bool HasChildren
        {
            get { return hasChildren; }
            set { hasChildren = value; }
        }

        public EPublishAction PublishAction
        {
            get { return publishAction; }
            set { publishAction = value; }
        }

        public bool Published
        {
            get { return published; }
            set { published = value; }
        }

        public DateTime ReleaseDate
        {
            get { return releaseDate; }
            set { releaseDate = value; }
        }

        public DateTime ExpireDate
        {
            get { return expireDate; }
            set { expireDate = value; }
        }



    }
}