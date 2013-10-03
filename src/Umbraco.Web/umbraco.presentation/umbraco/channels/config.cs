using System;
using System.IO;
using System.Web;
using System.Xml;
using umbraco.BusinessLogic;
using Umbraco.Core.IO;

namespace umbraco.presentation.channels.businesslogic
{
    public class config
    {
        private static XmlDocument _metablogConfigFile;

        public static XmlDocument MetaBlogConfigFile
        {
            get
            {
                if (_metablogConfigFile == null)
                {
                    _metablogConfigFile = new XmlDocument();
                    _metablogConfigFile.Load(IOHelper.MapPath(SystemFiles.MetablogConfig));
                }

                return _metablogConfigFile;
            }
        }
    }

    public class Channel
    {
        public Channel(string username)
        {
            User u = new User(username);
            initialize(u.Id);
        }

        public Channel(int UserId)
        {
            initialize(UserId);
        }

        private void initialize(int UserId)
        {
            XmlDocument configFile = config.MetaBlogConfigFile;
            XmlNode channelXml = configFile.SelectSingleNode(string.Format("//channel [user = '{0}']", UserId));
            if (channelXml != null)
            {
                Id = UserId;
                User = new User(UserId);
                Name = channelXml.SelectSingleNode("./name").FirstChild.Value;
                StartNode = int.Parse(channelXml.SelectSingleNode("./startNode").FirstChild.Value);
                FullTree = bool.Parse(channelXml.SelectSingleNode("./fullTree").FirstChild.Value);
                DocumentTypeAlias = channelXml.SelectSingleNode("./documentTypeAlias").FirstChild.Value;
                if (channelXml.SelectSingleNode("./fields/categories").FirstChild != null)
                    FieldCategoriesAlias = channelXml.SelectSingleNode("./fields/categories").FirstChild.Value;
                if (channelXml.SelectSingleNode("./fields/description").FirstChild != null)
                    FieldDescriptionAlias = channelXml.SelectSingleNode("./fields/description").FirstChild.Value;
                if (channelXml.SelectSingleNode("./fields/excerpt") != null && channelXml.SelectSingleNode("./fields/excerpt").FirstChild != null)
                    FieldExcerptAlias = channelXml.SelectSingleNode("./fields/excerpt").FirstChild.Value;

                XmlNode mediaSupport = channelXml.SelectSingleNode("./mediaObjectSupport");
                ImageSupport = bool.Parse(mediaSupport.Attributes.GetNamedItem("enabled").Value);
                MediaFolder = int.Parse(mediaSupport.Attributes.GetNamedItem("folderId").Value);
                MediaTypeAlias = mediaSupport.Attributes.GetNamedItem("mediaTypeAlias").Value;
                MediaTypeFileProperty = mediaSupport.Attributes.GetNamedItem("mediaTypeFileProperty").Value;
            }
            else
                throw new ArgumentException(string.Format("No channel found for user with id: '{0}'", UserId));
        }

        public Channel()
        {
        }

        public void Save()
        {
            // update node
            XmlDocument configFile = config.MetaBlogConfigFile;
            XmlNode channelXml = null;
            if (User != null && User.Id > -1)
                channelXml = configFile.SelectSingleNode(string.Format("//channel [user = '{0}']", this.User.Id));
            if (channelXml != null)
                configFile.DocumentElement.RemoveChild(channelXml);

            // add new node
            XmlElement newChannelxml = configFile.CreateElement("channel");
            newChannelxml.AppendChild(
                xmlHelper.addTextNode(configFile, "name", Name));
            newChannelxml.AppendChild(
                xmlHelper.addTextNode(configFile, "user", User.Id.ToString()));
            newChannelxml.AppendChild(
                xmlHelper.addTextNode(configFile, "startNode", StartNode.ToString()));
            newChannelxml.AppendChild(
                xmlHelper.addTextNode(configFile, "fullTree", FullTree.ToString()));
            newChannelxml.AppendChild(
                xmlHelper.addTextNode(configFile, "documentTypeAlias", DocumentTypeAlias));
            
            // fields
            XmlElement fieldsxml = configFile.CreateElement("fields");
            fieldsxml.AppendChild(
                xmlHelper.addTextNode(configFile, "categories", FieldCategoriesAlias));
            fieldsxml.AppendChild(
                xmlHelper.addTextNode(configFile, "description", FieldDescriptionAlias));
            fieldsxml.AppendChild(
                xmlHelper.addTextNode(configFile, "excerpt", FieldExcerptAlias));
            newChannelxml.AppendChild(fieldsxml);


            // media
            XmlElement media = configFile.CreateElement("mediaObjectSupport");
            media.Attributes.Append(xmlHelper.addAttribute(configFile, "enabled", ImageSupport.ToString()));
            media.Attributes.Append(xmlHelper.addAttribute(configFile, "folderId", MediaFolder.ToString()));
            media.Attributes.Append(xmlHelper.addAttribute(configFile, "mediaTypeAlias", MediaTypeAlias));
            media.Attributes.Append(xmlHelper.addAttribute(configFile, "mediaTypeFileProperty", MediaTypeFileProperty));
            newChannelxml.AppendChild(media);
            configFile.DocumentElement.AppendChild(newChannelxml);

            configFile.Save( IOHelper.MapPath( SystemFiles.MetablogConfig ));


        }

        private string  _fieldExcerptAlias;

        public string FieldExcerptAlias
        {
            get { return _fieldExcerptAlias; }
            set { _fieldExcerptAlias = value; }
        }
	

        private string _mediaTypeFileProperty;

        public string MediaTypeFileProperty
        {
            get { return _mediaTypeFileProperty; }
            set { _mediaTypeFileProperty = value; }
        }


        private string _mediaTypeAlias;

        public string MediaTypeAlias
        {
            get { return _mediaTypeAlias; }
            set { _mediaTypeAlias = value; }
        }


        private int _mediaFolder;

        public int MediaFolder
        {
            get { return _mediaFolder; }
            set { _mediaFolder = value; }
        }


        private bool _imageSupport;

        public bool ImageSupport
        {
            get { return _imageSupport; }
            set { _imageSupport = value; }
        }


        private int _startNode;

        public int StartNode
        {
            get { return _startNode; }
            set { _startNode = value; }
        }


        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }


        private string _fieldCategoriesAlias;

        public string FieldCategoriesAlias
        {
            get { return _fieldCategoriesAlias; }
            set { _fieldCategoriesAlias = value; }
        }


        private string _fieldDescriptionAlias;

        public string FieldDescriptionAlias
        {
            get { return _fieldDescriptionAlias; }
            set { _fieldDescriptionAlias = value; }
        }


        private string _documentTypeAlias;

        public string DocumentTypeAlias
        {
            get { return _documentTypeAlias; }
            set { _documentTypeAlias = value; }
        }


        private bool _fulltree;

        public bool FullTree
        {
            get { return _fulltree; }
            set { _fulltree = value; }
        }


        private User _user;

        public User User
        {
            get { return _user; }
            set { _user = value; }
        }


        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}