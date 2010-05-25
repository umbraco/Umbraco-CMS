using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web;
using System.Runtime.CompilerServices;
using System.Linq;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.propertytype;
using umbraco.DataLayer;

[assembly: InternalsVisibleTo("Umbraco.Test")]

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

        ///// <summary>
        ///// Initializes a new instance of the <see cref="ContentType"/> class.
        ///// </summary>
        ///// <param name="id">The id.</param>
        ///// <param name="UseOptimizedMode">if set to <c>true</c> [use optimized mode] which loads in the data from the 
        ///// database in an optimized manner (less queries)
        ///// </param>
        //public ContentType(bool optimizedMode, int id)
        //    : base(id, optimizedMode)
        //{
        //    this._optimizedMode = optimizedMode;

        //    if (optimizedMode)
        //    {

        //    }
        //}

        /// <summary>
        /// Creates a new content type object manually.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="alias"></param>
        /// <param name="icon"></param>
        /// <param name="thumbnail"></param>
        /// <param name="masterContentType"></param>
        /// <remarks>
        /// This is like creating a ContentType node using optimized mode but this lets you set
        /// all of the properties that are initialized normally from the database.
        /// This is used for performance reasons.
        /// </remarks>
        internal ContentType(int id, string alias, string icon, string thumbnail, int? masterContentType)
            : base(id, true)
        {
            _alias = alias;
            _iconurl = icon;
            _thumbnail = thumbnail;
            if (masterContentType.HasValue)
                m_masterContentType = masterContentType.Value;
        }

        #endregion

        #region Constants and static members

        protected internal const string m_SQLOptimizedGetAll = @"
            SELECT id, createDate, trashed, parentId, nodeObjectType, nodeUser, level, path, sortOrder, uniqueID, text,
                masterContentType,Alias,icon,thumbnail,description 
            FROM umbracoNode INNER JOIN cmsContentType ON umbracoNode.id = cmsContentType.nodeId
            WHERE nodeObjectType = @nodeObjectType";

        private static readonly object m_Locker = new object();        

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static ContentType GetContentType(int id)
        {
            if (HttpRuntime.Cache[string.Format("UmbracoContentType{0}", id.ToString())] == null)
            {
                lock (m_Locker)
                {
                    //double check
                    if (HttpRuntime.Cache[string.Format("UmbracoContentType{0}", id.ToString())] == null)
                    {
                        ContentType ct = new ContentType(id);
                        HttpRuntime.Cache.Insert(string.Format("UmbracoContentType{0}", id.ToString()), ct);
                    }
                }                
            }
            return (ContentType)HttpRuntime.Cache[string.Format("UmbracoContentType{0}", id.ToString())];
        }

        /// <summary>
        /// If true, this instance uses default umbraco data only.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        private static bool usesUmbracoDataOnly(ContentType ct)
        {
            bool retVal = true;
            foreach (PropertyType pt in ct.PropertyTypes)
            {
                if (!DataTypeDefinition.IsDefaultData(pt.DataTypeDefinition.DataType.Data))
                {
                    retVal = false;
                    break;
                }
            }
            return retVal;
        }

        // This is needed, because the Tab class is protected and as such it's not possible for 
        // the PropertyType class to easily access the cache flusher
        /// <summary>
        /// Flushes the tab cache.
        /// </summary>
        /// <param name="TabId">The tab id.</param>
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
            SqlHelper.ExecuteNonQuery(
                                      "Insert into cmsContentType (nodeId,alias,icon) values (" + NodeId + ",'" + helpers.Casing.SafeAliasWithForcingCheck(Alias) +
                                      "','" + IconUrl + "')");
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
            object tmp = SqlHelper.ExecuteScalar<object>("Select tabId from cmsPropertyType where id = " + pt.Id.ToString());
            if (tmp == DBNull.Value)
                return 0;
            else return int.Parse(tmp.ToString());
        }

        #endregion

        #region Private Members

        //private bool _optimizedMode = false;
        private string _alias;
        private string _iconurl;
        private string _description;
        private string _thumbnail;
        private int m_masterContentType = 0;

        private List<int> m_AllowedChildContentTypeIDs = null;
        private List<TabI> m_VirtualTabs = null;

        private static readonly object propertyTypesCacheSyncLock = new object();

        #endregion
        
        #region Public Properties

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                SqlHelper.ExecuteNonQuery(
                                          "update cmsContentType set description = @description where nodeId = @id",
                                          SqlHelper.CreateParameter("@description", value),
                                          SqlHelper.CreateParameter("@id", Id));

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
                SqlHelper.ExecuteNonQuery(
                                          "update cmsContentType set thumbnail = @thumbnail where nodeId = @id",
                                          SqlHelper.CreateParameter("@thumbnail", value),
                                          SqlHelper.CreateParameter("@id", Id));

                FlushFromCache(Id);
            }
        }

        ///// <summary>
        ///// Gets or sets a value indicating whether [optimized mode].
        ///// </summary>
        ///// <value><c>true</c> if [optimized mode]; otherwise, <c>false</c>.</value>
        //public bool OptimizedMode
        //{
        //    get { return _optimizedMode; }
        //    set { _optimizedMode = value; }
        //}

        /// <summary>
        /// Human readable name/label
        /// </summary>
        /// <value></value>
        public override string Text
        {
            get
            {
                string tempText = base.Text;
                if (!tempText.StartsWith("#"))
                    return tempText;
                else
                {
                    Language lang =
                        Language.GetByCultureCode(Thread.CurrentThread.CurrentCulture.Name);
                    if (lang != null)
                    {
                        if (Dictionary.DictionaryItem.hasKey(tempText.Substring(1, tempText.Length - 1)))
                        {
                            Dictionary.DictionaryItem di =
                                new Dictionary.DictionaryItem(tempText.Substring(1, tempText.Length - 1));
                            return di.Value(lang.id);
                        }
                    }

                    return "[" + tempText + "]";
                }
            }
            set
            {
                base.Text = value;

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
                string cacheKey = GetPropertiesCacheKey();

                return Cache.GetCacheItem<List<PropertyType>>(cacheKey, propertyTypesCacheSyncLock,
                    TimeSpan.FromMinutes(15),
                    delegate
                    {
                        List<PropertyType> result = new List<PropertyType>();
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
                                    result.Add(pt);
                            }
                        }

                        // Get Property Types from the master content type
                        if (MasterContentType != 0)
                        {
                            foreach (PropertyType pt in ContentType.GetContentType(MasterContentType).PropertyTypes)
                            {
                                result.Add(pt);
                            }
                        }
                        return result;
                    });
            }
        }

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
                SqlHelper.ExecuteNonQuery(
                                          "update cmsContentType set alias = @alias where nodeId = @id",
                                          SqlHelper.CreateParameter("@alias", _alias),
                                          SqlHelper.CreateParameter("@id", Id));

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
                SqlHelper.ExecuteNonQuery(
                                          "update cmsContentType set icon='" + value + "' where nodeid = " + Id);
                // Remove from cache
                FlushFromCache(Id);
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
                return m_masterContentType;
            }
            set
            {
                m_masterContentType = value;

                SqlHelper.ExecuteNonQuery("update cmsContentType set masterContentType = @masterContentType where nodeId = @nodeId",
                    SqlHelper.CreateParameter("@masterContentType", value),
                    SqlHelper.CreateParameter("@nodeId", Id));

                // Remove from cache
                FlushFromCache(Id);
            }
        }

        /// <summary>
        /// Retrieve a list of all Tabs on the current ContentType
        /// </summary>
        public TabI[] getVirtualTabs
        {
            get
            {
                EnsureVirtualTabs();
                return m_VirtualTabs.ToArray();
            }
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
                if (m_AllowedChildContentTypeIDs == null)
                {
                    m_AllowedChildContentTypeIDs = new List<int>();
                    using (IRecordsReader dr = SqlHelper.ExecuteReader(
                                                                      "Select AllowedId from cmsContentTypeAllowedContentType where id=" +
                                                                      Id))
                    {
                        while (dr.Read())
                        {
                            m_AllowedChildContentTypeIDs.Add(dr.GetInt("AllowedId"));
                        }
                    }
                }

                return m_AllowedChildContentTypeIDs.ToArray();
            }
            set
            {
                m_AllowedChildContentTypeIDs = value.ToList();

                SqlHelper.ExecuteNonQuery(
                                          "delete from cmsContentTypeAllowedContentType where id=" + Id);
                foreach (int i in value)
                {
                    SqlHelper.ExecuteNonQuery(
                                              "insert into cmsContentTypeAllowedContentType (id,AllowedId) values (" +
                                              Id + "," + i + ")");
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

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            base.Save();

            // Remove from all doctypes from cache
            FlushAllFromCache();
        }

        /// <summary>
        /// Retrieve a list of all ContentTypes
        /// </summary>
        /// <returns>The list of all ContentTypes</returns>
        public ContentType[] GetAll()
        {            
            var contentTypes = new List<ContentType>();

            using (IRecordsReader dr =
                SqlHelper.ExecuteReader(m_SQLOptimizedGetAll.Trim(), SqlHelper.CreateParameter("@nodeObjectType", base.nodeObjectType)))
            {
                while (dr.Read())
                {
                    //create the ContentType object without setting up
                    ContentType ct = new ContentType(dr.Get<int>("id"), true);
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
        public void AddPropertyType(DataTypeDefinition dt, string Alias, string Name)
        {
            PropertyType pt = PropertyType.MakeNew(dt, this, Name, Alias);

            // Optimized call
            populatePropertyData(pt, this.Id);

            // Inherited content types (document types only)
            populateMasterContentTypes(pt, this.Id);

            //			foreach (Content c in Content.getContentOfContentType(this)) 
            //				c.addProperty(pt,c.Version);

            // Remove from cache
            FlushFromCache(Id);
        }

        /// <summary>
        /// Adding a PropertyType to a Tab, the Tabs are primarily used for making the 
        /// editing interface more userfriendly.
        /// </summary>
        /// <param name="pt">The PropertyType</param>
        /// <param name="TabId">The Id of the Tab</param>
        public void SetTabOnPropertyType(PropertyType pt, int TabId)
        {           
            // This is essentially just a wrapper for the property
            pt.TabId = TabId;
            //flush the content type cache, the the tab cache (why so much cache?! argh!)
            FlushFromCache(Id);
            foreach (TabI t in getVirtualTabs.ToList())
                FlushTabCache(t.Id, pt.ContentTypeId);
        }

        /// <summary>
        /// Removing a PropertyType from the associated Tab
        /// </summary>
        /// <param name="pt">The PropertyType which should be freed from its tab</param>
        public void removePropertyTypeFromTab(PropertyType pt)
        {            
            pt.TabId = 0; //this will set to null in the database.
            // Remove from cache
            FlushFromCache(Id);
        }

        /// <summary>
        /// Creates a new Tab on the Content
        /// </summary>
        /// <param name="Caption">Returns the Id of the new Tab</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public int AddVirtualTab(string Caption)
        {
            // Remove from cache
            FlushFromCache(Id);

            // Get tab count
            int tabCount = SqlHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsTab WHERE contenttypeNodeId = @nodeId",
                SqlHelper.CreateParameter("@nodeId", Id));

            // The method is synchronized
            SqlHelper.ExecuteNonQuery("INSERT INTO cmsTab (contenttypeNodeId,text,sortorder) VALUES (@nodeId,@text,@sortorder)",
                    SqlHelper.CreateParameter("@nodeId", Id),
                    SqlHelper.CreateParameter("@text", Caption),
                    SqlHelper.CreateParameter("@sortorder", tabCount + 1));
            return SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsTab");
        }

        /// <summary>
        /// Releases all PropertyTypes on tab (this does not delete the PropertyTypes) and then Deletes the Tab
        /// </summary>
        /// <param name="id">The Id of the Tab to be deleted.</param>
        public void DeleteVirtualTab(int id)
        {
            //set each property on the tab to have a tab id of zero
            this.getVirtualTabs.ToList()
                .Where(x => x.Id == id)
                .Single()
                .PropertyTypes
                .ToList()
                .ForEach(x =>
                {
                    x.TabId = 0;
                });

            SqlHelper.ExecuteNonQuery("delete from cmsTab where id =" + id);

            // Remove from cache
            FlushFromCache(Id);
        }

        /// <summary>
        /// Updates the caption of the Tab
        /// </summary>
        /// <param name="tabId">The Id of the Tab to be updated</param>
        /// <param name="Caption">The new Caption</param>
        public void SetTabName(int tabId, string Caption)
        {
            SqlHelper.ExecuteNonQuery(
                                      "Update  cmsTab set text = '" + Caption + "' where id = " + tabId);

            // Remove from cache
            FlushFromCache(Id);
        }

        /// <summary>
        /// Updates the sort order of the Tab
        /// </summary>
        /// <param name="tabId">The Id of the Tab to be updated</param>
        /// <param name="Caption">The new order number</param>
        public void SetTabSortOrder(int tabId, int sortOrder)
        {
            SqlHelper.ExecuteNonQuery(
                                      "Update  cmsTab set sortOrder = " + sortOrder + " where id = " + tabId);

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
            object o = this.PropertyTypes.Find(delegate(PropertyType pt) { return pt.Alias == alias; });

            //object o = SqlHelper.ExecuteScalar<object>(
            //    "Select id from cmsPropertyType where contentTypeId=@contentTypeId And Alias=@alias",
            //    SqlHelper.CreateParameter("@contentTypeId", this.Id),
            //    SqlHelper.CreateParameter("@alias", alias));

            if (o == null)
            {
                return null;
            }
            else
            {
                return (PropertyType)o;
            }

            //int propertyTypeId;
            //if (!int.TryParse(o.ToString(), out propertyTypeId))
            //    return null;

            //return PropertyType.GetPropertyType(propertyTypeId);
        }

        /// <summary>
        /// Deletes the current ContentType
        /// </summary>
        public override void delete()
        {
            // Remove from cache
            FlushFromCache(Id);

            // Delete all propertyTypes
            foreach (PropertyType pt in PropertyTypes)
            {
                if (pt.ContentTypeId == this.Id)
                {
                    pt.delete();
                }
            }

            // delete all tabs
            foreach (Tab t in getVirtualTabs.ToList())
            {
                if (t.ContentType == this.Id)
                {
                    t.Delete();
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

        protected void PopulateContentTypeNodeFromReader(IRecordsReader dr)
        {
            _alias = dr.GetString("Alias");
            _iconurl = dr.GetString("icon");
            if (!dr.IsNull("masterContentType"))
                m_masterContentType = dr.GetInt("masterContentType");

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

            using (IRecordsReader dr =
                SqlHelper.ExecuteReader("Select masterContentType,Alias,icon,thumbnail,description from cmsContentType where nodeid=" + Id)
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
        /// Set up the internal data of the ContentType
        /// </summary>
        [Obsolete("Use the overriden setupNode method instead. This method now calls that method")]
        protected void setupContentType()
        {
            setupNode();
        }

        /// <summary>
        /// Flushes the cache.
        /// </summary>
        /// <param name="Id">The id.</param>
        protected void FlushFromCache(int Id)
        {
            if (HttpRuntime.Cache[string.Format("UmbracoContentType{0}", Id.ToString())] != null)
            {
                lock (m_Locker)
                {
                    //double check
                    if (HttpRuntime.Cache[string.Format("UmbracoContentType{0}", Id.ToString())] != null)
                    {
                        HttpRuntime.Cache.Remove(string.Format("UmbracoContentType{0}", Id.ToString()));
                    }                    
                }
            }

            string cacheKey = GetPropertiesCacheKey();
            Cache.ClearCacheItem(cacheKey);

            ClearVirtualTabs();
        }

        protected void FlushAllFromCache()
        {
            Cache.ClearCacheByKeySearch("UmbracoContentType");
            Cache.ClearCacheByKeySearch("ContentType_PropertyTypes_Content");

            ClearVirtualTabs();
        } 

        #endregion

        #region Private Methods

        /// <summary>
        /// The cache key used to cache the properties for the content type
        /// </summary>
        /// <returns></returns>
        private string GetPropertiesCacheKey()
        {
            return "ContentType_PropertyTypes_Content:" + this.Id;
        }

        /// <summary>
        /// Clears the locally loaded tabs which forces them to be reloaded next time they requested
        /// </summary>
        private void ClearVirtualTabs()
        {
            m_VirtualTabs = null;
        }

        /// <summary>
        /// Checks if we've loaded the virtual tabs into memory and if not gets them from the databse.
        /// </summary>
        private void EnsureVirtualTabs()
        {
            //optimize, lazy load the data only one time
            if (m_VirtualTabs == null)
            {
                InitializeVirtualTabs();
            }
        }

        /// <summary>
        /// Loads the tabs into memory from the database and stores them in a local list for retreival
        /// </summary>
        private void InitializeVirtualTabs()
        {
            m_VirtualTabs = new List<TabI>();
            using (IRecordsReader dr = SqlHelper.ExecuteReader(
                                                              string.Format(
                                                                  "Select Id,text,sortOrder from cmsTab where contenttypeNodeId = {0} order by sortOrder",
                                                                  Id)))
            {
                while (dr.Read())
                {
                    m_VirtualTabs.Add(new Tab(dr.GetInt("id"), dr.GetString("text"), dr.GetInt("sortOrder"), this));
                }
            }

            // Master Content Type
            if (MasterContentType != 0)
            {
                foreach (TabI t in ContentType.GetContentType(MasterContentType).getVirtualTabs.ToList())
                {
                    m_VirtualTabs.Add(t);
                }
            }

            // sort all tabs
            m_VirtualTabs.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
        }

        private void populateMasterContentTypes(PropertyType pt, int docTypeId)
        {
            foreach (web.DocumentType docType in web.DocumentType.GetAllAsList())
            {
                if (docType.MasterContentType == docTypeId)
                {
                    populatePropertyData(pt, docType.Id);
                    populateMasterContentTypes(pt, docType.Id);
                }
            }
        }

        private void populatePropertyData(PropertyType pt, int contentTypeId)
        {
            SqlHelper.ExecuteNonQuery(
                                      "insert into cmsPropertyData (contentNodeId, versionId, propertyTypeId) select contentId, versionId, @propertyTypeId from cmsContent inner join cmsContentVersion on cmsContent.nodeId = cmsContentVersion.contentId where contentType = @contentTypeId",
                                      SqlHelper.CreateParameter("@propertyTypeId", pt.Id),
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

            /// <summary>
            /// A list of all PropertyTypes on the Tab
            /// </summary>
            PropertyType[] PropertyTypes { get; }

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
        protected class Tab : TabI
        {
            private ContentType _contenttype;
            private static object propertyTypesCacheSyncLock = new object();
            public static readonly string CacheKey = "Tab_PropertyTypes_Content:";

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

                // NH: the cachekey needs to combine the tab and the content type id, due to master content types enables sharing of tabs between child types
                string cacheKey = generateCacheKey(id, cType.Id);
                _propertytypes =
                    Cache.GetCacheItem<PropertyType[]>(cacheKey, propertyTypesCacheSyncLock, TimeSpan.FromMinutes(10),
                                                       delegate
                                                       {
                                                           List<PropertyType> tmp = new List<PropertyType>();

                                                           using (
                                                               IRecordsReader dr =
                                                                   SqlHelper.ExecuteReader(string.Format(
                                                                                               "Select id from cmsPropertyType where tabid = {0} and contentTypeId = {1} order by sortOrder",
                                                                                               _id, cType.Id)))
                                                           {
                                                               while (dr.Read())
                                                               {
                                                                   tmp.Add(
                                                                       PropertyType.GetPropertyType(
                                                                           dr.GetInt("id")));
                                                               }
                                                           }
                                                           return tmp.ToArray();
                                                       });
            }

            /// <summary>
            /// Flushes the cache.
            /// </summary>
            /// <param name="Id">The id.</param>
            public static void FlushCache(int Id, int ContentTypeId)
            {
                Cache.ClearCacheItem(generateCacheKey(Id, ContentTypeId));
            }

            private static string generateCacheKey(int tabId, int contentTypeId)
            {
                return String.Format("{0}_{1}_{2}", CacheKey, tabId, contentTypeId);
            }

            /// <summary>
            /// Deletes this instance.
            /// </summary>
            public void Delete()
            {
                SqlHelper.ExecuteNonQuery("delete from cmsTab where id = @id",
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
                    string tempCaption = SqlHelper.ExecuteScalar<string>("Select text from cmsTab where id = " + id.ToString());
                    if (!tempCaption.StartsWith("#"))
                        return tempCaption;
                    else
                    {
                        Language lang =
                            Language.GetByCultureCode(Thread.CurrentThread.CurrentCulture.Name);
                        if (lang != null)
                            return
                                new Dictionary.DictionaryItem(tempCaption.Substring(1, tempCaption.Length - 1)).Value(
                                    lang.id);
                        else
                            return "[" + tempCaption + "]";
                    }
                }
                catch
                {
                    return null;
                }
            }

            private int _id;

            private int? _sortOrder = null;

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
                        _sortOrder = SqlHelper.ExecuteScalar<int>("select sortOrder from cmsTab where id = " + _id);
                    }
                    return _sortOrder.Value;
                }
                set
                {
                    SqlHelper.ExecuteNonQuery("update cmsTab set sortOrder = " + value + " where id =" + _id);
                }
            }

            /// <summary>
            /// Moves the Tab up
            /// </summary>
            public void MoveUp()
            {
                FixTabOrder();
                // If this tab is not the first we can switch places with the previous tab 
                // hence moving it up.
                if (SortOrder > 0)
                {
                    int newsortorder = SortOrder - 1;
                    // Find the tab to switch with
                    TabI[] Tabs = _contenttype.getVirtualTabs;
                    foreach (Tab t in Tabs)
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
            public void MoveDown()
            {
                FixTabOrder();
                // If this tab is not the last tab we can switch places with the next tab 
                // hence moving it down.
                TabI[] Tabs = _contenttype.getVirtualTabs;
                if (SortOrder < Tabs.Length - 1)
                {
                    int newsortorder = SortOrder + 1;
                    // Find the tab to switch with
                    foreach (Tab t in Tabs)
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
                TabI[] Tabs = _contenttype.getVirtualTabs;
                for (int i = 0; i < Tabs.Length; i++)
                {
                    Tab t = (Tab)Tabs[i];
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

            private PropertyType[] _propertytypes;

            /// <summary>
            /// A list of PropertyTypes on the Tab
            /// </summary>
            public PropertyType[] PropertyTypes
            {
                get { return _propertytypes; }
            }

            public int ContentType
            {
                get { return _contenttype.Id; }
            }

            private string _caption;

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
                    else
                    {
                        Language lang =
                            Language.GetByCultureCode(Thread.CurrentThread.CurrentCulture.Name);
                        if (lang != null)
                        {
                            if (Dictionary.DictionaryItem.hasKey(_caption.Substring(1, _caption.Length - 1)))
                            {
                                Dictionary.DictionaryItem di =
                                    new Dictionary.DictionaryItem(_caption.Substring(1, _caption.Length - 1));
                                if (di != null)
                                    return di.Value(lang.id);
                            }
                        }

                        return "[" + _caption + "]";
                    }
                }
            }
        } 
        #endregion
        

        ///// <summary>
        ///// Analyzes the content types.
        ///// </summary>
        ///// <param name="ObjectType">Type of the object.</param>
        ///// <param name="ForceUpdate">if set to <c>true</c> [force update].</param>
        //protected void AnalyzeContentTypes(Guid ObjectType, bool ForceUpdate)
        //{
        //    if (!_analyzedContentTypes.ContainsKey(ObjectType) || ForceUpdate)
        //    {
        //        using (IRecordsReader dr = SqlHelper.ExecuteReader(
        //                                                          "select id from umbracoNode where nodeObjectType = @objectType",
        //                                                          SqlHelper.CreateParameter("@objectType", ObjectType)))
        //        {
        //            while (dr.Read())
        //            {
        //                ContentType ct = new ContentType(dr.GetInt("id"));
        //                if (!_optimizedContentTypes.ContainsKey(ct.UniqueId))
        //                    _optimizedContentTypes.Add(ct.UniqueId, false);

        //                _optimizedContentTypes[ct.UniqueId] = usesUmbracoDataOnly(ct);
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Determines whether this instance is optimized.
        ///// </summary>
        ///// <returns>
        ///// 	<c>true</c> if this instance is optimized; otherwise, <c>false</c>.
        ///// </returns>
        //protected bool IsOptimized()
        //{
        //    return (bool) _optimizedContentTypes[UniqueId];
        //}

        
    }
}
