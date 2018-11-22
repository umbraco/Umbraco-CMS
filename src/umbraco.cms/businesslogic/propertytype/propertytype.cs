using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;

using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.cms.helpers;
using umbraco.DataLayer;
using umbraco.interfaces;
using DataTypeDefinition = umbraco.cms.businesslogic.datatype.DataTypeDefinition;
using Language = umbraco.cms.businesslogic.language.Language;

namespace umbraco.cms.businesslogic.propertytype
{
    /// <summary>
    /// Summary description for propertytype.
    /// </summary>
    [Obsolete("Use the ContentTypeService instead")]
    public class PropertyType
    {
        #region Declarations
        
        private readonly int _contenttypeid;
        private readonly int _id;
        private int _DataTypeId;
        private string _alias;
        private string _description = "";
        private bool _mandatory;
        private string _name;
        private int _sortOrder;
        private int _tabId;
        private int _propertyTypeGroup;
        private string _validationRegExp = "";

        #endregion

        /// <summary>
        /// Unused, please do not use
        /// </summary>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        #region Constructors

        public PropertyType(int id)
        {
            var found = ApplicationContext.Current.DatabaseContext.Database
                .SingleOrDefault<dynamic>(
                    "Select mandatory as mandatory, dataTypeId as dataTypeId, propertyTypeGroupId as propertyTypeGroupId, contentTypeId as contentTypeId, sortOrder as sortOrder, alias as alias, name as name, validationRegExp as validationRegExp, description as description from cmsPropertyType where id=@id",
                    new {id = id});

            if (found == null)
                throw new ArgumentException("Propertytype with id: " + id + " doesnt exist!");

            _mandatory = found.mandatory;
            _id = id;

            if (found.propertyTypeGroupId != null)
            {
                _propertyTypeGroup = found.propertyTypeGroupId;
                //TODO: Remove after refactoring!
                _tabId = _propertyTypeGroup;
            }

            _sortOrder = found.sortOrder;
            _alias = found.alias;
            _name = found.name;
            _validationRegExp = found.validationRegExp;
            _DataTypeId = found.dataTypeId;
            _contenttypeid = found.contentTypeId;
            _description = found.description;
        }

        #endregion

        #region Properties

        public DataTypeDefinition DataTypeDefinition
        {
            get { return DataTypeDefinition.GetDataTypeDefinition(_DataTypeId); }
            set
            {
                _DataTypeId = value.Id;
                using (var sqlHelper = Application.SqlHelper)
                    sqlHelper.ExecuteNonQuery(
                        "Update cmsPropertyType set DataTypeId = " + value.Id + " where id=" + Id);
            }
        }

        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Setting the tab id is not meant to be used directly in code. Use the ContentType SetTabOnPropertyType method instead
        /// as that will handle all of the caching properly, this will not.
        /// </summary>
        /// <remarks>
        /// Setting the tab id to a negative value will actually set the value to NULL in the database
        /// </remarks>
        [Obsolete("Use the new PropertyTypeGroup parameter", false)]
        public int TabId
        {
            get { return _tabId; }
            set
            {
                _tabId = value;
                PropertyTypeGroup = value;
            }
        }

        public int PropertyTypeGroup
        {
            get { return _propertyTypeGroup; }
            set
            {
                _propertyTypeGroup = value;
                object dbPropertyTypeGroup = value;
                if (value < 1)
                {
                    dbPropertyTypeGroup = DBNull.Value;
                }

                using (var sqlHelper = Application.SqlHelper)
                    sqlHelper.ExecuteNonQuery("Update cmsPropertyType set propertyTypeGroupId = @propertyTypeGroupId where id = @id",
                              sqlHelper.CreateParameter("@propertyTypeGroupId", dbPropertyTypeGroup),
                              sqlHelper.CreateParameter("@id", Id));
            }
        }

        public bool Mandatory
        {
            get { return _mandatory; }
            set
            {
                _mandatory = value;
                using (var sqlHelper = Application.SqlHelper)
                    sqlHelper.ExecuteNonQuery("Update cmsPropertyType set mandatory = @mandatory where id = @id",
                        sqlHelper.CreateParameter("@mandatory", value),
                        sqlHelper.CreateParameter("@id", Id));
            }
        }

        public string ValidationRegExp
        {
            get { return _validationRegExp; }
            set
            {
                _validationRegExp = value;
                using (var sqlHelper = Application.SqlHelper)
                    sqlHelper.ExecuteNonQuery("Update cmsPropertyType set validationRegExp = @validationRegExp where id = @id",
                        sqlHelper.CreateParameter("@validationRegExp", value), sqlHelper.CreateParameter("@id", Id));
            }
        }

