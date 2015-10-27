using System;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using umbraco.DataLayer;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic.relation
{
	/// <summary>
	/// Summary description for Relation.
	/// </summary>
	[Obsolete("Use the IRelationService instead")]
	public class Relation
	{
        private CMSNode _parentNode;
        private CMSNode _childNode;

	    internal IRelation RelationEntity;

        internal Relation(IRelation relation)
        {
            RelationEntity = relation;
        }

        public Relation(int Id)
        {
            var found = ApplicationContext.Current.Services.RelationService.GetById(Id);
            if (found == null)
            {
                throw new NullReferenceException("No relation found with id " + Id);
            }
            RelationEntity = found;
        }

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		public CMSNode Parent 
		{
		    get { return _parentNode ?? (_parentNode = new CMSNode(RelationEntity.ParentId)); }
		    set
		    {
                _parentNode = value;
		        RelationEntity.ParentId = value.Id;
		    }
		}

		public CMSNode Child 
		{
            get { return _childNode ?? (_childNode = new CMSNode(RelationEntity.ChildId)); }
            set
            {
                _childNode = value;
                RelationEntity.ChildId = value.Id;
            }
		}

		public string Comment 
		{
			get {return RelationEntity.Comment;}
			set { RelationEntity.Comment = value; }
		}

		public DateTime CreateDate 
		{
			get {return RelationEntity.CreateDate;}
		}

		public RelationType RelType
		{
			get { return new RelationType(RelationEntity.RelationType); }
			set { RelationEntity.RelationType = value.RelationTypeEntity; }
		}

		public int Id 
		{
			get {return RelationEntity.Id;}
		}

        /// <summary>
        /// Used to persist object changes to the database
        /// </summary>
        public virtual void Save()
        {
            ApplicationContext.Current.Services.RelationService.Save(RelationEntity);
        }

		public void Delete()
		{
		    ApplicationContext.Current.Services.RelationService.Delete(RelationEntity);
		}

		public static Relation MakeNew(int ParentId, int ChildId, RelationType RelType, string Comment)
        {
            var relation = new Umbraco.Core.Models.Relation(ParentId, ChildId, RelType.RelationTypeEntity)
            {
                Comment = Comment
            };
            ApplicationContext.Current.Services.RelationService.Save(relation);
            return new Relation(relation);
        }

		public static List<Relation> GetRelationsAsList(int NodeId)
		{
		    return ApplicationContext.Current.Services.RelationService.GetByParentOrChildId(NodeId)
		        .Select(x => new Relation(x))
		        .ToList();
		}

        public static bool IsRelated(int ParentID, int ChildId)
        {
            return ApplicationContext.Current.Services.RelationService.AreRelated(ParentID, ChildId);
        }

        public static bool IsRelated(int ParentID, int ChildId, RelationType Filter) 
        {
            return ApplicationContext.Current.Services.RelationService.AreRelated(ParentID, ChildId, Filter.Alias);
        }

        public static Relation[] GetRelations(int NodeId)
        {
            return GetRelationsAsList(NodeId).ToArray();
        }

        public static Relation[] GetRelations(int NodeId, RelationType Filter)
        {
            return ApplicationContext.Current.Services.RelationService.GetByParentOrChildId(NodeId, Filter.RelationTypeEntity.Alias)
                .Select(x => new Relation(x))
                .ToArray();
        }
	}
}
