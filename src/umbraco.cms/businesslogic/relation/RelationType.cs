using System;
using System.Data;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using System.Collections.Generic;
using umbraco.cms.businesslogic.web;

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
        
		#endregion

        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		#region Constructors

        /// <summary>
        /// Internal constructor to create a new relation type
        /// </summary>
        internal RelationType() { }

		public RelationType(int id)
		{
			using (IRecordsReader dr = SqlHelper.ExecuteReader(
				"SELECT id, [dual], name, alias FROM umbracoRelationType WHERE id = @id", SqlHelper.CreateParameter("@id", id)))
			{
                if (dr.Read())
                {
                    PopulateFromReader(dr);
                }
                else
                {
                    throw new ArgumentException("Not RelationType found for id " + id.ToString());
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
					"UPDATE umbracoRelationType SET name = @name WHERE id = " + this.Id.ToString(), SqlHelper.CreateParameter("@name", value));
			}
		}

		public string Alias
		{
			get { return _alias; }
			set
			{
				_alias = value;
				SqlHelper.ExecuteNonQuery(
					"UPDATE umbracoRelationType SET alias = @alias WHERE id = " + this.Id.ToString(), SqlHelper.CreateParameter("@alias", value));
			}
		}

		public bool Dual
		{
			get { return _dual; }
			set
			{
				_dual = value;
				SqlHelper.ExecuteNonQuery(
					"UPDATE umbracoRelationType SET [dual] = @dual WHERE id = " + this.Id.ToString(), SqlHelper.CreateParameter("@dual", value));
			}
		}

		#endregion

        private void PopulateFromReader(IRecordsReader dr)
        {
            this._id = dr.GetInt("id");
            this._dual = dr.GetBoolean("dual");
            //this._parentObjectType = dr.GetGuid("parentObjectType");
            //this._childObjectType = dr.GetGuid("childObjectType");
            this._name = dr.GetString("name");
            this._alias = dr.GetString("alias");
        }

		#region Methods

		/// <summary>
		/// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
		/// </summary>
		public virtual void Save()
		{
		}

        /// <summary>
        /// Return all relation types
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<RelationType> GetAll()
        {
            var relationTypes = new List<RelationType>();

            using (IRecordsReader dr = SqlHelper.ExecuteReader("SELECT id, [dual], name, alias FROM umbracoRelationType"))
            {
                while (dr.Read())
                {
                    var rt = new RelationType();
                    rt.PopulateFromReader(dr);
                    relationTypes.Add(rt);
                }
            }

            return relationTypes;
        }

		public static RelationType GetById(int id)
		{
			try
			{
			    return new RelationType(id);
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
        
		#endregion

	}
}