        public string Description
        {
            get
            {
                if (_description != null)
                {
                    if (!_description.StartsWith("#"))
                        return _description;
                    else
                    {
                        Language lang = Language.GetByCultureCode(Thread.CurrentThread.CurrentCulture.Name);
                        if (lang != null)
                        {
                            if (Dictionary.DictionaryItem.hasKey(_description.Substring(1, _description.Length - 1)))
                            {
                                var di =
                                    new Dictionary.DictionaryItem(_description.Substring(1, _description.Length - 1));
                                return di.Value(lang.id);
                            }
                        }
                    }

                    return "[" + _description + "]";
                }

                return _description;
            }
            set
            {
                _description = value;
                using (var sqlHelper = Application.SqlHelper)
                    sqlHelper.ExecuteNonQuery("Update cmsPropertyType set description = @description where id = @id",
                        sqlHelper.CreateParameter("@description", value),
                        sqlHelper.CreateParameter("@id", Id));
            }
        }

        public int SortOrder
        {
            get { return _sortOrder; }
            set
            {
                _sortOrder = value;
                using (var sqlHelper = Application.SqlHelper)
                    sqlHelper.ExecuteNonQuery("Update cmsPropertyType set sortOrder = @sortOrder where id = @id",
                        sqlHelper.CreateParameter("@sortOrder", value),
                        sqlHelper.CreateParameter("@id", Id));
            }
        }

        public string Alias
        {
            get { return _alias; }
            set
            {
                _alias = value;
                using (var sqlHelper = Application.SqlHelper)
                    sqlHelper.ExecuteNonQuery("Update cmsPropertyType set alias = @alias where id= @id",
                        sqlHelper.CreateParameter("@alias", Casing.SafeAliasWithForcingCheck(_alias)),
                        sqlHelper.CreateParameter("@id", Id));
            }
        }

        public int ContentTypeId
        {
            get { return _contenttypeid; }
        }

        public string Name
        {
            get
            {
                if (!_name.StartsWith("#"))
                    return _name;
                else
                {
                    Language lang = Language.GetByCultureCode(Thread.CurrentThread.CurrentCulture.Name);
                    if (lang != null)
                    {
                        if (Dictionary.DictionaryItem.hasKey(_name.Substring(1, _name.Length - 1)))
                        {
                            var di = new Dictionary.DictionaryItem(_name.Substring(1, _name.Length - 1));
                            return di.Value(lang.id);
                        }
                    }

                    return "[" + _name + "]";
                }
            }
            set
            {
                _name = value;
                using (var sqlHelper = Application.SqlHelper)
                    sqlHelper.ExecuteNonQuery(
                        "UPDATE cmsPropertyType SET name=@name WHERE id=@id",
                        sqlHelper.CreateParameter("@name", _name),
                        sqlHelper.CreateParameter("@id", Id));
            }
        }

        #endregion

        #region Methods

        public string GetRawName()
        {
            return _name;
        }

        public string GetRawDescription()
        {
            return _description;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static PropertyType MakeNew(DataTypeDefinition dt, ContentType ct, string name, string alias)
        {
            //make sure that the alias starts with a letter
            if (string.IsNullOrEmpty(alias))
                throw new ArgumentNullException("alias");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (!Char.IsLetter(alias[0]))
                throw new ArgumentException("alias must start with a letter", "alias");

            PropertyType pt;
            try
            {
                // The method is synchronized, but we'll still look it up with an additional parameter (alias)
                using (var sqlHelper = Application.SqlHelper)
                    sqlHelper.ExecuteNonQuery(
                        "INSERT INTO cmsPropertyType (DataTypeId, ContentTypeId, alias, name) VALUES (@DataTypeId, @ContentTypeId, @alias, @name)",
                        sqlHelper.CreateParameter("@DataTypeId", dt.Id),
                        sqlHelper.CreateParameter("@ContentTypeId", ct.Id),
                        sqlHelper.CreateParameter("@alias", alias),
                        sqlHelper.CreateParameter("@name", name));

                using (var sqlHelper = Application.SqlHelper)
                    pt = new PropertyType(sqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsPropertyType WHERE alias=@alias",
                            sqlHelper.CreateParameter("@alias", alias)));
                }
            finally
            {
                // Clear cached items
                ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.PropertyTypeCacheKey);
            }

            return pt;
        }

        public static PropertyType[] GetAll()
        {
            var result = GetPropertyTypes();
            return result.ToArray();
        }
        public static IEnumerable<PropertyType> GetPropertyTypes()
        {
            var result = new List<PropertyType>();

            var propertyTypeIds = ApplicationContext.Current.DatabaseContext.Database.Fetch<int>(
                "select id from cmsPropertyType order by Name");

            foreach (var propertyTypeId in propertyTypeIds)
            {
                PropertyType pt = GetPropertyType(propertyTypeId);
                if (pt != null)
                    result.Add(pt);
            }
            
            return result;
        }

