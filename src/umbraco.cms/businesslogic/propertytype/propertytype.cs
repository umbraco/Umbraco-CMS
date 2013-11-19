using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Persistence.Caching;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.cms.helpers;
using umbraco.DataLayer;
using umbraco.interfaces;
using Umbraco.Core.Persistence;
using Umbraco.Core.Models.Rdbms;

namespace umbraco.cms.businesslogic.propertytype
{
    /// <summary>
    /// Summary description for propertytype.
    /// </summary>
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

        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        internal static UmbracoDatabase Database
        {
            get { return ApplicationContext.Current.DatabaseContext.Database; }
        }

        #region Constructors

        public PropertyType(int id)
        {
            var propertyTypeDto = Database.FirstOrDefault<PropertyTypeDto>(
                 "Select mandatory, DataTypeId, propertyTypeGroupId, ContentTypeId, sortOrder, alias, name, validationRegExp, description from cmsPropertyType where id=@0", id);
            if (propertyTypeDto == null)
                throw new ArgumentException("Propertytype with id: " + id + " doesnt exist!");
            else
            {
                _mandatory = propertyTypeDto.Mandatory;
                _id = id;
                _sortOrder = propertyTypeDto.SortOrder;
                _alias = propertyTypeDto.Alias;
                _name = propertyTypeDto.Name;
                _validationRegExp = propertyTypeDto.ValidationRegExp;
                _DataTypeId = propertyTypeDto.DataTypeId;
                _contenttypeid = propertyTypeDto.ContentTypeId;
                _description = propertyTypeDto.Description;

                if (propertyTypeDto.PropertyTypeGroupId != null)
                {
                    _propertyTypeGroup = (int)propertyTypeDto.PropertyTypeGroupId;
                    //TODO: Remove after refactoring!
                    _tabId = _propertyTypeGroup;
                }
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
                Database.Execute(
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
                Database.Execute("Update cmsPropertyType set propertyTypeGroupId = @0 where id = @1", dbPropertyTypeGroup, Id);
            }
        }

        public bool Mandatory
        {
            get { return _mandatory; }
            set
            {
                _mandatory = value;
                InvalidateCache();
                Database.Execute(
                    "Update cmsPropertyType set mandatory = @0 where id = @1", value, Id);
            }
        }

        public string ValidationRegExp
        {
            get { return _validationRegExp; }
            set
            {
                _validationRegExp = value;
                InvalidateCache();
                Database.Execute(
                    "Update cmsPropertyType set validationRegExp = @0 where id = @1", value, Id);
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
                Database.Execute(
                    "Update cmsPropertyType set description = @0 where id = @1", value, Id);
            }
        }

        public int SortOrder
        {
            get { return _sortOrder; }
            set
            {
                _sortOrder = value;
                InvalidateCache();
                Database.Execute(
                    "Update cmsPropertyType set sortOrder = @0 where id = @1", value, Id);
            }
        }

        public string Alias
        {
            get { return _alias; }
            set
            {
                _alias = value;
                InvalidateCache();
                Database.Execute("Update cmsPropertyType set alias = @0 where id= @1", Casing.SafeAliasWithForcingCheck(_alias), Id);
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
                Database.Execute(
                    "UPDATE cmsPropertyType SET name=@0 WHERE id=@1", _name, Id);
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
                Database.Execute(
                    "INSERT INTO cmsPropertyType (DataTypeId, ContentTypeId, alias, name) VALUES (@0, @1, @2, @3)", dt.Id, ct.Id, alias, name);
                pt =
                    new PropertyType(
                        Database.ExecuteScalar<int>("SELECT MAX(id) FROM cmsPropertyType WHERE alias=@0", alias));
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
            return getobjects(Database.Query<int>("select id from cmsPropertyType order by Name"));
        }
        private static IEnumerable<PropertyType> getobjects(IEnumerable<int> ids)
        {
            foreach (var id in ids) yield return GetPropertyType(id);
        }

        public static IEnumerable<PropertyType> GetPropertyTypesByGroup(int groupId, List<int> contentTypeIds)
        {
            return GetPropertyTypesByGroup(groupId).Where(x => contentTypeIds.Contains(x.ContentTypeId));
        }

        public static IEnumerable<PropertyType> GetPropertyTypesByGroup(int groupId)
        {
            return getobjects(Database.Query<int>("SELECT id FROM cmsPropertyType WHERE propertyTypeGroupId = @0 order by SortOrder", groupId));
        }

        /// <summary>
        /// Returns all property types based on the data type definition
        /// </summary>
        /// <param name="dataTypeDefId"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyType> GetByDataTypeDefinition(int dataTypeDefId)
        {
            return getobjects(Database.Query<int>("select id, Name from cmsPropertyType where dataTypeId=@0 order by Name", dataTypeDefId));
        }

        public void delete()
        {
            // flush cache
            FlushCache();

            // Delete all properties of propertytype
            //ss:temp commented and used the following code line  CleanPropertiesOnDeletion(_contenttypeid);
            CleanAllPropertiesOnDeletion();

            // Delete PropertyType ..
            Database.Execute("Delete from cmsPropertyType where id = @0", Id);


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

        // (!new) temp
        private void CleanAllPropertiesOnDeletion()
        {
            Database.Execute("DELETE FROM cmsPropertyData WHERE PropertyTypeId = @0", this.Id);
        }

        //SS: 11-NOV-2013 - see cmd_businesslogic_Property_Tests. - 
        // for refs see https://groups.google.com/d/msg/umbraco-dev/9qLYrQrTQ8o/Uljx446Bv1YJ
        // temp CleanAllPropertiesOnDeletion() used instead
        private void CleanPropertiesOnDeletion(int contentTypeId)
        {
            // first delete from all master document types
            //TODO: Verify no endless loops with mixins
            DocumentType.GetAllAsList().FindAll(dt => dt.MasterContentTypes.Contains(contentTypeId)).ForEach(
                dt => CleanPropertiesOnDeletion(dt.Id));

            //Initially Content.getContentOfContentType() was called, but because this doesn't include members we resort to sql lookups and deletes
            foreach (var contentId in
                Database.Query<int>("SELECT nodeId FROM cmsContent INNER JOIN umbracoNode ON cmsContent.nodeId = umbracoNode.id WHERE ContentType = @0 ORDER BY umbracoNode.text ", contentTypeId))
            {
                System.Console.WriteLine("**** DELETED {0} {1} ****", this.Id, contentId);

                Database.Execute("DELETE FROM cmsPropertyData WHERE PropertyTypeId = @0 AND contentNodeId = @1", this.Id, contentId);
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
            InMemoryCacheProvider.Current.Clear();
            RuntimeCacheProvider.Current.Clear();
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