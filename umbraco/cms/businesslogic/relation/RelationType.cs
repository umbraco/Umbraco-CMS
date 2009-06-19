using System;
using System.Data;
using System.Web;

using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace umbraco.cms.businesslogic.relation
{
	/// <summary>
	/// Summary description for RelationType.
	/// </summary>
	public class RelationType
	{
		#region Declarations

		private int _id;
		private bool _dual;
		private string _name;
		//private Guid _parentObjectType;
		//private Guid _childObjectType;
		private string _alias;

		private static object relationTypeSyncLock = new object();

		#endregion

        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		#region Constructors

		public RelationType(int Id)
		{
			using (IRecordsReader dr = SqlHelper.ExecuteReader(
				"select id, dual, name, alias from umbracoRelationType where id = @id", SqlHelper.CreateParameter("@id", Id)))
			{
				if(dr.Read())
				{
					this._id = dr.GetInt("id");
					this._dual = dr.GetBoolean("dual");
					//this._parentObjectType = dr.GetGuid("parentObjectType");
					//this._childObjectType = dr.GetGuid("childObjectType");
					this._name = dr.GetString("name");
					this._alias = dr.GetString("alias");
				}
			}
		}

		#endregion

		#region Properties

		public int Id
		{
			get { return _id; }
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				SqlHelper.ExecuteNonQuery(
					"update umbracoRelationType set name = @name where id = " + this.Id.ToString(), SqlHelper.CreateParameter("@name", value));
				this.InvalidateCache();
			}
		}

		public string Alias
		{
			get { return _alias; }
			set
			{
				_alias = value;
				SqlHelper.ExecuteNonQuery(
					"update umbracoRelationType set alias = @alias where id = " + this.Id.ToString(), SqlHelper.CreateParameter("@alias", value));
				this.InvalidateCache();
			}
		}

		public bool Dual
		{
			get { return _dual; }
			set
			{
				_dual = value;
				SqlHelper.ExecuteNonQuery(
					"update umbracoRelationType set [dual] = @dual where id = " + this.Id.ToString(), SqlHelper.CreateParameter("@dual", value));
				this.InvalidateCache();
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
		/// </summary>
		public virtual void Save()
		{
		}

		public static RelationType GetById(int id)
		{
			try
			{
				return umbraco.cms.businesslogic.cache.Cache.GetCacheItem<RelationType>(
					GetCacheKey(id), relationTypeSyncLock, TimeSpan.FromMinutes(30),
					delegate { return new RelationType(id); });
			}
			catch
			{
				return null;
			}
		}

		public static RelationType GetByAlias(string Alias)
		{
			try
			{
				return GetById(SqlHelper.ExecuteScalar<int>(
					"select id from umbracoRelationType where alias = @alias",
					SqlHelper.CreateParameter("@alias", Alias)));
			}
			catch
			{
				return null;
			}
		}

		protected virtual void InvalidateCache()
		{
			umbraco.cms.businesslogic.cache.Cache.ClearCacheItem(GetCacheKey(this.Id));
		}

		private static string GetCacheKey(int id)
		{
			return string.Format("RelationTypeCacheItem_{0}", id);
		}

		#endregion
	}
}