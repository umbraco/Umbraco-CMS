using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using DataTypeDefinition = umbraco.cms.businesslogic.datatype.DataTypeDefinition;
using Language = umbraco.cms.businesslogic.language.Language;
using PropertyType = umbraco.cms.businesslogic.propertytype.PropertyType;

namespace umbraco.cms.businesslogic
{
    /// <summary>
    /// ContentTypes defines the datafields of Content objects of that type, it's similar to defining columns 
    /// in a database table, where the PropertyTypes on the ContentType each responds to a Column, and the Content
    /// objects is similar to a row of data, where the Properties on the Content object corresponds to the PropertyTypes
    /// on the ContentType.
    /// 
    /// Besides data definition, the ContentType also defines the sorting and grouping (in tabs) of Properties/Datafields
    /// on the Content and which Content (by ContentType) can be created as child to the Content of the ContentType.
    /// </summary>
    [Obsolete("Obsolete, Use Umbraco.Core.Models.ContentType or Umbraco.Core.Models.MediaType or  or Umbraco.Core.Models.MemberType", false)]
    public class ContentType : CMSNode
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public ContentType(int id) : base(id) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentType"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public ContentType(Guid id) : base(id) { }

        public ContentType(int id, bool noSetup) : base(id, noSetup) { }

        public ContentType(Guid id, bool noSetup) : base(id, noSetup) { }

        /// <summary>
        /// Creates a new content type object manually.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="alias"></param>
        /// <param name="icon"></param>
        /// <param name="thumbnail"></param>
        /// <param name="masterContentType"></param>
        /// <param name="isContainer"></param>
        /// <remarks>
        /// This is like creating a ContentType node using optimized mode but this lets you set
        /// all of the properties that are initialized normally from the database.
        /// This is used for performance reasons.
        /// </remarks>
        internal ContentType(int id, string alias, string icon, string thumbnail, int? masterContentType, bool? isContainer)
            : base(id, true)
        {
            _alias = alias;
            _iconurl = icon;
            _thumbnail = thumbnail;

            if (masterContentType.HasValue)
                MasterContentType = masterContentType.Value;

            if (isContainer.HasValue)
                _isContainerContentType = isContainer.Value;
        }

        internal ContentType(IContentTypeComposition contentType) : base(contentType)
        {
            ContentTypeItem = contentType;
        }

        #endregion

        #region Constants and static members

        protected internal const string m_SQLOptimizedGetAll = @"
            SELECT id, createDate, trashed, parentId, nodeObjectType, nodeUser, level, path, sortOrder, uniqueID, text,
                allowAtRoot, isContainer, Alias,icon,thumbnail,description 
            FROM umbracoNode INNER JOIN cmsContentType ON umbracoNode.id = cmsContentType.nodeId
            WHERE nodeObjectType = @nodeObjectType";

        #endregion

        #region Static Methods

        /// <summary>
        /// Used for cache so we don't have to lookup column names all the time, this is actually only used for the ChildrenAsTable methods
        /// </summary>
        private static readonly ConcurrentDictionary<string, IDictionary<string, string>> AliasToNames = new ConcurrentDictionary<string, IDictionary<string, string>>();
        private static readonly ConcurrentDictionary<System.Tuple<string, string>, Guid> PropertyTypeCache = new ConcurrentDictionary<System.Tuple<string, string>, Guid>();

        /// <summary>
        /// Returns a content type's columns alias -> name mapping
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is currently only used for ChildrenAsTable methods, caching is moved here instead of in the app logic so we can clear the cache
        /// </remarks>
        internal static IDictionary<string, string> GetAliasesAndNames(string contentTypeAlias)
        {
            return AliasToNames.GetOrAdd(contentTypeAlias, s =>
                {
                    var ct = GetByAlias(contentTypeAlias);
                    var userFields = ct.PropertyTypes.ToDictionary(x => x.Alias, x => x.Name);
                    return userFields;
                });
        }

        /// <summary>
        /// Removes the static object cache
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        public static void RemoveFromDataTypeCache(string contentTypeAlias)
        {
            var toDelete = PropertyTypeCache.Keys.Where(key => string.Equals(key.Item1, contentTypeAlias)).ToList();
            foreach (var key in toDelete)
            {
                Guid id;
                PropertyTypeCache.TryRemove(key, out id);
            }
            AliasToNames.Clear();
        }

        /// <summary>
        /// Removes the static object cache
        /// </summary>
        internal static void RemoveAllDataTypeCache()
        {
            AliasToNames.Clear();
            PropertyTypeCache.Clear();
        }

        public static Guid GetDataType(string contentTypeAlias, string propertyTypeAlias)
        {
            //propertyTypeAlias needs to be invariant, so we will store uppercase
            var key = new System.Tuple<string, string>(contentTypeAlias, propertyTypeAlias.ToUpper());


            return PropertyTypeCache.GetOrAdd(
                key,
                tuple =>
                {
                    // With 4.10 we can't do this via direct SQL as we have content type mixins
                    var controlId = Guid.Empty;
                    var ct = GetByAlias(contentTypeAlias);
                    var pt = ct.getPropertyType(propertyTypeAlias);
                    if (pt != null)
                    {
                        controlId = pt.DataTypeDefinition.DataType.Id;
                    }
                    return controlId;
                });
        }

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static ContentType GetContentType(int id)
        {
            return ApplicationContext.Current.ApplicationCache.GetCacheItem
                (string.Format("{0}{1}", CacheKeys.ContentTypeCacheKey, id),
                 TimeSpan.FromMinutes(30),
                 () => new ContentType(id));
        }

        // This is needed, because the Tab class is protected and as such it's not possible for 
        // the PropertyType class to easily access the cache flusher
        /// <summary>
        /// Flushes the tab cache.
        /// </summary>
        /// <param name="TabId">The tab id.</param>
        [Obsolete("There is no cache to flush for tabs")]
        public static void FlushTabCache(int TabId, int ContentTypeId)
        {
            Tab.FlushCache(TabId, ContentTypeId);
        }

        /// <summary>
        /// Creates a new ContentType
        /// </summary>
        /// <param name="NodeId">The CMSNode Id of the ContentType</param>
        /// <param name="Alias">The Alias of the ContentType</param>
        /// <param name="IconUrl">The Iconurl of Contents of this ContentType</param>
        protected static void Create(int NodeId, string Alias, string IconUrl)
        {
            Create(NodeId, Alias, IconUrl, true);
        }

        internal static void Create(int nodeId, string alias, string iconUrl, bool formatAlias)
        {
            SqlHelper.ExecuteNonQuery(
                                      "Insert into cmsContentType (nodeId,alias,icon) values (" + 
                                      nodeId + ",'" +
                                      (formatAlias ? helpers.Casing.SafeAliasWithForcingCheck(alias) : alias) +
                                      "','" + iconUrl + "')");
        }

