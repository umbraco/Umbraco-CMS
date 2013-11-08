using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using umbraco.DataLayer;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;

namespace umbraco.cms.businesslogic.relation
{
	/// <summary>
	/// Summary description for Relation.
	/// </summary>
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
                ApplicationContext.Current.DatabaseContext.Database.Update<RelationDto>("set parentId = @0 where id = @1", value.Id, this.Id);
				_parentNode = value;
			}
		}

		public CMSNode Child 
		{
			get {return _childNode;}
			set 
			{
                ApplicationContext.Current.DatabaseContext.Database.Update<RelationDto>("set childId = @0 where id = @1", value.Id, this.Id);
				_childNode = value;
			}
		}

		public string Comment 
		{
			get {return _comment;}
			set 
			{
                ApplicationContext.Current.DatabaseContext.Database.Update<RelationDto>("set comment = @0 where id = @1", value, this.Id);
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
                ApplicationContext.Current.DatabaseContext.Database.Update<RelationDto>("set relType = @0 where id = @1", value.Id, this.Id);
				_relType = value;
			}
		}

		public int Id 
		{
			get {return _id;}
		}

		public Relation(int Id)
		{
            var relationDto = ApplicationContext.Current.DatabaseContext.Database.SingleOrDefault<RelationDto>("select * from umbracoRelation where id = @0", Id);
            if (relationDto != null) this.PopulateFromDto(relationDto);  
            else throw new ArgumentException("No relation found for id " + Id.ToString());
		}

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public virtual void Save()
        {
        }


		public void Delete() 
		{
            ApplicationContext.Current.DatabaseContext.Database.Delete<RelationDto>("DELETE FROM umbracoRelation WHERE id = @0", this.Id);   
		}

        [MethodImpl(MethodImplOptions.Synchronized)]
		public static Relation MakeNew(int parentId, int childId, RelationType relType, string comment) 
		{
            // The method is synchronized
            var seedRelationDto = new RelationDto()
            {
                 ParentId = parentId,
                 ChildId = childId,
                 RelationType = relType.Id,
                 Comment = comment
            };
            ApplicationContext.Current.DatabaseContext.Database.Insert(seedRelationDto);
            return new Relation(ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>("SELECT MAX(id) FROM umbracoRelation"));
        }



		public static List<Relation> GetRelationsAsList(int NodeId) 
		{
            List<Relation> _rels = new List<Relation>();
            foreach (var relationId in ApplicationContext.Current.DatabaseContext.Database.Query<int>(
                        "select umbracoRelation.id from umbracoRelation inner join umbracoRelationType on umbracoRelationType.id = umbracoRelation.relType " +
                        " where umbracoRelation.parentId = @0 or (umbracoRelation.childId = @0 and umbracoRelationType.[dual] = 1)", NodeId))
                     _rels.Add(new Relation(relationId));
			return _rels;
		}

        public static bool IsRelated(int ParentID, int ChildId) {
            int count = ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>(
                     "SELECT count(*) FROM umbracoRelation WHERE childId = @0 AND parentId = @1", ChildId, ParentID);
            return (count > 0);
        }

        public static bool IsRelated(int ParentID, int ChildId, RelationType Filter) {
            int count = ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>(
                     "SELECT count(*) FROM umbracoRelation WHERE childId = @0 AND parentId = @1 AND relType = @2", ChildId, ParentID, Filter.Id);
            return (count > 0);
        }

        public static Relation[] GetRelations(int NodeId)
        {
            return GetRelationsAsList(NodeId).ToArray();
        }

        public static Relation[] GetRelations(int NodeId, RelationType Filter)
        {
            System.Collections.ArrayList tmp = new System.Collections.ArrayList();
            foreach (var relationId in ApplicationContext.Current.DatabaseContext.Database.Query<int>(
                      "select umbracoRelation.id from umbracoRelation inner join umbracoRelationType on umbracoRelationType.id = umbracoRelation.relType and umbracoRelationType.id = @0 " +
                      " where umbracoRelation.parentId = @1 or (umbracoRelation.childId = @1 and umbracoRelationType.[dual] = 1)", Filter.Id, NodeId))
            {
                tmp.Add(relationId);
            }
   
            Relation[] retval = new Relation[tmp.Count];

            for (int i = 0; i < tmp.Count; i++) retval[i] = new Relation((int)tmp[i]);
            return retval;
        }

        internal Relation() { }

        internal void PopulateFromDto(RelationDto relationDto)
        {
            this._id = relationDto.Id;
            this._parentNode = new CMSNode(relationDto.ParentId);
            this._childNode = new CMSNode(relationDto.ChildId);
            this._relType = RelationType.GetById(relationDto.RelationType);
            this._comment = relationDto.Comment;
            this._datetime = relationDto.Datetime;
        }
	}
}
