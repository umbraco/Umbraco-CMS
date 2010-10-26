using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web.UI;
using System.Linq;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.language;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;

namespace umbraco.cms.businesslogic.propertytype
{
	/// <summary>
	/// Summary description for propertytype.
	/// </summary>
	public class PropertyType
	{
		#region Declarations

		private string _alias;
		private string _name;
		private int _id;
		private int _DataTypeId;
		private int _contenttypeid;
		private int _sortOrder;
		private bool _mandatory = false;
		private string _validationRegExp = "";
		private string _description = "";
		private int _tabId = 0;
		private static string _connstring = GlobalSettings.DbDSN;

		private static object propertyTypeCacheSyncLock = new object();
		private static readonly string UmbracoPropertyTypeCacheKey = "UmbracoPropertyTypeCache";

		#endregion

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		#region Constructors

		public PropertyType(int id)
		{
			using (IRecordsReader dr = SqlHelper.ExecuteReader(
					"Select mandatory, DataTypeId, tabId, ContentTypeId, sortOrder, alias, name, validationRegExp, description from cmsPropertyType where id=@id",
					SqlHelper.CreateParameter("@id", id)))
			{
				if(!dr.Read())
					throw new ArgumentException("Propertytype with id: " + id + " doesnt exist!");
				_mandatory = dr.GetBoolean("mandatory");
				_id = id;
				if(!dr.IsNull("tabId"))
					_tabId = dr.GetInt("tabId");
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
				this.InvalidateCache();
				SqlHelper.ExecuteNonQuery(
					"Update cmsPropertyType set DataTypeId = " + value.Id + " where id=" + this.Id);
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
		public int TabId
		{
			get { return _tabId; }
			set
			{
				_tabId = value;
				this.InvalidateCache();
                object tabId = value;
                if (value < 1)
                {
                    tabId = DBNull.Value;
                }

				SqlHelper.ExecuteNonQuery( "Update cmsPropertyType set tabId = @tabId where id = @id",
					SqlHelper.CreateParameter("@tabId", tabId), 
                    SqlHelper.CreateParameter("@id", this.Id));
			}
		}

		public bool Mandatory
		{
			get { return _mandatory; }
			set
			{
				_mandatory = value;
				this.InvalidateCache();
				SqlHelper.ExecuteNonQuery(
					"Update cmsPropertyType set mandatory = @mandatory where id = @id", SqlHelper.CreateParameter("@mandatory", value),
					SqlHelper.CreateParameter("@id", this.Id));
			}
		}

		public string ValidationRegExp
		{
			get { return _validationRegExp; }
			set
			{
				_validationRegExp = value;
				this.InvalidateCache();
				SqlHelper.ExecuteNonQuery(
					"Update cmsPropertyType set validationRegExp = @validationRegExp where id = @id",
					SqlHelper.CreateParameter("@validationRegExp", value), SqlHelper.CreateParameter("@id", this.Id));
			}
		}

		public string Description
		{
			get {
                if (_description != null) {
                    if (!_description.StartsWith("#"))
                        return _description;
                    else {
                        Language lang = Language.GetByCultureCode(Thread.CurrentThread.CurrentCulture.Name);
                        if (lang != null) {
                            if (Dictionary.DictionaryItem.hasKey(_description.Substring(1, _description.Length - 1))) {
                                Dictionary.DictionaryItem di = new Dictionary.DictionaryItem(_description.Substring(1, _description.Length - 1));
                                return di.Value(lang.id);
                            }
                        }
                    }

                    return "[" + _description + "]";
                } else
                    return _description;
            }
			set
			{
				_description = value;
				this.InvalidateCache();
				SqlHelper.ExecuteNonQuery(
					"Update cmsPropertyType set description = @description where id = @id", SqlHelper.CreateParameter("@description", value),
					SqlHelper.CreateParameter("@id", this.Id));
			}
		}

		public int SortOrder
		{
			get { return _sortOrder; }
			set
			{
				_sortOrder = value;
				this.InvalidateCache();
				SqlHelper.ExecuteNonQuery(
					"Update cmsPropertyType set sortOrder = @sortOrder where id = @id", SqlHelper.CreateParameter("@sortOrder", value),
					SqlHelper.CreateParameter("@id", this.Id));
			}
		}

		public string Alias
		{
			get { return _alias; }
			set
			{
				_alias = value;
				this.InvalidateCache();
                SqlHelper.ExecuteNonQuery("Update cmsPropertyType set alias = @alias where id= @id", 
                    SqlHelper.CreateParameter("@alias", helpers.Casing.SafeAliasWithForcingCheck(_alias)),
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
				if(!_name.StartsWith("#"))
					return _name;
				else
				{
					Language lang = Language.GetByCultureCode(Thread.CurrentThread.CurrentCulture.Name);
					if(lang != null)
					{
						if(Dictionary.DictionaryItem.hasKey(_name.Substring(1, _name.Length - 1)))
						{
							Dictionary.DictionaryItem di = new Dictionary.DictionaryItem(_name.Substring(1, _name.Length - 1));
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
        public string GetRawDescription() {
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
                SqlHelper.ExecuteNonQuery("INSERT INTO cmsPropertyType (DataTypeId, ContentTypeId, alias, name) VALUES (@DataTypeId, @ContentTypeId, @alias, @name)",
                    SqlHelper.CreateParameter("@DataTypeId", dt.Id),
                    SqlHelper.CreateParameter("@ContentTypeId", ct.Id),
                    SqlHelper.CreateParameter("@alias", alias),
                    SqlHelper.CreateParameter("@name", name));
                pt = new PropertyType(SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsPropertyType WHERE alias=@alias",
                    SqlHelper.CreateParameter("@alias", alias)));
			}
			finally
			{
				// Clear cached items
                Cache.ClearCacheByKeySearch(UmbracoPropertyTypeCacheKey);               
			}

		    return pt;
		}

		public static PropertyType[] GetAll()
		{
			List<PropertyType> result = new List<PropertyType>();
			using (IRecordsReader dr =
				SqlHelper.ExecuteReader("select id, Name from cmsPropertyType order by Name"))
			{
				while(dr.Read())
				{
					PropertyType pt = GetPropertyType(dr.GetInt("id"));
					if(pt != null)
						result.Add(pt);
				}				
			}
            return result.ToArray();
		}

        /// <summary>
        /// Returns all property types based on the data type definition
        /// </summary>
        /// <param name="dataTypeDefId"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyType> GetByDataTypeDefinition(int dataTypeDefId)
        {
            List<PropertyType> result = new List<PropertyType>();
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

            // clean all properties on inherited document types (if this propertytype is removed from a master)
            DocumentType.GetAllAsList().FindAll(dt => dt.MasterContentType == _contenttypeid).ForEach(dt => cleanPropertiesOnDeletion(dt.Id));

            // Delete all properties of propertytype
            cleanPropertiesOnDeletion(_contenttypeid);

            // Delete PropertyType ..
			SqlHelper.ExecuteNonQuery("Delete from cmsPropertyType where id = " + this.Id);


            // delete 
			this.InvalidateCache();
		}

        private void cleanPropertiesOnDeletion(int contentTypeId)
        {
            var objs = Content.getContentOfContentType(new ContentType(contentTypeId));
            foreach (Content c in objs.ToList())
            {
                var prop = c.getProperty(this);
                if (prop != null)
                {
                    prop.delete();
                }
            }

            // invalidate content type cache
            ContentType.GetContentType(contentTypeId).FlushFromCache(contentTypeId);
        }

		public IDataType GetEditControl(object Value, bool IsPostBack)
		{
			IDataType dt = this.DataTypeDefinition.DataType;
			dt.DataEditor.Editor.ID = this.Alias;
			IData df = this.DataTypeDefinition.DataType.Data;
			((Control)dt.DataEditor.Editor).ID = this.Alias;

			if(!IsPostBack)
			{
				if(Value != null)
					dt.Data.Value = Value;
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
            Cache.ClearCacheItem(GetCacheKey(Id));

            // clear cache in contentype
            Cache.ClearCacheItem("ContentType_PropertyTypes_Content:" + this._contenttypeid.ToString());

            // clear cache in tab
            foreach(umbraco.cms.businesslogic.ContentType.TabI t in new ContentType(ContentTypeId).getVirtualTabs)
                ContentType.FlushTabCache(t.Id, t.ContentType);
        }

		public static PropertyType GetPropertyType(int id)
		{
			return Cache.GetCacheItem<PropertyType>(GetCacheKey(id), propertyTypeCacheSyncLock,
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
			Cache.ClearCacheItem(GetCacheKey(this.Id));
		}

		private static string GetCacheKey(int id)
		{
			return UmbracoPropertyTypeCacheKey + id;
		}

		#endregion
	}
}
