using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using umbraco.DataLayer;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic.relation
{
	/// <summary>
	/// Summary description for Relation.
	/// </summary>
	[Obsolete("Use the RelationService instead")]
	public class Relation
	{
		private int _id;
		private CMSNode _parentNode;
		private CMSNode _childNode;
		private string _comment;
		private DateTime _datetime;
		private RelationType _relType;

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		public CMSNode Parent 
		{
			get {return _parentNode;}
			set 
			{
				SqlHelper.ExecuteNonQuery("update umbracoRelation set parentId = @parent where id = " + this.Id, SqlHelper.CreateParameter("@parent", value.Id));
				_parentNode = value;
			}
		}

		public CMSNode Child 
		{
			get {return _childNode;}
			set 
			{
				SqlHelper.ExecuteNonQuery("update umbracoRelation set childId = @child where id = " + this.Id, SqlHelper.CreateParameter("@child", value.Id));
				_childNode = value;
			}
		}

		public string Comment 
		{
			get {return _comment;}
			set 
			{
				SqlHelper.ExecuteNonQuery("update umbracoRelation set comment = @comment where id = " + this.Id, SqlHelper.CreateParameter("@comment", value));
				_comment = value;
			}
		}

		public DateTime CreateDate 
		{
			get {return _datetime;}
		}

		public RelationType RelType
		{
			get {return _relType;}
			set 
			{
				SqlHelper.ExecuteNonQuery("update umbracoRelation set relType = @relType where id = " + this.Id, SqlHelper.CreateParameter("@relType", value.Id));
				_relType = value;
			}
		}

		public int Id 
		{
			get {return _id;}
		}

		public Relation(int Id)
		{
			using (IRecordsReader dr = SqlHelper.ExecuteReader("select * from umbracoRelation where id = @id", SqlHelper.CreateParameter("@id", Id)))
			{
                if (dr.Read())
                {
                    this._id = dr.GetInt("id");
                    this._parentNode = new CMSNode(dr.GetInt("parentId"));
                    this._childNode = new CMSNode(dr.GetInt("childId"));
                    this._relType = RelationType.GetById(dr.GetInt("relType"));
                    this._comment = dr.GetString("comment");
                    this._datetime = dr.GetDateTime("datetime");
                }
                else
                {
                    throw new ArgumentException("No relation found for id " + Id.ToString());
                }
			}
		}

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public virtual void Save()
        {
        }


		public void Delete() 
		{
			SqlHelper.ExecuteNonQuery("DELETE FROM umbracoRelation WHERE id = @id", SqlHelper.CreateParameter("@id", this.Id));
		}

        [MethodImpl(MethodImplOptions.Synchronized)]
		public static Relation MakeNew(int ParentId, int ChildId, RelationType RelType, string Comment) 
		{
            // The method is synchronized
            SqlHelper.ExecuteNonQuery("INSERT INTO umbracoRelation (childId, parentId, relType, comment) VALUES (@childId, @parentId, @relType, @comment)",
                SqlHelper.CreateParameter("@childId", ChildId),
                SqlHelper.CreateParameter("@parentId", ParentId),
                SqlHelper.CreateParameter("@relType", RelType.Id),
                SqlHelper.CreateParameter("@comment", Comment));
            return new Relation(SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM umbracoRelation"));
		}



		public static List<Relation> GetRelationsAsList(int NodeId) 
		{
            List<Relation> _rels = new List<Relation>();
			System.Collections.ArrayList tmp = new System.Collections.ArrayList();
			using (IRecordsReader dr = SqlHelper.ExecuteReader("select umbracoRelation.id from umbracoRelation inner join umbracoRelationType on umbracoRelationType.id = umbracoRelation.relType where umbracoRelation.parentId = @id or (umbracoRelation.childId = @id and umbracoRelationType.[dual] = 1)", SqlHelper.CreateParameter("@id", NodeId)))
			{
				while(dr.Read())
				{
                    _rels.Add(new Relation(dr.GetInt("id")));
				}
			}

			return _rels;
		}

        public static bool IsRelated(int ParentID, int ChildId) {
            int count = SqlHelper.ExecuteScalar<int>("SELECT count(*) FROM umbracoRelation WHERE childId = @childId AND parentId = @parentId",
                  SqlHelper.CreateParameter("@childId", ChildId),
                  SqlHelper.CreateParameter("@parentId", ParentID));

            return (count > 0);
        }

        public static bool IsRelated(int ParentID, int ChildId, RelationType Filter) {
            
            int count = SqlHelper.ExecuteScalar<int>("SELECT count(*) FROM umbracoRelation WHERE childId = @childId AND parentId = @parentId AND relType = @relType",
                 SqlHelper.CreateParameter("@childId", ChildId),
                 SqlHelper.CreateParameter("@parentId", ParentID),
                 SqlHelper.CreateParameter("@relType", Filter.Id) 
            );

            return (count > 0);
        }

        public static Relation[] GetRelations(int NodeId)
        {
            return GetRelationsAsList(NodeId).ToArray();
        }

        public static Relation[] GetRelations(int NodeId, RelationType Filter)
        {
            System.Collections.ArrayList tmp = new System.Collections.ArrayList();
			using (IRecordsReader dr = SqlHelper.ExecuteReader("select umbracoRelation.id from umbracoRelation inner join umbracoRelationType on umbracoRelationType.id = umbracoRelation.relType and umbracoRelationType.id = @relTypeId where umbracoRelation.parentId = @id or (umbracoRelation.childId = @id and umbracoRelationType.[dual] = 1)", SqlHelper.CreateParameter("@id", NodeId), SqlHelper.CreateParameter("@relTypeId", Filter.Id)))
			{
				while(dr.Read())
				{
					tmp.Add(dr.GetInt("id"));
				}
			}

            Relation[] retval = new Relation[tmp.Count];

            for (int i = 0; i < tmp.Count; i++) retval[i] = new Relation((int)tmp[i]);
            return retval;
        }
	}
}
