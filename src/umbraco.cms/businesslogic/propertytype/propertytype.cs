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

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        #region Constructors

        public PropertyType(int id)
        {
            using (IRecordsReader dr = SqlHelper.ExecuteReader(
                "Select mandatory, DataTypeId, propertyTypeGroupId, ContentTypeId, sortOrder, alias, name, validationRegExp, description from cmsPropertyType where id=@id",
                SqlHelper.CreateParameter("@id", id)))
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

        public DataTypeDefinition DataTypeDefinition
        {
            get { return DataTypeDefinition.GetDataTypeDefinition(_DataTypeId); }
            set
            {
                _DataTypeId = value.Id;
                InvalidateCache();
                SqlHelper.ExecuteNonQuery(
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
                SqlHelper.ExecuteNonQuery("Update cmsPropertyType set propertyTypeGroupId = @propertyTypeGroupId where id = @id",
                              SqlHelper.CreateParameter("@propertyTypeGroupId", dbPropertyTypeGroup),
                              SqlHelper.CreateParameter("@id", Id));
            }
        }

        public bool Mandatory
        {
            get { return _mandatory; }
            set
            {
                _mandatory = value;
                InvalidateCache();
                SqlHelper.ExecuteNonQuery(
                    "Update cmsPropertyType set mandatory = @mandatory where id = @id",
                    SqlHelper.CreateParameter("@mandatory", value),
                    SqlHelper.CreateParameter("@id", Id));
            }
        }

        public string ValidationRegExp
        {
            get { return _validationRegExp; }
            set
            {
                _validationRegExp = value;
                InvalidateCache();
                SqlHelper.ExecuteNonQuery(
                    "Update cmsPropertyType set validationRegExp = @validationRegExp where id = @id",
                    SqlHelper.CreateParameter("@validationRegExp", value), SqlHelper.CreateParameter("@id", Id));
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
                InvalidateCache();
                SqlHelper.ExecuteNonQuery(
                    "Update cmsPropertyType set description = @description where id = @id",
                    SqlHelper.CreateParameter("@description", value),
                    SqlHelper.CreateParameter("@id", Id));
            }
        }

        public int SortOrder
        {
            get { return _sortOrder; }
            set
            {
                _sortOrder = value;
                InvalidateCache();
                SqlHelper.ExecuteNonQuery(
                    "Update cmsPropertyType set sortOrder = @sortOrder where id = @id",
                    SqlHelper.CreateParameter("@sortOrder", value),
                    SqlHelper.CreateParameter("@id", Id));
            }
        }

        public string Alias
        {
            get { return _alias; }
            set
            {
                _alias = value;
                InvalidateCache();
                SqlHelper.ExecuteNonQuery("Update cmsPropertyType set alias = @alias where id= @id",
                                          SqlHelper.CreateParameter("@alias", Casing.SafeAliasWithForcingCheck(_alias)),
                                          SqlHelper.CreateParameter("@id", Id));
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
                InvalidateCache();
                SqlHelper.ExecuteNonQuery(
                    "UPDATE cmsPropertyType SET name=@name WHERE id=@id",
                    SqlHelper.CreateParameter("@name", _name),
                    SqlHelper.CreateParameter("@id", Id));
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
                SqlHelper.ExecuteNonQuery(
                    "INSERT INTO cmsPropertyType (DataTypeId, ContentTypeId, alias, name) VALUES (@DataTypeId, @ContentTypeId, @alias, @name)",
                    SqlHelper.CreateParameter("@DataTypeId", dt.Id),
                    SqlHelper.CreateParameter("@ContentTypeId", ct.Id),
                    SqlHelper.CreateParameter("@alias", alias),
                    SqlHelper.CreateParameter("@name", name));
                pt =
                    new PropertyType(
                        SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsPropertyType WHERE alias=@alias",
                                                     SqlHelper.CreateParameter("@alias", alias)));
            }
            finally
            {
                // Clear cached items
                ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.PropertyTypeCacheKey);
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
            using (IRecordsReader dr =
                SqlHelper.ExecuteReader("select id from cmsPropertyType order by Name"))
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
            using (IRecordsReader dr =
                SqlHelper.ExecuteReader("SELECT id FROM cmsPropertyType WHERE propertyTypeGroupId = @groupId order by SortOrder",
                    SqlHelper.CreateParameter("@groupId", groupId)))
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
            using (IRecordsReader dr =
                SqlHelper.ExecuteReader(
                    "select id, Name from cmsPropertyType where dataTypeId=@dataTypeId order by Name",
                    SqlHelper.CreateParameter("@dataTypeId", dataTypeDefId)))
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
            SqlHelper.ExecuteNonQuery("Delete from cmsTagRelationship where propertyTypeId = " + Id);

            // Delete PropertyType ..
            SqlHelper.ExecuteNonQuery("Delete from cmsPropertyType where id = " + Id);


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
            IRecordsReader dr = SqlHelper.ExecuteReader("SELECT nodeId FROM cmsContent INNER JOIN umbracoNode ON cmsContent.nodeId = umbracoNode.id WHERE ContentType = " + contentTypeId + " ORDER BY umbracoNode.text ");
            while (dr.Read()) tmp.Add(dr.GetInt("nodeId"));
            dr.Close();

            foreach (var contentId in tmp)
            {
                SqlHelper.ExecuteNonQuery("DELETE FROM cmsPropertyData WHERE PropertyTypeId =" + this.Id + " AND contentNodeId = " + contentId);
            }

            // invalidate content type cache
            ContentType.FlushFromCache(contentTypeId);
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
            // clear local cache
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(GetCacheKey(Id));

            // clear cache in contentype
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(CacheKeys.ContentTypePropertiesCacheKey + _contenttypeid);

            //Ensure that DocumentTypes are reloaded from db by clearing cache - this similar to the Save method on DocumentType.
            //NOTE Would be nice if we could clear cache by type instead of emptying the entire cache.
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IContent>();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IContentType>();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IMedia>();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IMediaType>();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IMember>();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IMemberType>();
        }

        public static PropertyType GetPropertyType(int id)
        {
            return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                GetCacheKey(id),
                TimeSpan.FromMinutes(30),
                delegate
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
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(GetCacheKey(Id));
        }

        private static string GetCacheKey(int id)
        {
            return CacheKeys.PropertyTypeCacheKey + id;
        }

        #endregion
    }
}