        /// <summary>
        /// Initializes a ContentType object given the Alias.
        /// </summary>
        /// <param name="Alias">Alias of the content type</param>
        /// <returns>
        /// The ContentType with the corrosponding Alias
        /// </returns>
        public static ContentType GetByAlias(string Alias)
        {
            return new ContentType(SqlHelper.ExecuteScalar<int>("SELECT nodeid FROM cmsContentType WHERE alias = @alias",
                                   SqlHelper.CreateParameter("@alias", Alias)));
        }

        /// <summary>
        /// Helper method for getting the Tab id from a given PropertyType
        /// </summary>
        /// <param name="pt">The PropertyType from which to get the Tab Id</param>
        /// <returns>The Id of the Tab on which the PropertyType is placed</returns>
        public static int getTabIdFromPropertyType(PropertyType pt)
        {
            object tmp = SqlHelper.ExecuteScalar<object>("Select propertyTypeGroupId from cmsPropertyType where id = " + pt.Id.ToString());
            if (tmp == DBNull.Value)
                return 0;
            return int.Parse(tmp.ToString());
        }

        #endregion

        #region Private Members

        private bool _allowAtRoot;
        private string _alias;
        private string _iconurl;
        private string _description;
        private string _thumbnail;
        List<int> m_masterContentTypes;
        private bool _isContainerContentType;
        private List<int> _allowedChildContentTypeIDs;
        private List<TabI> _virtualTabs;

        protected internal IContentTypeComposition ContentTypeItem
        {
            get { return base.Entity as IContentTypeComposition; }
            set { base.Entity = value; }
        }

        #endregion

        #region Public Properties

        #region Regenerate Xml Structures

        /// <summary>
        /// Used to rebuild all of the xml structures for content of the current content type in the cmsContentXml table
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void RebuildXmlStructuresForContent()
        {
            //Clears all xml structures in the cmsContentXml table for the current content type
            ClearXmlStructuresForContent();
            foreach (var i in GetContentIdsForContentType())
            {
                RebuildXmlStructureForContentItem(i);
            }
        }

        /// <summary>
        /// Returns all content ids associated with this content type
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This will generally just return the content ids associated with this content type but in the case
        /// of a DocumentType where we can have inherited types, this will return all content Ids that are of 
        /// this content type or any descendant types as well.
        /// </remarks>
        internal virtual IEnumerable<int> GetContentIdsForContentType()
        {
            var ids = new List<int>();
            //get all the content item ids of the current content type
            using (var dr = SqlHelper.ExecuteReader(@"SELECT DISTINCT cmsContent.nodeId FROM cmsContent 
                                                INNER JOIN cmsContentType ON cmsContent.contentType = cmsContentType.nodeId 
                                                WHERE cmsContentType.nodeId = @nodeId",
                                                    SqlHelper.CreateParameter("@nodeId", this.Id)))
            {
                while (dr.Read())
                {
                    ids.Add(dr.GetInt("nodeId"));
                }
                dr.Close();
            }
            return ids;
        }

        /// <summary>
        /// Rebuilds the xml structure for the content item by id
        /// </summary>
        /// <param name="contentId"></param>
        /// <remarks>
        /// This is not thread safe
        /// </remarks>
        internal virtual void RebuildXmlStructureForContentItem(int contentId)
        {
            var xd = new XmlDocument();
            try
            {
                new Content(contentId).XmlGenerate(xd);
            }
            catch (Exception ee)
            {
                LogHelper.Error<ContentType>("Error generating xml", ee);
            }
        }

        /// <summary>
        /// Clears all xml structures in the cmsContentXml table for the current content type
        /// </summary>
        internal virtual void ClearXmlStructuresForContent()
        {
            //Remove all items from the cmsContentXml table that are of this current content type
            SqlHelper.ExecuteNonQuery(@"DELETE FROM cmsContentXml WHERE nodeId IN
                                        (SELECT DISTINCT cmsContent.nodeId FROM cmsContent 
                                            INNER JOIN cmsContentType ON cmsContent.contentType = cmsContentType.nodeId 
                                            WHERE cmsContentType.nodeId = @nodeId)",
                                      SqlHelper.CreateParameter("@nodeId", this.Id));
        }

        #endregion

        /// <summary>
        /// The Alias of the ContentType, is used for import/export and more human readable initialization see: GetByAlias 
        /// method.
        /// </summary>
        public string Alias
        {
            get { return _alias; }
            set
            {
                _alias = helpers.Casing.SafeAliasWithForcingCheck(value);

                // validate if alias is empty
                if (String.IsNullOrEmpty(_alias))
                {
                    throw new ArgumentOutOfRangeException("An Alias cannot be empty");
                }

                //This switches between using new vs. legacy api.
                //Note that this is currently only done to support both DocumentType and MediaType, which use the new api and MemberType that doesn't.
                if (ContentTypeItem == null)
                {
                    SqlHelper.ExecuteNonQuery("update cmsContentType set alias = @alias where nodeId = @id",
                                              SqlHelper.CreateParameter("@alias", _alias),
                                              SqlHelper.CreateParameter("@id", Id));
                }
                else
                {
                    ContentTypeItem.Alias = _alias;
                }

                // Remove from cache
                FlushFromCache(Id);
            }
        }

        /// <summary>
        /// A Content object is often (always) represented in the treeview in the Umbraco console, the ContentType defines
        /// which Icon the Content of this type is representated with.
        /// </summary>
        public string IconUrl
        {
            get { return _iconurl; }
            set
            {
                _iconurl = value;

                //This switches between using new vs. legacy api.
                //Note that this is currently only done to support both DocumentType and MediaType, which use the new api and MemberType that doesn't.
                if (ContentTypeItem == null)
                {
                    SqlHelper.ExecuteNonQuery("update cmsContentType set icon='" + value + "' where nodeid = " + Id);
                }
                else
                {
                    ContentTypeItem.Icon = _iconurl;
                }

                // Remove from cache
                FlushFromCache(Id);
            }
        }

        /// <summary>
        /// Get or Sets the Container status of the Content Type. A Container Content Type doesn't show its children in the tree,
        /// but instead adds a tab when edited showing its children in a grid
        /// </summary>
        public bool IsContainerContentType
        {
            get { return _isContainerContentType; }
            set
            {
                _isContainerContentType = value;

                //This switches between using new vs. legacy api.
                //Note that this is currently only done to support both DocumentType and MediaType, which use the new api and MemberType that doesn't.
                if (ContentTypeItem == null)
                {
                    SqlHelper.ExecuteNonQuery(
                                          "update cmsContentType set isContainer = @isContainer where nodeId = @id",
                                          SqlHelper.CreateParameter("@isContainer", value),
                                          SqlHelper.CreateParameter("@id", Id));
                }
                else
                {
                    ContentTypeItem.IsContainer = _isContainerContentType;
                }
            }
        }