		public static IEnumerable<PropertyType> GetPropertyTypesByGroup(int groupId, List<int> contentTypeIds)
        {
            return GetPropertyTypesByGroup(groupId).Where(x => contentTypeIds.Contains(x.ContentTypeId));
        }

		public static IEnumerable<PropertyType> GetPropertyTypesByGroup(int groupId)
        {
            var result = new List<PropertyType>();

            var propertyTypeIds = ApplicationContext.Current.DatabaseContext.Database.Fetch<int>(
                "SELECT id FROM cmsPropertyType WHERE propertyTypeGroupId = @groupId order by SortOrder", new {groupId = groupId});

            foreach (var propertyTypeId in propertyTypeIds)
            {
                PropertyType pt = GetPropertyType(propertyTypeId);
                if (pt != null)
                    result.Add(pt);
            }
            
            return result;
        }

        /// <summary>
        /// Returns all property types based on the data type definition
        /// </summary>
        /// <param name="dataTypeDefId"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyType> GetByDataTypeDefinition(int dataTypeDefId)
        {
            var result = new List<PropertyType>();

            var propertyTypeIds = ApplicationContext.Current.DatabaseContext.Database.Fetch<int>(
                "select id from cmsPropertyType where dataTypeId=@dataTypeId order by Name", new {dataTypeId = dataTypeDefId});

            foreach (var propertyTypeId in propertyTypeIds)
            {
                PropertyType pt = GetPropertyType(propertyTypeId);
                if (pt != null)
                    result.Add(pt);
            }
            
            return result;
        }

        public void delete()
        {
            // flush cache
            FlushCache();

            // Delete all properties of propertytype
            CleanPropertiesOnDeletion(_contenttypeid);

            //delete tag refs
            using (var sqlHelper = Application.SqlHelper)
                sqlHelper.ExecuteNonQuery("Delete from cmsTagRelationship where propertyTypeId = " + Id);

            // Delete PropertyType ..
            using (var sqlHelper = Application.SqlHelper)
                sqlHelper.ExecuteNonQuery("Delete from cmsPropertyType where id = " + Id);


            // delete cache from either master (via tabid) or current contentype
            FlushCacheBasedOnTab();
        }

        public void FlushCacheBasedOnTab()
        {
            if (TabId != 0)
            {
                ContentType.FlushFromCache(ContentType.Tab.GetTab(TabId).ContentType);
            }
            else
            {
                ContentType.FlushFromCache(ContentTypeId);
            }
        }

        private void CleanPropertiesOnDeletion(int contentTypeId)
        {
            // first delete from all master document types
            //TODO: Verify no endless loops with mixins
            DocumentType.GetAllAsList().FindAll(dt => dt.MasterContentTypes.Contains(contentTypeId)).ForEach(
                dt => CleanPropertiesOnDeletion(dt.Id));

            //Initially Content.getContentOfContentType() was called, but because this doesn't include members we resort to sql lookups and deletes
            var tmp = new List<int>();
            using (var sqlHelper = Application.SqlHelper)
            using (IRecordsReader dr = sqlHelper.ExecuteReader("SELECT nodeId FROM cmsContent INNER JOIN umbracoNode ON cmsContent.nodeId = umbracoNode.id WHERE ContentType = @contentTypeId ORDER BY umbracoNode.text", sqlHelper.CreateParameter("contentTypeId", contentTypeId)))
            {
                while (dr.Read()) tmp.Add(dr.GetInt("nodeId"));

                foreach (var contentId in tmp)
                {
                    sqlHelper.ExecuteNonQuery("DELETE FROM cmsPropertyData WHERE PropertyTypeId =" + this.Id + " AND contentNodeId = " + contentId);
                }

                // invalidate content type cache
                ContentType.FlushFromCache(contentTypeId);
            }
        }

        public IDataType GetEditControl(object value, bool isPostBack)
        {
            IDataType dt = DataTypeDefinition.DataType;
            dt.DataEditor.Editor.ID = Alias;
            IData df = DataTypeDefinition.DataType.Data;
            (dt.DataEditor.Editor).ID = Alias;

            if (!isPostBack)
            {
                if (value != null)
                    dt.Data.Value = value;
                else
                    dt.Data.Value = "";
            }

            return dt;
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public virtual void Save()
        {
            FlushCache();
        }

        protected virtual void FlushCache()
        {

            // clear cache in contentype
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(CacheKeys.ContentTypePropertiesCacheKey + _contenttypeid);

            //Ensure that DocumentTypes are reloaded from db by clearing cache - this similar to the Save method on DocumentType.
            //NOTE Would be nice if we could clear cache by type instead of emptying the entire cache.
            ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IContent>();
            ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IContentType>();
            ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IMedia>();
            ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IMediaType>();
            ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IMember>();
            ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IMemberType>();
        }

        public static PropertyType GetPropertyType(int id)
        {
            return new PropertyType(id);
        }
        

        #endregion
    }
}