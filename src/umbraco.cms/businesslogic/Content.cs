using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using umbraco.cms.businesslogic.property;
using umbraco.DataLayer;
using System.Runtime.CompilerServices;
using umbraco.cms.helpers;
using umbraco.cms.businesslogic.datatype.controls;
using File = System.IO.File;
using Property = umbraco.cms.businesslogic.property.Property;
using PropertyType = umbraco.cms.businesslogic.propertytype.PropertyType;

namespace umbraco.cms.businesslogic
{
    /// <summary>
    /// Content is an intermediate layer between CMSNode and class'es which will use generic data.
    /// 
    /// Content is a datastructure that holds generic data defined in its corresponding ContentType. Content can in some
    /// sence be compared to a row in a database table, it's contenttype hold a definition of the columns and the Content
    /// contains the data
    /// 
    /// Note that Content data in umbraco is *not* tablular but in a treestructure.
    /// 
    /// </summary>
    [Obsolete("Obsolete, Use Umbraco.Core.Models.Content or Umbraco.Core.Models.Media", false)]
    public class Content : CMSNode
    {
        #region Private Members

        private Guid _version;
        private DateTime _versionDate;
        private XmlNode _xml;
        private bool _versionDateInitialized;
        private string _contentTypeIcon;
        private ContentType _contentType;
        private Properties _loadedProperties = null;
        protected internal IContentBase ContentBase;

        #endregion

        #region Constructors

        protected internal Content(int id, Guid version)
            : base(id, new object[] { version })
        {
        }

        public Content(int id) : base(id) { }

        protected Content(int id, bool noSetup) : base(id, noSetup) { }
       
        protected Content(Guid id) : base(id) { }

        protected Content(Guid id, bool noSetup) : base(id, noSetup) { }

        protected internal Content(IUmbracoEntity entity) : base(entity) { }

