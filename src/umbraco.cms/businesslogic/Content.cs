using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using umbraco.DataLayer;
using System.Runtime.CompilerServices;
using umbraco.cms.helpers;
using File = System.IO.File;

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

        }



        /// <summary>
        /// Removes the Xml cached in the database - unpublish and cleaning
        /// </summary>
        public virtual void XmlRemoveFromDB()
        {
            SqlHelper.ExecuteNonQuery("delete from cmsContentXml where nodeId = @nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));
        }


       

        /// <summary>
        /// Deletes the current Content object, must be overridden in the child class.
        /// </summary>
        public override void delete()
        {

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

            if (_contentType == null)
                _contentType = ContentType.GetContentType(InitContentType);

            _version = InitVersion;
            _versionDate = InitVersionDate;
            _contentTypeIcon = InitContentTypeIcon;
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

       
        #endregion

        #region Private Methods



        /// <summary>
        /// Optimized method for bulk deletion of properties´on a Content object.
        /// </summary>
        protected void deleteAllProperties()
        {
            SqlHelper.ExecuteNonQuery("Delete from cmsPropertyData where contentNodeId = @nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));
        }

        #endregion

     
    }
}