        /// <summary>
        /// Gets or sets the 'allow at root' boolean
        /// </summary>
        public bool AllowAtRoot
        {
            get { return _allowAtRoot; }
            set
            {
                _allowAtRoot = value;

                //This switches between using new vs. legacy api. 
                //Note that this is currently only done to support both DocumentType and MediaType, which use the new api and MemberType that doesn't.
                if (ContentTypeItem == null)
                {
                    SqlHelper.ExecuteNonQuery(
                                          "update cmsContentType set allowAtRoot = @allowAtRoot where nodeId = @id",
                                          SqlHelper.CreateParameter("@allowAtRoot", value),
                                          SqlHelper.CreateParameter("@id", Id));
                }
                else
                {
                    ContentTypeItem.AllowedAsRoot = _allowAtRoot;
                }
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get
            {
                if (_description != null)
                {
                    if (!_description.StartsWith("#"))
                        return _description;
                    var lang = Language.GetByCultureCode(Thread.CurrentThread.CurrentCulture.Name);
                    if (lang != null)
                    {
                        if (Dictionary.DictionaryItem.hasKey(_description.Substring(1, _description.Length - 1)))
                        {
                            var di =
                                new Dictionary.DictionaryItem(_description.Substring(1, _description.Length - 1));
                            return di.Value(lang.id);
                        }
                    }

                    return "[" + _description + "]";
                }

                return _description;
            }
            set
            {
                _description = value;

                //This switches between using new vs. legacy api. 
                //Note that this is currently only done to support both DocumentType and MediaType, which use the new api and MemberType that doesn't.
                if (ContentTypeItem == null)
                {
                    SqlHelper.ExecuteNonQuery(
                                          "update cmsContentType set description = @description where nodeId = @id",
                                          SqlHelper.CreateParameter("@description", value),
                                          SqlHelper.CreateParameter("@id", Id));
                }
                else
                {
                    ContentTypeItem.Description = _description;
                }

                FlushFromCache(Id);
            }
        }

        /// <summary>
        /// Gets or sets the thumbnail.
        /// </summary>
        /// <value>The thumbnail.</value>
        public string Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                _thumbnail = value;

                //This switches between using new vs. legacy api. 
                //Note that this is currently only done to support both DocumentType and MediaType, which use the new api and MemberType that doesn't.
                if (ContentTypeItem == null)
                {
                    SqlHelper.ExecuteNonQuery(
                                          "update cmsContentType set thumbnail = @thumbnail where nodeId = @id",
                                          SqlHelper.CreateParameter("@thumbnail", value),
                                          SqlHelper.CreateParameter("@id", Id));
                }
                else
                {
                    ContentTypeItem.Thumbnail = _thumbnail;
                }

                FlushFromCache(Id);
            }
        }


        /// <summary>
        /// Human readable name/label
        /// </summary>
        /// <value></value>
        public override string Text
        {
            get
            {
                var tempText = base.Text;
                if (!tempText.StartsWith("#"))
                    return tempText;
                var lang = Language.GetByCultureCode(Thread.CurrentThread.CurrentCulture.Name);
                if (lang != null)
                {
                    if (Dictionary.DictionaryItem.hasKey(tempText.Substring(1, tempText.Length - 1)))
                    {
                        var di = new Dictionary.DictionaryItem(tempText.Substring(1, tempText.Length - 1));
                        return di.Value(lang.id);
                    }
                }

                return "[" + tempText + "]";
            }
            set
            {
                base.Text = value;

                if (ContentTypeItem != null)
                {
                    ContentTypeItem.Name = value;
                }

                // Remove from cache
                FlushFromCache(Id);
            }
        }

        //THIS SHOULD BE IENUMERABLE<PROPERTYTYPE> NOT LIST!

        /// <summary>
        /// The "datafield/column" definitions, a Content object of this type will have an equivalent
        /// list of Properties.
        /// </summary>		
        /// <remarks>
        /// Property types are are cached so any calls to this property will returne cached versions
        /// </remarks>
        public List<PropertyType> PropertyTypes
        {
            get
            {
                var cacheKey = GetPropertiesCacheKey();

                return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                    cacheKey,
                    TimeSpan.FromMinutes(15),
                    () =>
                    {
                        //MCH NOTE: For the timing being I have changed this to a dictionary to ensure that property types
                        //aren't added multiple times through the MasterContentType structure, because each level loads
                        //its own + inherited property types, which is wrong. Once we are able to fully switch to the new api
                        //this should no longer be a problem as the composition always contains a correct list of property types.
                        var result = new Dictionary<int, PropertyType>();
                        using (IRecordsReader dr =
                            SqlHelper.ExecuteReader(
                                "select id from cmsPropertyType where contentTypeId = @ctId order by sortOrder",
                                SqlHelper.CreateParameter("@ctId", Id)))
                        {
                            while (dr.Read())
                            {
                                int id = dr.GetInt("id");
                                PropertyType pt = PropertyType.GetPropertyType(id);
                                if (pt != null)
                                    result.Add(pt.Id, pt);
                            }
                        }

                        // Get Property Types from the master content type
                        if (MasterContentTypes.Count > 0)
                        {
                            foreach (var mct in MasterContentTypes)
                            {
                                var pts = GetContentType(mct).PropertyTypes;
                                foreach (var pt in pts)
                                {
                                    if (result.ContainsKey(pt.Id) == false)
                                        result.Add(pt.Id, pt);
                                }
                            }
                        }
                        return result.Select(x => x.Value).ToList();
                    });
            }
        }

        public List<int> MasterContentTypes
        {
            get
            {
                if (m_masterContentTypes == null)
                {
                    m_masterContentTypes = ContentTypeItem == null
                        ? new List<int>()
                        : ContentTypeItem.CompositionIds().ToList();

                    if (ContentTypeItem == null)
                    {
                        //TODO Make this recursive, so it looks up Masters of the Master ContentType
                        using (
                            var dr =
                                SqlHelper.ExecuteReader(
                                    @"SELECT parentContentTypeId FROM cmsContentType2ContentType WHERE childContentTypeId = @id",
                                    SqlHelper.CreateParameter("@id", Id)))
                        {
                            while (dr.Read())
                            {
                                m_masterContentTypes.Add(dr.GetInt("parentContentTypeId"));
                            }
                        }
                    }

                }
                return m_masterContentTypes;
            }
        }

        /// <summary>
        /// Gets or sets the Master Content Type for inheritance of tabs and properties.
        /// </summary>
        /// <value>The ID of the Master Content Type</value>
        public int MasterContentType
        {
            get
            {
                if (ContentTypeItem == null)
                    return 0;

                return ContentTypeItem.ParentId == -1 ? 0 : ContentTypeItem.ParentId;
            }
            set
            {
                if (value != MasterContentType)
                {

                    if (ContentTypeItem == null)
                    {
                        //Legacy
                        if (MasterContentTypes.Count > 0)
                        {
                            var masterId = MasterContentTypes[0];
                            RemoveParentContentType(masterId);
                        }

                        AddParentContentType(value);
                    }
                    else
                    {
                        ContentTypeItem.ParentId = value;

                        //Try to load the ContentType/MediaType through the new public api
                        if (nodeObjectType == new Guid(Constants.ObjectTypes.DocumentType))
                        {
                            
                        }

                        var newMaster = CallGetContentTypeMethod(value);
                        var added = ContentTypeItem.AddContentType(newMaster);
                    }
                }
            }
        }

        public void AddParentContentType(int parentContentTypeId)
        {
            if (MasterContentTypes.Contains(parentContentTypeId))
            {
                // Should we throw an exception if you try to add something that already exist?
            }
            else
            {
                if (ContentTypeItem == null)
                {
                    SqlHelper.ExecuteNonQuery(
                        "INSERT INTO [cmsContentType2ContentType] (parentContentTypeId, childContentTypeId) VALUES (@parentContentTypeId, @childContentTypeId)",
                        SqlHelper.CreateParameter("@parentContentTypeId", parentContentTypeId),
                        SqlHelper.CreateParameter("@childContentTypeId", Id));

                    MasterContentTypes.Add(parentContentTypeId);
                }
                else
                {
                    var newMaster = CallGetContentTypeMethod(parentContentTypeId);
                    var added = ContentTypeItem.AddContentType(newMaster);
                }
            }
        }

        public bool IsMaster()
        {
            return SqlHelper.ExecuteScalar<int>("select count(*) from cmsContentType2ContentType where parentContentTypeId = @parentContentTypeId",
                SqlHelper.CreateParameter("@parentContentTypeId", this.Id)) > 0;
        }

        public IEnumerable<ContentType> GetChildTypes()
        {
            var cts = new List<ContentType>();
            using (var dr = SqlHelper.ExecuteReader(@"SELECT childContentTypeId FROM cmsContentType2ContentType WHERE parentContentTypeId = @parentContentTypeId",
                SqlHelper.CreateParameter("@parentContentTypeId", Id)))
            {
                while (dr.Read())
                {
                    cts.Add(GetContentType(dr.GetInt("childContentTypeId")));
                }
            }

            return cts;
        }

        public void RemoveParentContentType(int parentContentTypeId)
        {
            if (MasterContentTypes.Contains(parentContentTypeId) == false)
            {
                // Should we throw an exception if you're trying to remove something that doesn't exist?
            }
            else
            {

                // Clean up property data (when we remove a reference we also need to remove all data relating to the doc type!
                // So that would be all propertyData that uses a propertyType from the content type with 'parentContentTypeId' and 
                // has a nodetype of this id
                var contentTypeToRemove = new ContentType(parentContentTypeId);

                RemoveMasterPropertyTypeData(contentTypeToRemove, this);

                SqlHelper.ExecuteNonQuery(
                                          "DELETE FROM [cmsContentType2ContentType] WHERE parentContentTypeId = @parentContentTypeId AND childContentTypeId = @childContentTypeId",
                                          SqlHelper.CreateParameter("@parentContentTypeId", parentContentTypeId),
                                          SqlHelper.CreateParameter("@childContentTypeId", Id));
                MasterContentTypes.Remove(parentContentTypeId);
            }
        }

        private void RemoveMasterPropertyTypeData(ContentType contentTypeToRemove, ContentType currentContentType)
        {
            foreach (var pt in contentTypeToRemove.PropertyTypes)
            {
                if (pt.ContentTypeId == contentTypeToRemove.Id)
                {
                    // before we can remove a parent content type we need to remove all data that 
                    // relates to property types
                    SqlHelper.ExecuteNonQuery(
                            @"delete cmsPropertyData from cmsPropertyData
                            inner join cmsContent on cmsContent.nodeId = cmsPropertyData.contentNodeId
                            where cmsPropertyData.propertyTypeId = @propertyType
                            and contentType = @contentType",
                        SqlHelper.CreateParameter("@contentType", currentContentType.Id),
                        SqlHelper.CreateParameter("@propertyType", pt.Id));
                }
            }

            // remove sub data too
            foreach (var ct in currentContentType.GetChildTypes())
                RemoveMasterPropertyTypeData(contentTypeToRemove, ct);
        }

        public IEnumerable<PropertyTypeGroup> PropertyTypeGroups
        {
            get { return PropertyTypeGroup.GetPropertyTypeGroupsFromContentType(Id); }
        }

        /// <summary>
        /// Retrieve a list of all Tabs on the current ContentType
        /// </summary>
        [Obsolete("Use PropertyTypeGroup methods instead", false)]
        public TabI[] getVirtualTabs
        {
            get
            {
                EnsureVirtualTabs();
                return _virtualTabs.ToArray();
            }
        }


        /// <summary>
        /// Clears the locally loaded tabs which forces them to be reloaded next time they requested
        /// </summary>
        [Obsolete("Use PropertyTypeGroup methods instead", false)]
        public void ClearVirtualTabs()
        {
            //NOTE: SD: There is no cache to clear so this doesn't actually do anything
            //NOTE: SD: There is no cache to clear so this doesn't actually do anything
            foreach (var t in getVirtualTabs)
                Tab.FlushCache(t.Id, Id);

            _virtualTabs = null;
        }

        /// <summary>
        /// The list of ContentType Id's that defines which Content (by ContentType) can be created as child 
        /// to the Content of this ContentType
        /// </summary>
        public int[] AllowedChildContentTypeIDs
        {
            get
            {
                //optimize this property so it lazy loads the data only one time.
                if (_allowedChildContentTypeIDs == null)
                {
                    _allowedChildContentTypeIDs = new List<int>();
                    using (var dr = SqlHelper.ExecuteReader("Select AllowedId from cmsContentTypeAllowedContentType where id=" + Id))
                    {
                        while (dr.Read())
                        {
                            _allowedChildContentTypeIDs.Add(dr.GetInt("AllowedId"));
                        }
                    }
                }

                return _allowedChildContentTypeIDs.ToArray();
            }
            set
            {
                _allowedChildContentTypeIDs = value.ToList();

                //This switches between using new vs. legacy api.
                //Note that this is currently only done to support both DocumentType and MediaType, which use the new api and MemberType that doesn't.
                if (ContentTypeItem == null)
                {
                    SqlHelper.ExecuteNonQuery(
                        "delete from cmsContentTypeAllowedContentType where id=" + Id);
                    foreach (int i in value)
                    {
                        SqlHelper.ExecuteNonQuery(
                            "insert into cmsContentTypeAllowedContentType (id,AllowedId) values (" +
                            Id + "," + i + ")");
                    }
                }
                else
                {
                    var list = new List<ContentTypeSort>();
                    int sort = 0;
                    foreach (var i in value)
                    {
                        int id = i;
                        list.Add(new ContentTypeSort(id, sort));
                        sort++;
                    }

                    ContentTypeItem.AllowedContentTypes = list;
                }
            }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the raw text.
        /// </summary>
        /// <returns></returns>
        public string GetRawText()
        {
            return base.Text;
        }
        public string GetRawDescription()
        {
            return _description;
        }
        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            base.Save();
        }

        /// <summary>
        /// Retrieve a list of all ContentTypes
        /// </summary>
        /// <returns>The list of all ContentTypes</returns>
        public ContentType[] GetAll()
        {
            var contentTypes = new List<ContentType>();

            using (var dr =
                SqlHelper.ExecuteReader(m_SQLOptimizedGetAll.Trim(), SqlHelper.CreateParameter("@nodeObjectType", nodeObjectType)))
            {
                while (dr.Read())
                {
                    //create the ContentType object without setting up
                    var ct = new ContentType(dr.Get<int>("id"), true);
                    //populate it's CMSNode properties
                    ct.PopulateCMSNodeFromReader(dr);
                    //populate it's ContentType properties
                    ct.PopulateContentTypeNodeFromReader(dr);

                    contentTypes.Add(ct);
                }
            }

            return contentTypes.ToArray();

        }

        /// <summary>
        /// Adding a PropertyType to the ContentType, will add a new datafield/Property on all Documents of this Type.
        /// </summary>
        /// <param name="dt">The DataTypeDefinition of the PropertyType</param>
        /// <param name="Alias">The Alias of the PropertyType</param>
        /// <param name="Name">The userfriendly name</param>
        public PropertyType AddPropertyType(DataTypeDefinition dt, string Alias, string Name)
        {
            PropertyType pt = PropertyType.MakeNew(dt, this, Name, Alias);

            // Optimized call
            PopulatePropertyData(pt, Id);

            // Inherited content types (document types only)
            PopulateMasterContentTypes(pt, Id);

            //			foreach (Content c in Content.getContentOfContentType(this)) 
            //				c.addProperty(pt,c.Version);

            // Remove from cache
            FlushFromCache(Id);

            return pt;
        }

        /// <summary>
        /// Adding a PropertyType to a Tab, the Tabs are primarily used for making the 
        /// editing interface more userfriendly.
        /// </summary>
        /// <param name="pt">The PropertyType</param>
        /// <param name="TabId">The Id of the Tab</param>
        [Obsolete("Use PropertyTypeGroup methods instead", false)]
        public void SetTabOnPropertyType(PropertyType pt, int TabId)
        {
            // This is essentially just a wrapper for the property
            pt.TabId = TabId;
            //flush the content type cache, the the tab cache (why so much cache?! argh!)
            pt.FlushCacheBasedOnTab();
        }

        /// <summary>
        /// Removing a PropertyType from the associated Tab
        /// </summary>
        /// <param name="pt">The PropertyType which should be freed from its tab</param>
        [Obsolete("Use PropertyTypeGroup methods instead", false)]
        public void removePropertyTypeFromTab(PropertyType pt)
        {
            pt.TabId = 0; //this will set to null in the database.

            if (ContentTypeItem != null)
            {
                ContentTypeItem.RemovePropertyType(pt.Alias);
            }

            // Remove from cache
            FlushFromCache(Id);
        }

        /// <summary>
        /// Creates a new Tab on the Content
        /// </summary>
        /// <param name="Caption">Returns the Id of the new Tab</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        [Obsolete("Use PropertyTypeGroup methods instead", false)]
        public int AddVirtualTab(string Caption)
        {
            // The method is synchronized
            PropertyTypeGroup ptg = new PropertyTypeGroup(0, Id, Caption);
            ptg.Save();

            // Remove from cache
            FlushFromCache(Id);

            return ptg.Id;
        }

        /// <summary>
        /// Releases all PropertyTypes on tab (this does not delete the PropertyTypes) and then Deletes the Tab
        /// </summary>
        /// <param name="id">The Id of the Tab to be deleted.</param>
        [Obsolete("Use PropertyTypeGroup methods instead", false)]
        public void DeleteVirtualTab(int id)
        {
            PropertyTypeGroup.GetPropertyTypeGroup(id).Delete();

            // Remove from cache
            FlushFromCache(Id);
        }

        /// <summary>
        /// Updates the caption of the Tab
        /// </summary>
        /// <param name="tabId">The Id of the Tab to be updated</param>
        /// <param name="Caption">The new Caption</param>
        [Obsolete("Use PropertyTypeGroup methods instead", false)]
        public void SetTabName(int tabId, string Caption)
        {
            var ptg = PropertyTypeGroup.GetPropertyTypeGroup(tabId);
            ptg.Name = Caption;
            ptg.Save();

            // Remove from cache
            FlushFromCache(Id);
        }

        /// <summary>
        /// Updates the sort order of the Tab
        /// </summary>
        /// <param name="tabId">The Id of the Tab to be updated</param>
        /// <param name="sortOrder">The new order number</param>
        [Obsolete("Use PropertyTypeGroup methods instead", false)]
        public void SetTabSortOrder(int tabId, int sortOrder)
        {
            var ptg = PropertyTypeGroup.GetPropertyTypeGroup(tabId);
            ptg.SortOrder = sortOrder;
            ptg.Save();

            // Remove from cache
            FlushFromCache(Id);
        }

        /// <summary>
        /// Retrieve a PropertyType by it's alias
        /// </summary>
        /// <param name="alias">PropertyType alias</param>
        /// <returns>The PropertyType with the given Alias</returns>
        public PropertyType getPropertyType(string alias)
        {
            // NH 22-08-08, Get from the property type stack to ensure support of master document types
            object o = PropertyTypes.Find(pt => pt.Alias == alias);

            if (o == null)
            {
                return null;
            }
            return (PropertyType)o;
        }
        /// <summary>
        /// Deletes the current ContentType
        /// </summary>
        public override void delete()
        {
            // Remove from cache
            FlushFromCache(Id);

            // Delete all propertyTypes
            foreach (var pt in PropertyTypes)
            {
                if (pt.ContentTypeId == Id)
                {
                    pt.delete();
                }
            }

            // delete all tabs
            foreach (PropertyTypeGroup ptg in PropertyTypeGroups)
            {
                if (ptg.ContentTypeId == this.Id)
                {
                    ptg.Delete();
                }
            }

            //need to delete the allowed relationships between content types
            SqlHelper.ExecuteNonQuery("delete from cmsContentTypeAllowedContentType where AllowedId=@allowedId or Id=@id",
                SqlHelper.CreateParameter("@allowedId", Id),
                SqlHelper.CreateParameter("@id", Id));

            // delete contenttype entrance
            SqlHelper.ExecuteNonQuery("Delete from cmsContentType where NodeId = " + Id);

            // delete CMSNode entrance
            base.delete();
        }

        #endregion

        #region Protected Methods

        internal protected void PopulateContentTypeFromContentTypeBase(IContentTypeComposition contentType)
        {
            _alias = contentType.Alias;
            _iconurl = contentType.Icon;
            _isContainerContentType = contentType.IsContainer;
            _allowAtRoot = contentType.AllowedAsRoot;
            _thumbnail = contentType.Thumbnail;
            _description = contentType.Description;

            /*if (ContentTypeItem == null)
                ContentTypeItem = contentType;*/
        }

        protected void PopulateContentTypeNodeFromReader(IRecordsReader dr)
        {
            _alias = dr.GetString("Alias");
            _iconurl = dr.GetString("icon");
            _isContainerContentType = dr.GetBoolean("isContainer");
            _allowAtRoot = dr.GetBoolean("allowAtRoot");

            if (!dr.IsNull("thumbnail"))
                _thumbnail = dr.GetString("thumbnail");
            if (!dr.IsNull("description"))
                _description = dr.GetString("description");
        }

        /// <summary>
        /// Set up the internal data of the ContentType
        /// </summary>
        protected override void setupNode()
        {
            base.setupNode();

            //Try to load the ContentType/MediaType through the new public api
            if (nodeObjectType == new Guid(Constants.ObjectTypes.DocumentType))
            {
                var contentType = CallGetContentTypeMethod(Id);
                if (contentType != null)
                {
                    PopulateContentTypeFromContentTypeBase(contentType);
                    return;
                }
            }
            else if (nodeObjectType == new Guid(Constants.ObjectTypes.MediaType))
            {
                var mediaType = CallGetContentTypeMethod(Id);
                if (mediaType != null)
                {
                    PopulateContentTypeFromContentTypeBase(mediaType);
                    return;
                }
            }
            else if (nodeObjectType == new Guid(Constants.ObjectTypes.MemberType))
            {
                var memberType = CallGetContentTypeMethod(Id);
                if (memberType != null)
                {
                    PopulateContentTypeFromContentTypeBase(memberType);
                    return;
                }
            }

            // TODO: Load master content types
            using (var dr = SqlHelper.ExecuteReader("Select allowAtRoot, isContainer, Alias,icon,thumbnail,description from cmsContentType where nodeid=" + Id)
                )
            {
                if (dr.Read())
                {
                    PopulateContentTypeNodeFromReader(dr);
                }
                else
                {
                    throw new ArgumentException("No Contenttype with id: " + Id);
                }
            }
        }

        /// <summary>
        /// Flushes the cache.
        /// </summary>
        /// <param name="id">The id.</param>
        public static void FlushFromCache(int id)
        {
            
            var ct = new ContentType(id);
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(string.Format("{0}{1}", CacheKeys.ContentTypeCacheKey, id));
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(ct.GetPropertiesCacheKey());
            ct.ClearVirtualTabs();

            //clear the content type from the property datatype cache used by razor
            RemoveFromDataTypeCache(ct.Alias);

            // clear anything that uses this as master content type
            if (ct.nodeObjectType == DocumentType._objectType 
                || ct.nodeObjectType == media.MediaType._objectType)
            {
                //NOTE Changed from "DocumentType.GetAllAsList().FindAll(dt => dt.MasterContentType == id)" to loading master contenttypes directly from the db.
                //Related to http://issues.umbraco.org/issue/U4-1714
                var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<ContentType2ContentTypeDto>("WHERE parentContentTypeId = @Id", new { Id = id });
                foreach (var dto in dtos)
                {
                    FlushFromCache(dto.ChildId);
                }
            }
        }

        [Obsolete("The content type cache is automatically cleared by Umbraco when a content type is saved, this method is no longer used")]
        protected internal void FlushAllFromCache()
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.ContentTypeCacheKey);
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.ContentTypePropertiesCacheKey);

            RemoveAllDataTypeCache();
            ClearVirtualTabs();
        }

        #endregion

        #region Private Methods

        private Func<int, IContentTypeComposition> CallGetContentTypeMethod
        {
            get
            {
                if (nodeObjectType == new Guid(Constants.ObjectTypes.DocumentType))
                {
                    return ApplicationContext.Current.Services.ContentTypeService.GetContentType;
                }
                if (nodeObjectType == new Guid(Constants.ObjectTypes.MediaType))
                {
                    return ApplicationContext.Current.Services.ContentTypeService.GetMediaType;
                }
                if (nodeObjectType == new Guid(Constants.ObjectTypes.MemberType))
                {
                    return ApplicationContext.Current.Services.MemberTypeService.Get;
                }

                //default to content
                return ApplicationContext.Current.Services.ContentTypeService.GetContentType;
            }
        }

        /// <summary>
        /// The cache key used to cache the properties for the content type
        /// </summary>
        /// <returns></returns>
        private string GetPropertiesCacheKey()
        {
            return CacheKeys.ContentTypePropertiesCacheKey + this.Id;
        }


        private readonly object _virtualTabLoadLock = new object();
        /// <summary>
        /// Checks if we've loaded the virtual tabs into memory and if not gets them from the databse.
        /// </summary>
        [Obsolete("Use PropertyTypeGroup methods instead", false)]
        private void EnsureVirtualTabs()
        {
            // This class can be cached and potentially shared between multiple threads.
            // Two or more threads can attempt to lazyily-load its virtual tabs at the same time.
            // If that happens, the m_VirtualTabs will contain duplicates.
            // We must prevent two threads from running InitializeVirtualTabs at the same time.
            // We must also prevent m_VirtualTabs from being modified while it is being populated.
            if (_virtualTabs == null || _virtualTabs.Count == 0)
            {
                lock (_virtualTabLoadLock)
                {
                    //optimize, lazy load the data only one time
                    if (_virtualTabs == null || _virtualTabs.Count == 0)
                    {
                        InitializeVirtualTabs();
                    }
                }
            }
        }


        /// <summary>
        /// Loads the tabs into memory from the database and stores them in a local list for retreival
        /// </summary>
        [Obsolete("Use PropertyTypeGroup methods instead", false)]
        private void InitializeVirtualTabs()
        {
            // While we are initialising, we should not use the class-scoped list, as it may be used by other threads
            var temporaryList = new List<TabI>();
            foreach (PropertyTypeGroup ptg in PropertyTypeGroups.Where(x => x.ParentId == 0 && x.ContentTypeId == this.Id))
                temporaryList.Add(new Tab(ptg.Id, ptg.Name, ptg.SortOrder, this));

            // Master Content Type
            if (MasterContentTypes.Count > 0)
            {
                foreach (var mct in MasterContentTypes)
                    temporaryList.AddRange(GetContentType(mct).getVirtualTabs.ToList());
            }


            // sort all tabs
            temporaryList.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

            // now that we aren't going to modify the list, we can set it to the class-scoped variable.
            _virtualTabs = temporaryList.DistinctBy(x => x.Id).ToList();
        }

        private static void PopulateMasterContentTypes(PropertyType pt, int docTypeId)
        {
            foreach (var docType in DocumentType.GetAllAsList())
            {
                //TODO: Check for multiple references (mixins) not causing endless loops!
                if (docType.MasterContentTypes.Contains(docTypeId))
                {
                    PopulatePropertyData(pt, docType.Id);
                    PopulateMasterContentTypes(pt, docType.Id);
                }
            }
        }

        private static void PopulatePropertyData(PropertyType pt, int contentTypeId)
        {
            // NH: PropertyTypeId inserted directly into SQL instead of as a parameter for SQL CE 4 compatibility
            SqlHelper.ExecuteNonQuery(
                                      "insert into cmsPropertyData (contentNodeId, versionId, propertyTypeId) select contentId, versionId, " + pt.Id + " from cmsContent inner join cmsContentVersion on cmsContent.nodeId = cmsContentVersion.contentId where contentType = @contentTypeId",
                                      SqlHelper.CreateParameter("@contentTypeId", contentTypeId));
        }

        #endregion

        #region Public TabI Interface
        /// <summary>
        /// An interface for the tabs, should be refactored
        /// </summary>
        public interface TabI
        {
            /// <summary>
            /// Public identifier
            /// </summary>
            int Id { get; }

            /// <summary>
            /// The text on the tab
            /// </summary>
            string Caption { get; }

            /// <summary>
            /// The sortorder of the tab
            /// </summary>
            int SortOrder { get; }

            // zb-00036 #29889 : removed PropertyTypes property (not making sense), replaced with methods

            /// <summary>
            /// Gets a list of all PropertyTypes on the Tab for a given ContentType.
            /// </summary>
            /// <remarks>This includes properties inherited from master content types.</remarks>
            /// <param name="contentTypeId">The unique identifier of the ContentType.</param>
            /// <returns>An array of PropertyType.</returns>
            PropertyType[] GetPropertyTypes(int contentTypeId);

            [Obsolete("Please use GetPropertyTypes() instead", false)]
            PropertyType[] PropertyTypes { get; }

            /// <summary>
            /// Gets a list of all PropertyTypes on the Tab for a given ContentType.
            /// </summary>
            /// <param name="contentTypeId">The unique identifier of the ContentType.</param>
            /// <param name="includeInheritedProperties">Indicates whether properties inherited from master content types should be included.</param>
            /// <returns>An array of PropertyType.</returns>
            PropertyType[] GetPropertyTypes(int contentTypeId, bool includeInheritedProperties);

            /// <summary>
            /// Gets a list if all PropertyTypes on the Tab for all ContentTypes.
            /// </summary>
            /// <returns>An IEnumerable of all the PropertyTypes.</returns>
            List<PropertyType> GetAllPropertyTypes();

            /// <summary>
            /// The contenttype
            /// </summary>
            int ContentType { get; }

            /// <summary>
            /// Method for moving the tab up
            /// </summary>
            void MoveUp();

            /// <summary>
            /// Method for retrieving the original, non processed name from the db
            /// </summary>
            /// <returns>The original, non processed name from the db</returns>
            string GetRawCaption();

            /// <summary>
            /// Method for moving the tab down
            /// </summary>
            void MoveDown();
        }
        #endregion

        #region Protected Tab Class
        /// <summary>
        /// A tab is merely a way to organize data on a ContentType to make it more
        /// human friendly
        /// </summary>
        [Obsolete("Please use PropertyTypes instead", false)]
        public class Tab : TabI
        {
            private readonly ContentType _contenttype;

            /// <summary>
            /// Initializes a new instance of the <see cref="Tab"/> class.
            /// </summary>
            /// <param name="id">The id.</param>
            /// <param name="caption">The caption.</param>
            /// <param name="sortOrder">The sort order.</param>
            /// <param name="cType">Type of the c.</param>
            public Tab(int id, string caption, int sortOrder, ContentType cType)
            {
                _id = id;
                _caption = caption;
                _sortOrder = sortOrder;
                _contenttype = cType;
            }

            public static Tab GetTab(int id)
            {
                Tab tab = null;
                // Tabs have been replaced with PropertyTypeGroups, so we use the new api to provide legacy support
                PropertyTypeGroup ptg = PropertyTypeGroup.GetPropertyTypeGroup(id);
                if (ptg != null)
                {
                    tab = new Tab(id, ptg.Name, ptg.SortOrder, new ContentType(ptg.ContentTypeId));
                }

                return tab;
            }

            // zb-00036 #29889 : Fix content tab properties.
            public PropertyType[] GetPropertyTypes(int contentTypeId)
            {
                return GetPropertyTypes(contentTypeId, true);
            }

            // zb-00036 #29889 : Fix content tab properties. Replaces getPropertyTypes, which was
            // loading all properties for the tab, causing exceptions here and there... we want
            // the properties for the tab for a given content type, and we want them in the right order.
            // Also this is public now because we removed the PropertyTypes property (not making sense).
            public PropertyType[] GetPropertyTypes(int contentTypeId, bool includeInheritedProperties)
            {
                // NH, temp fix for 4.9 to use the new PropertyTypeGroup API
                var pts = PropertyTypeGroup.GetPropertyTypeGroup(this.Id).GetPropertyTypes();

                if (includeInheritedProperties)
                {
                    // we need to 
                    var contentType = businesslogic.ContentType.GetContentType(contentTypeId);
                    return pts.Where(x => contentType.MasterContentTypes.Contains(x.ContentTypeId) || x.ContentTypeId == contentTypeId).ToArray();
                }

                return pts.Where(x => x.ContentTypeId == contentTypeId).ToArray();
            }

            // zb-00036 #29889 : yet we may want to be able to get *all* property types
            // Note: This will get ALL PropertyTypes that is associated with a specific Tab 
            // regardless of the PropertyTypes belonging to the current ContentType.
            public List<PropertyType> GetAllPropertyTypes()
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                var propertyTypeDtos = db.Fetch<PropertyTypeDto>("WHERE propertyTypeGroupId = @Id", new { Id = _id });
                var tmp = propertyTypeDtos
                            .Select(propertyTypeDto => PropertyType.GetPropertyType(propertyTypeDto.Id))
                            .ToList();

                var propertyTypeGroupDtos = db.Fetch<PropertyTypeGroupDto>("WHERE parentGroupId = @Id", new { Id = _id });
                foreach (var propertyTypeGroupDto in propertyTypeGroupDtos)
                {
                    var inheritedPropertyTypeDtos = db.Fetch<PropertyTypeDto>("WHERE propertyTypeGroupId = @Id", new { Id = propertyTypeGroupDto.Id });
                    tmp.AddRange(inheritedPropertyTypeDtos
                                     .Select(propertyTypeDto => PropertyType.GetPropertyType(propertyTypeDto.Id))
                                     .ToList());
                }

                return tmp;
            }

            /// <summary>
            /// A list of PropertyTypes on the Tab
            /// </summary>
            [Obsolete("Please use GetPropertyTypes() instead", false)]
            public PropertyType[] PropertyTypes
            {
                get { return GetPropertyTypes(ContentType, true); }
            }



            /// <summary>
            /// Flushes the cache.
            /// </summary>
            /// <param name="id">The id.</param>
            /// <param name="contentTypeId"></param>
            [Obsolete("There is no cache to flush for tabs")]
            public static void FlushCache(int id, int contentTypeId)
            {
                //at some stage there was probably caching for tabs but this hasn't been in place for some time, so this method
                //now does nothing.
            }

            /// <summary>
            /// Deletes this instance.
            /// </summary>
            public void Delete()
            {
                SqlHelper.ExecuteNonQuery("update cmsPropertyType set propertyTypeGroupId = NULL where propertyTypeGroupId = @id",
                                          SqlHelper.CreateParameter("@id", Id));
                SqlHelper.ExecuteNonQuery("delete from cmsPropertyTypeGroup where id = @id",
                                          SqlHelper.CreateParameter("@id", Id));
            }

            /// <summary>
            /// Gets the tab caption by id.
            /// </summary>
            /// <param name="id">The id.</param>
            /// <returns></returns>
            public static string GetCaptionById(int id)
            {
                try
                {
                    var tempCaption = SqlHelper.ExecuteScalar<string>("Select text from cmsPropertyTypeGroup where id = " + id.ToString());
                    if (!tempCaption.StartsWith("#"))
                        return tempCaption;
                    var lang = Language.GetByCultureCode(Thread.CurrentThread.CurrentCulture.Name);
                    if (lang != null)
                    {
                        return new Dictionary.DictionaryItem(tempCaption.Substring(1, tempCaption.Length - 1)).Value(lang.id);
                    }
                    return "[" + tempCaption + "]";
                }
                catch
                {
                    return null;
                }
            }

            /// <summary>
            /// Gets the tab caption by id.
            /// </summary>
            /// <param name="id">The id.</param>
            /// <returns></returns>
            internal static string GetRawCaptionById(int id)
            {
                try
                {
                    var tempCaption = SqlHelper.ExecuteScalar<string>("Select text from cmsPropertyTypeGroup where id = " + id);
                    return tempCaption;
                }
                catch
                {
                    return null;
                }
            }

            private readonly int _id;

            private int? _sortOrder;

            /// <summary>
            /// The sortorder of the tab
            /// </summary>
            /// <value></value>
            public int SortOrder
            {
                get
                {
                    if (!_sortOrder.HasValue)
                    {
                        _sortOrder = SqlHelper.ExecuteScalar<int>("select sortOrder from cmsPropertyTypeGroup where id = " + _id);
                    }
                    return _sortOrder.Value;
                }
                set
                {
                    SqlHelper.ExecuteNonQuery("update cmsPropertyTypeGroup set sortOrder = " + value + " where id =" + _id);
                }
            }

            /// <summary>
            /// Moves the Tab up
            /// </summary>
            [Obsolete("Please use GetPropertyTypes() instead", false)]
            public void MoveUp()
            {
                FixTabOrder();
                // If this tab is not the first we can switch places with the previous tab 
                // hence moving it up.
                if (SortOrder > 0)
                {
                    var newsortorder = SortOrder - 1;
                    // Find the tab to switch with
                    var tabs = _contenttype.getVirtualTabs;
                    foreach (Tab t in tabs)
                    {
                        if (t.SortOrder == newsortorder)
                            t.SortOrder = SortOrder;
                    }
                    SortOrder = newsortorder;
                }
            }

            /// <summary>
            /// Moves the Tab down
            /// </summary>
            [Obsolete("Please use GetPropertyTypes() instead", false)]
            public void MoveDown()
            {
                FixTabOrder();
                // If this tab is not the last tab we can switch places with the next tab 
                // hence moving it down.
                var tabs = _contenttype.getVirtualTabs;
                if (SortOrder < tabs.Length - 1)
                {
                    var newsortorder = SortOrder + 1;
                    // Find the tab to switch with
                    foreach (Tab t in tabs)
                    {
                        if (t.SortOrder == newsortorder)
                            t.SortOrder = SortOrder;
                    }
                    SortOrder = newsortorder;
                }
            }

            /// <summary>
            /// Method for retrieving the original, non processed name from the db
            /// </summary>
            /// <returns>
            /// The original, non processed name from the db
            /// </returns>
            public string GetRawCaption()
            {
                return _caption;
            }


            /// <summary>
            /// Fixes the tab order.
            /// </summary>
            private void FixTabOrder()
            {
                var tabs = _contenttype.getVirtualTabs;
                for (int i = 0; i < tabs.Length; i++)
                {
                    var t = (Tab)tabs[i];
                    t.SortOrder = i;
                }
            }


            /// <summary>
            /// Public identifier
            /// </summary>
            /// <value></value>
            public int Id
            {
                get { return _id; }
            }

            // zb-00036 #29889 : removed unused field
            // zb-00036 #29889 : removed PropertyTypes property (not making sense)

            public int ContentType
            {
                get { return _contenttype.Id; }
            }

            private readonly string _caption;

            /// <summary>
            /// The text on the tab
            /// </summary>
            /// <value></value>
            public string Caption
            {
                get
                {
                    if (!_caption.StartsWith("#"))
                        return _caption;

                    var lang = Language.GetByCultureCode(Thread.CurrentThread.CurrentCulture.Name);
                    if (lang != null)
                    {
                        if (Dictionary.DictionaryItem.hasKey(_caption.Substring(1, _caption.Length - 1)))
                        {
                            var di = new Dictionary.DictionaryItem(_caption.Substring(1, _caption.Length - 1));
                            return di.Value(lang.id);
                        }
                    }

                    return "[" + _caption + "]";
                }
            }
        }
        #endregion
    }
}