        protected internal Content(IContentBase contentBase)
            : base(contentBase)
        {
            ContentBase = contentBase;
            _version = ContentBase.Version;
            _versionDate = ContentBase.UpdateDate;
            _versionDateInitialized = true;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Retrive a list of Content sharing the ContentType
        /// </summary>
        /// <param name="ct">The ContentType</param>
        /// <returns>A list of Content objects sharing the ContentType defined.</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetContentOfContentType() or Umbraco.Core.Services.MediaService.GetMediaOfMediaType()", false)]
        public static Content[] getContentOfContentType(ContentType ct)
        {
            var list = new List<Content>();
            var content = ApplicationContext.Current.Services.ContentService.GetContentOfContentType(ct.Id);
            list.AddRange(content.OrderBy(x => x.Name).Select(x => new Content(x)));

            var media = ApplicationContext.Current.Services.MediaService.GetMediaOfMediaType(ct.Id);
            list.AddRange(media.OrderBy(x => x.Name).Select(x => new Content(x)));

            return list.ToArray();
        }

        /// <summary>
        /// Initialize a contentobject given a version.
        /// </summary>
        /// <param name="version">The version identifier</param>
        /// <returns>The Content object from the given version</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentService.GetByIdVersion() or Umbraco.Core.Services.MediaService.GetByIdVersion()", false)]
        public static Content GetContentFromVersion(Guid version)
        {
            var content = ApplicationContext.Current.Services.ContentService.GetByVersion(version);
            if (content != null)
            {
                return new Content(content);
            }

            var media = ApplicationContext.Current.Services.MediaService.GetByVersion(version);
            return new Content(media);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The current Content objects ContentType, which defines the Properties of the Content (data)
        /// </summary>
        public ContentType ContentType
        {
            get
            {
                if (_contentType == null)
                {
                    object o = SqlHelper.ExecuteScalar<object>(
                        "Select ContentType from cmsContent where nodeId=@nodeid",
                            SqlHelper.CreateParameter("@nodeid", this.Id));
                    if (o == null)
                        return null;
                    int contentTypeId;
                    if (int.TryParse(o.ToString(), out contentTypeId) == false)
                        return null;
                    try
                    {
                        _contentType = new ContentType(contentTypeId);
                    }
                    catch
                    {
                        return null;
                    }
                }
                return _contentType;
            }
            set
            {
                _contentType = value;
            }
        }

        /// <summary>
        /// The icon used in the tree - placed in this layer for performance reasons.
        /// </summary>
        /// <remarks>
        /// This is here for performance reasons only. If the _contentTypeIcon is manually set
        /// then a database call is not made to initialize the ContentType.
        /// 
        /// The data layer has slightly changed in 4.1 so that for Document and Media, the ContentType
        /// is automatically initialized with one SQL call when creating the documents/medias so using this
        /// method or the ContentType.IconUrl property when accessing the icon from Media or Document 
        /// won't affect performance.
        /// </remarks>
        public string ContentTypeIcon
        {
            get
            {
                if (_contentTypeIcon == null && this.ContentType != null)
                    _contentTypeIcon = this.ContentType.IconUrl;
                return _contentTypeIcon ?? string.Empty;
            }
            set
            {
                _contentTypeIcon = value;
            }
        }

        /// <summary>
        /// The createtimestamp on this version
        /// </summary>
        public DateTime VersionDate
        {
            get
            {
                if (!_versionDateInitialized)
                {
                    // A Media item only contains a single version (which relates to it's creation) so get this value from the media xml fragment instead
                    if (this is media.Media)
                    {
                        // get the xml fragment from cmsXmlContent
                        string xmlFragment = SqlHelper.ExecuteScalar<string>(@"SELECT [xml] FROM cmsContentXml WHERE nodeId = " + this.Id);
                        if (!string.IsNullOrWhiteSpace(xmlFragment))
                        {
                            XmlDocument xmlDocument = new XmlDocument();
                            xmlDocument.LoadXml(xmlFragment);                            

                            _versionDateInitialized = DateTime.TryParse(xmlDocument.SelectSingleNode("//*[1]").Attributes["updateDate"].Value, out _versionDate);
                        }
                    }

                    if (!_versionDateInitialized)
                    {
                        object o = SqlHelper.ExecuteScalar<object>(
                            "select VersionDate from cmsContentVersion where versionId = '" + this.Version.ToString() + "'");
                        if (o == null)
                        {
                            _versionDate = DateTime.Now;
                        }
                        else
                        {
                            _versionDateInitialized = DateTime.TryParse(o.ToString(), out _versionDate);
                        }
                    }
                }
                return _versionDate;
            }
            set
            {
                _versionDate = value;
                _versionDateInitialized = true;
            }
        }

        /// <summary>
        /// Retrieve a list of generic properties of the content
        /// </summary>
        public Properties GenericProperties
        {
            get
            {
                EnsureProperties();
                return _loadedProperties;
            }
        }

        /// <summary>
        /// Retrieve a list of generic properties of the content
        /// </summary>
        [Obsolete("Use the GenericProperties property instead")]
        public Property[] getProperties
        {
            get
            {
                EnsureProperties();
                return _loadedProperties.ToArray();
            }
        }

        /// <summary>
        /// Content is under version control, you are able to programatically create new versions
        /// </summary>
        public Guid Version
        {
            get
            {
                if (_version == Guid.Empty)
                {
                    string sql = "Select versionId from cmsContentVersion where contentID = " + this.Id +
                                 " order by id desc ";

                    using (IRecordsReader dr = SqlHelper.ExecuteReader(sql))
                    {
                        if (!dr.Read())
                            _version = Guid.Empty;
                        else
                            _version = dr.GetGuid("versionId");
                    }
                }
                return _version;
            }
            set { _version = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Used to persist object changes to the database. This ensures that the properties are re-loaded from the database.
        /// </summary>
        public override void Save()
        {            
            base.Save();

            ClearLoadedProperties();
        }

        /// <summary>
        /// Retrieve a Property given the alias
        /// </summary>
        /// <param name="alias">Propertyalias (defined in the documenttype)</param>
        /// <returns>The property with the given alias</returns>
        public virtual Property getProperty(string alias)
        {
            EnsureProperties();

            return _loadedProperties.SingleOrDefault(x => x.PropertyType.Alias == alias);
        }

        /// <summary>
        /// Retrieve a property given the propertytype
        /// </summary>
        /// <param name="pt">PropertyType</param>
        /// <returns>The property with the given propertytype</returns>
        public virtual Property getProperty(PropertyType pt)
        {
            EnsureProperties();

            return _loadedProperties.SingleOrDefault(x => x.PropertyType.Id == pt.Id);
        }

        /// <summary>
        /// Add a property to the Content
        /// </summary>
        /// <param name="pt">The PropertyType of the Property</param>
        /// <param name="versionId">The version of the document on which the property should be add'ed</param>
        /// <returns>The new Property</returns>
        public virtual Property addProperty(PropertyType pt, Guid versionId)
        {
            ClearLoadedProperties();
            
            return property.Property.MakeNew(pt, this, versionId);

        }

        /// <summary>
        /// An Xmlrepresentation of a Content object.
        /// </summary>
        /// <param name="xd">Xmldocument context</param>
        /// <param name="Deep">If true, the Contents children are appended to the Xmlnode recursive</param>
        /// <returns>The Xmlrepresentation of the data on the Content object</returns>
        public override XmlNode ToXml(XmlDocument xd, bool Deep)
        {
            if (_xml == null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                // we add a try/catch clause here, as the xmlreader will throw an exception if there's no xml in the table
                // after the empty catch we'll generate the xml which is why we don't do anything in the catch part
                try
                {
                    XmlReader xr = SqlHelper.ExecuteXmlReader("select xml from cmsContentXml where nodeID = " + this.Id.ToString());
                    if (xr.MoveToContent() != System.Xml.XmlNodeType.None)
                    {
                        xmlDoc.Load(xr);
                        _xml = xmlDoc.FirstChild;
                    }
                    xr.Close();
                }
                catch
                {
                }


                // Generate xml if xml still null (then it hasn't been initialized before)
                if (_xml == null)
                {
                    this.XmlGenerate(new XmlDocument());
                    _xml = importXml();
                }

            }

            XmlNode x = xd.ImportNode(_xml, true);

            if (Deep)
            {
                var childs = this.Children;
                foreach (BusinessLogic.console.IconI c in childs)
                {
                    try
                    {
                        x.AppendChild(new Content(c.Id).ToXml(xd, true));
                    }
                    catch (Exception mExp)
                    {
                        System.Web.HttpContext.Current.Trace.Warn("Content", "Error adding node to xml: " + mExp.ToString());
                    }
                }
            }

            return x;

        }

        /// <summary>
        /// Removes the Xml cached in the database - unpublish and cleaning
        /// </summary>
        public virtual void XmlRemoveFromDB()
        {
            SqlHelper.ExecuteNonQuery("delete from cmsContentXml where nodeId = @nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));
        }

        /// <summary>
        /// Generates the Content XmlNode
        /// </summary>
        /// <param name="xd"></param>
        public virtual void XmlGenerate(XmlDocument xd)
        {
            SaveXmlDocument(generateXmlWithoutSaving(xd));
        }

        public virtual void XmlPopulate(XmlDocument xd, ref XmlNode x, bool Deep)
        {
            var props = this.GenericProperties;
            foreach (property.Property p in props)
                if (p != null && p.Value != null && string.IsNullOrEmpty(p.Value.ToString()) == false)
                    x.AppendChild(p.ToXml(xd));

            // attributes
            x.Attributes.Append(XmlHelper.AddAttribute(xd, "id", this.Id.ToString()));
            x.Attributes.Append(XmlHelper.AddAttribute(xd, "key", this.UniqueId.ToString()));
            x.Attributes.Append(XmlHelper.AddAttribute(xd, "version", this.Version.ToString()));
            if (this.Level > 1)
                x.Attributes.Append(XmlHelper.AddAttribute(xd, "parentID", this.Parent.Id.ToString()));
            else
                x.Attributes.Append(XmlHelper.AddAttribute(xd, "parentID", "-1"));
            x.Attributes.Append(XmlHelper.AddAttribute(xd, "level", this.Level.ToString()));
            x.Attributes.Append(XmlHelper.AddAttribute(xd, "writerID", this.User.Id.ToString()));
            if (this.ContentType != null)
                x.Attributes.Append(XmlHelper.AddAttribute(xd, "nodeType", this.ContentType.Id.ToString()));
            x.Attributes.Append(XmlHelper.AddAttribute(xd, "template", "0"));
            x.Attributes.Append(XmlHelper.AddAttribute(xd, "sortOrder", this.sortOrder.ToString()));
            x.Attributes.Append(XmlHelper.AddAttribute(xd, "createDate", this.CreateDateTime.ToString("s")));
            x.Attributes.Append(XmlHelper.AddAttribute(xd, "updateDate", this.VersionDate.ToString("s")));
            x.Attributes.Append(XmlHelper.AddAttribute(xd, "nodeName", this.Text));
            if (this.Text != null)
                x.Attributes.Append(XmlHelper.AddAttribute(xd, "urlName", this.Text.Replace(" ", "").ToLower()));
            x.Attributes.Append(XmlHelper.AddAttribute(xd, "writerName", this.User.Name));
            if (this.ContentType != null)
                x.Attributes.Append(XmlHelper.AddAttribute(xd, "nodeTypeAlias", this.ContentType.Alias));
            x.Attributes.Append(XmlHelper.AddAttribute(xd, "path", this.Path));

            if (Deep)
            {
                //store children array here because iterating over an Array property object is very inneficient.
                var children = this.Children;
                foreach (Content c in children)
                    x.AppendChild(c.ToXml(xd, true));
            }
        }

        /// <summary>
        /// Deletes the current Content object, must be overridden in the child class.
        /// </summary>
        public override void delete()
        {
            ClearLoadedProperties();

            // Delete all data associated with this content
            this.deleteAllProperties();

            // Remove all content preview xml
            SqlHelper.ExecuteNonQuery("delete from cmsPreviewXml where nodeId = " + Id);

            // Delete version history
            SqlHelper.ExecuteNonQuery("Delete from cmsContentVersion where ContentId = " + this.Id);

            // Delete xml
            SqlHelper.ExecuteNonQuery("delete from cmsContentXml where nodeID = @nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));

            // Delete Contentspecific data ()
            SqlHelper.ExecuteNonQuery("Delete from cmsContent where NodeId = " + this.Id);

            // Delete Nodeinformation!!
            base.delete();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// This is purely for a hackity hack hack hack in order to make the new Document(id, version) constructor work because
        /// the Version property needs to be set on the object before setupNode is called, otherwise it never works!
        /// </summary>
        /// <param name="ctorArgs"></param>
        internal override void PreSetupNode(params object[] ctorArgs)
        {
            //we know that there is one ctor arg and it is a GUID since we are only calling the base
            // ctor with this overload for one purpose.
            var version = (Guid) ctorArgs[0];
            _version = version;

            base.PreSetupNode(ctorArgs);
        }

        /// <summary>
        /// Sets up the ContentType property for this content item and sets the addition content properties manually.
        /// If the ContentType property is not already set, then this will get the ContentType from Cache.
        /// </summary>
        /// <param name="InitContentType"></param>
        /// <param name="InitVersion"></param>
        /// <param name="InitVersionDate"></param>
        /// <param name="InitContentTypeIcon"></param>
        protected void InitializeContent(int InitContentType, Guid InitVersion, DateTime InitVersionDate, string InitContentTypeIcon)
        {
            ClearLoadedProperties();

            if (_contentType == null)
                _contentType = ContentType.GetContentType(InitContentType);

            _version = InitVersion;
            _versionDate = InitVersionDate;
            _contentTypeIcon = InitContentTypeIcon;
        }

        /// <summary>
        /// Creates a new Content object from the ContentType.
        /// </summary>
        /// <param name="ct"></param>
        protected virtual void CreateContent(ContentType ct)
        {
            SqlHelper.ExecuteNonQuery("insert into cmsContent (nodeId,ContentType) values (" + this.Id + "," + ct.Id + ")");
            createNewVersion(DateTime.Now);
        }
        
        /// <summary>
        /// Method for creating a new version of the data associated to the Content. 
        /// </summary>
        /// <returns>The new version Id</returns>
		protected Guid createNewVersion(DateTime versionDate = default(DateTime))
        {
			if (versionDate == default (DateTime))
			{
				versionDate = DateTime.Now;
			}

            ClearLoadedProperties();

            Guid newVersion = Guid.NewGuid();
            bool tempHasVersion = hasVersion();

            // we need to ensure that a version in the db exist before we add related data
            SqlHelper.ExecuteNonQuery("Insert into cmsContentVersion (ContentId,versionId,versionDate) values (" + this.Id + ",'" + newVersion + "', @updateDate)",
                SqlHelper.CreateParameter("@updateDate", versionDate));

            List<PropertyType> pts = ContentType.PropertyTypes;
            foreach (propertytype.PropertyType pt in pts)
            {
                object oldValue = "";
                if (tempHasVersion)
                {
                    try
                    {
                        oldValue = this.getProperty(pt.Alias).Value;
                    }
                    catch { }
                }
                property.Property p = this.addProperty(pt, newVersion);
                if (oldValue != null && oldValue.ToString() != "") p.Value = oldValue;
            }
            this.Version = newVersion;
            return newVersion;
        }

        protected virtual XmlNode generateXmlWithoutSaving(XmlDocument xd)
        {
            string nodeName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "node" : Casing.SafeAliasWithForcingCheck(ContentType.Alias);
            XmlNode x = xd.CreateNode(XmlNodeType.Element, nodeName, "");
            XmlPopulate(xd, ref x, false);
            return x;
        }

        /// <summary>
        /// Saves the XML document to the data source.
        /// </summary>
        /// <param name="node">The XML Document.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual void SaveXmlDocument(XmlNode node)
        {
            // Method is synchronized so exists remains consistent (avoiding race condition)
            bool exists = SqlHelper.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsContentXml WHERE nodeId = @nodeId",
                                           SqlHelper.CreateParameter("@nodeId", Id)) > 0;
            string query;
            if (exists)
                query = "UPDATE cmsContentXml SET xml = @xml WHERE nodeId = @nodeId";
            else
                query = "INSERT INTO cmsContentXml(nodeId, xml) VALUES (@nodeId, @xml)";
            SqlHelper.ExecuteNonQuery(query,
                                      SqlHelper.CreateParameter("@nodeId", Id),
                                      SqlHelper.CreateParameter("@xml", node.OuterXml));
        }

        /// <summary>
        /// Deletes all files and the folder that have been saved with this content item which are based on the Upload data
        /// type. This is called when a media or content tree node is deleted. 
        /// </summary>
        protected void DeleteAssociatedMediaFiles()
        {
            // Remove all files

            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var uploadField = DataTypesResolver.Current.GetById(new Guid(Constants.PropertyEditors.UploadField));
             
            foreach (Property p in GenericProperties)
            {               
                var isUploadField = false;
                try
                {
                    if (p.PropertyType.DataTypeDefinition.DataType.Id == uploadField.Id
                         && p.Value.ToString() != ""
                         && File.Exists(global::Umbraco.Core.IO.IOHelper.MapPath(p.Value.ToString())))
                    {
                        isUploadField = true;
                    }
                }
                catch (ArgumentException)
                {
                    //the data type definition may not exist anymore at this point because another thread may
                    //have deleted it.
                    isUploadField = false;
                }
                if (isUploadField)
                {
                    var relativeFilePath = fs.GetRelativePath(p.Value.ToString());
                    var parentDirectory = System.IO.Path.GetDirectoryName(relativeFilePath);

                    // don't want to delete the media folder if not using directories.
                    if (UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories && parentDirectory != fs.GetRelativePath("/"))
                    {
                        //issue U4-771: if there is a parent directory the recursive parameter should be true
                        fs.DeleteDirectory(parentDirectory, String.IsNullOrEmpty(parentDirectory) == false);
                    }
                    else
                    {
                        fs.DeleteFile(relativeFilePath, true);
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Clears the locally loaded properties which forces them to be reloaded next time they requested
        /// </summary>
        private void ClearLoadedProperties()
        {
            _loadedProperties = null;
        }

        /// <summary>
        /// Makes sure that the properties are initialized. If they are already initialized, this does nothing.
        /// </summary>
        private void EnsureProperties()
        {
            if (_loadedProperties == null)
            {
                InitializeProperties();
            }
        }

        /// <summary>
        /// Loads all properties from database into objects. If this method is re-called, it will re-query the database.
        /// </summary>
        /// <remarks>
        /// This optimizes sql calls. This will first check if all of the properties have been loaded. If not, 
        /// then it will query for all property types for the current version from the db. It will then iterate over each
        /// cmdPropertyData row and store the id and propertyTypeId in a list for lookup later. Once the records have been 
        /// read, we iterate over the cached property types for this ContentType and create a new property based on
        /// our stored list of proeprtyTypeIds. We then have a cached list of Property objects which will get returned
        /// on all subsequent calls and is also used to return a property with calls to getProperty.
        /// </remarks>
        private void InitializeProperties()
        {
            _loadedProperties = new Properties();

            if (ContentBase != null)
            {
                //NOTE: we will not load any properties where HasIdentity = false - this is because if properties are 
                // added to the property collection that aren't persisted we'll get ysods
                _loadedProperties.AddRange(ContentBase.Properties.Where(x => x.HasIdentity).Select(x => new Property(x)));
                return;
            }

            if (this.ContentType == null)
                return;

            //Create anonymous typed list with 2 props, Id and PropertyTypeId of type Int.
            //This will still be an empty list since the props list is empty.
            var propData = _loadedProperties.Select(x => new { Id = 0, PropertyTypeId = 0 }).ToList();

            string sql = @"select id, propertyTypeId from cmsPropertyData where versionId=@versionId";

            using (IRecordsReader dr = SqlHelper.ExecuteReader(sql,
                SqlHelper.CreateParameter("@versionId", Version)))
            {
                while (dr.Read())
                {
                    //add the item to our list
                    propData.Add(new { Id = dr.Get<int>("id"), PropertyTypeId = dr.Get<int>("propertyTypeId") });
                }
            }

            foreach (PropertyType pt in this.ContentType.PropertyTypes)
            {
                if (pt == null)
                    continue;

                //get the propertyId
                var property = propData.LastOrDefault(x => x.PropertyTypeId == pt.Id);
                if (property == null)
                {
                    continue;
                    //var prop = Property.MakeNew(pt, this, Version);
                    //property = new {Id = prop.Id, PropertyTypeId = pt.Id};
                }
                var propertyId = property.Id;

                Property p = null;
                try
                {
                    p = new Property(propertyId, pt);
                }
                catch
                {
                    continue; //this remains from old code... not sure why we would do this?
                }

                _loadedProperties.Add(p);
            }
        }

        /// <summary>
        /// Optimized method for bulk deletion of properties´on a Content object.
        /// </summary>
        protected void deleteAllProperties()
        {
            SqlHelper.ExecuteNonQuery("Delete from cmsPropertyData where contentNodeId = @nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));
        }

        private XmlNode importXml()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(SqlHelper.ExecuteXmlReader("select xml from cmsContentXml where nodeID = " + this.Id.ToString()));

            return xmlDoc.FirstChild;
        }

        /// <summary>
        /// Indication if the Content exists in at least one version.
        /// </summary>
        /// <returns>Returns true if the Content has a version</returns>
        private bool hasVersion()
        {
            int versionCount = SqlHelper.ExecuteScalar<int>("select Count(Id) as tmp from cmsContentVersion where contentId = " + this.Id.ToString());
            return (versionCount > 0);
        }

        #endregion

        #region XmlPreivew

        public override XmlNode ToPreviewXml(XmlDocument xd)
        {
            if (!PreviewExists(Version))
            {
                saveXmlPreview(xd);
            }
            return GetPreviewXml(xd, Version);
        }

        private void saveXmlPreview(XmlDocument xd)
        {
            SavePreviewXml(generateXmlWithoutSaving(xd), Version);
        }

        #endregion
    }
}