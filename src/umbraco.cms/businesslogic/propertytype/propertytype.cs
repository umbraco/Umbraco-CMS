using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;

using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using Umbraco.Core.DI;
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
            get { return LegacySqlHelper.SqlHelper; }
        }

        #region Constructors

        public PropertyType(int id)
        {
            using (var sqlHelper = LegacySqlHelper.SqlHelper)
            using (IRecordsReader dr = sqlHelper.ExecuteReader(
                "Select mandatory, DataTypeId, propertyTypeGroupId, ContentTypeId, sortOrder, alias, name, validationRegExp, description from cmsPropertyType where id=@id",
                sqlHelper.CreateParameter("@id", id)))
            {
                if (!dr.Read())
                    throw new ArgumentException("Propertytype with id: " + id + " doesnt exist!");

                _mandatory = dr.GetBoolean("mandatory");
                _id = id;

                if (!dr.IsNull("propertyTypeGroupId"))
                {
                    _propertyTypeGroup = dr.GetInt("propertyTypeGroupId");
                    //TODO: Remove after refactoring!
                    _tabId = _propertyTypeGroup;
                }

                _sortOrder = dr.GetInt("sortOrder");
                _alias = dr.GetString("alias");
                _name = dr.GetString("Name");
                _validationRegExp = dr.GetString("validationRegExp");
                _DataTypeId = dr.GetInt("DataTypeId");
                _contenttypeid = dr.GetInt("contentTypeId");
                _description = dr.GetString("description");
            }
        }

        #endregion

        #region Properties


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
                InvalidateCache();
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

                using (var sqlHelper = LegacySqlHelper.SqlHelper)
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
                InvalidateCache();
                using (var sqlHelper = LegacySqlHelper.SqlHelper)
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
                InvalidateCache();
                using (var sqlHelper = LegacySqlHelper.SqlHelper)
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
                            
                            if (Current.Services.LocalizationService.DictionaryItemExists(_description.Substring(1, _description.Length - 1)))
                            {
                                var di = Current.Services.LocalizationService.GetDictionaryItemByKey(_description.Substring(1, _description.Length - 1));
                                return di.GetTranslatedValue(lang.id);
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
                InvalidateCache();
                using (var sqlHelper = LegacySqlHelper.SqlHelper)
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
                InvalidateCache();
                using (var sqlHelper = LegacySqlHelper.SqlHelper)
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
                InvalidateCache();
                using (var sqlHelper = LegacySqlHelper.SqlHelper)
                    sqlHelper.ExecuteNonQuery("Update cmsPropertyType set alias = @alias where id= @id",
                        sqlHelper.CreateParameter("@alias", _alias.ToSafeAliasWithForcingCheck()),
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
                        
                        if (Current.Services.LocalizationService.DictionaryItemExists(_name.Substring(1, _name.Length - 1)))
                        {
                            var di = Current.Services.LocalizationService.GetDictionaryItemByKey(_name.Substring(1, _name.Length - 1));
                            return di.GetTranslatedValue(lang.id);
                        }
                    }

                    return "[" + _name + "]";
                }
            }
            set
            {
                _name = value;
                InvalidateCache();
                using (var sqlHelper = LegacySqlHelper.SqlHelper)
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

       

        public static PropertyType[] GetAll()
        {
            var result = GetPropertyTypes();
            return result.ToArray();
        }
        public static IEnumerable<PropertyType> GetPropertyTypes()
        {
            var result = new List<PropertyType>();
            using (var sqlHelper = LegacySqlHelper.SqlHelper)
            using (IRecordsReader dr = 
                sqlHelper.ExecuteReader("select id from cmsPropertyType order by Name"))
            {
                while (dr.Read())
                {
                    PropertyType pt = GetPropertyType(dr.GetInt("id"));
                    if (pt != null)
                        result.Add(pt);
                }
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
            using (var sqlHelper = LegacySqlHelper.SqlHelper)
            using (IRecordsReader dr = 
                sqlHelper.ExecuteReader("SELECT id FROM cmsPropertyType WHERE propertyTypeGroupId = @groupId order by SortOrder",
                    sqlHelper.CreateParameter("@groupId", groupId)))
            {
                while (dr.Read())
                {
                    PropertyType pt = GetPropertyType(dr.GetInt("id"));
                    if (pt != null)
                        result.Add(pt);
                }
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
            using (var sqlHelper = LegacySqlHelper.SqlHelper)
            using (IRecordsReader dr = 
                sqlHelper.ExecuteReader(
                    "select id, Name from cmsPropertyType where dataTypeId=@dataTypeId order by Name",
                    sqlHelper.CreateParameter("@dataTypeId", dataTypeDefId)))
            {
                while (dr.Read())
                {
                    PropertyType pt = GetPropertyType(dr.GetInt("id"));
                    if (pt != null)
                        result.Add(pt);
                }
            }
            return result.ToList();
        }

        public void delete()
        {
            // flush cache
            FlushCache();

            // Delete all properties of propertytype
            CleanPropertiesOnDeletion(_contenttypeid);

            //delete tag refs
            using (var sqlHelper = LegacySqlHelper.SqlHelper)
                sqlHelper.ExecuteNonQuery("Delete from cmsTagRelationship where propertyTypeId = " + Id);

            // Delete PropertyType ..
            using (var sqlHelper = LegacySqlHelper.SqlHelper)
                sqlHelper.ExecuteNonQuery("Delete from cmsPropertyType where id = " + Id);


            // delete cache from either master (via tabid) or current contentype
            FlushCacheBasedOnTab();
            InvalidateCache();
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
            using (var sqlHelper = LegacySqlHelper.SqlHelper)
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


        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public virtual void Save()
        {
            FlushCache();
        }

        protected virtual void FlushCache()
        {
            // clear local cache
            Current.ApplicationCache.RuntimeCache.ClearCacheItem(GetCacheKey(Id));

            // clear cache in contentype
            Current.ApplicationCache.RuntimeCache.ClearCacheItem(CacheKeys.ContentTypePropertiesCacheKey + _contenttypeid);

            //Ensure that DocumentTypes are reloaded from db by clearing cache - this similar to the Save method on DocumentType.
            //NOTE Would be nice if we could clear cache by type instead of emptying the entire cache.
            Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IContent>();
            Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IContentType>();
            Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IMedia>();
            Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IMediaType>();
            Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IMember>();
            Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IMemberType>();
        }

        public static PropertyType GetPropertyType(int id)
        {
            return Current.ApplicationCache.RuntimeCache.GetCacheItem<PropertyType>(
                GetCacheKey(id),
                timeout:        TimeSpan.FromMinutes(30),
                getCacheItem: () =>
                {
                    try
                    {
                        return new PropertyType(id);
                    }
                    catch
                    {
                        return null;
                    }
                });
        }

        private void InvalidateCache()
        {
            Current.ApplicationCache.RuntimeCache.ClearCacheItem(GetCacheKey(Id));
        }

        private static string GetCacheKey(int id)
        {
            return CacheKeys.PropertyTypeCacheKey + id;
        }

        #endregion
    }
}