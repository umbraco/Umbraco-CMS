using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Relation between two items
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Relation : Entity, IAggregateRoot, IRelation
    {
        //NOTE: The datetime column from umbracoRelation is set on CreateDate on the Entity
        private int _parentId;
        private int _childId;
        private IRelationType _relationType;
        private string _comment;

        public Relation(int parentId, int childId, IRelationType relationType)
        {
            _parentId = parentId;
            _childId = childId;
            _relationType = relationType;
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<Relation, int>(x => x.ParentId);
            public readonly PropertyInfo ChildIdSelector = ExpressionHelper.GetPropertyInfo<Relation, int>(x => x.ChildId);
            public readonly PropertyInfo RelationTypeSelector = ExpressionHelper.GetPropertyInfo<Relation, IRelationType>(x => x.RelationType);
            public readonly PropertyInfo CommentSelector = ExpressionHelper.GetPropertyInfo<Relation, string>(x => x.Comment);
        }

        /// <summary>
        /// Gets or sets the Parent Id of the Relation (Source)
        /// </summary>
        [DataMember]
        public int ParentId
        {
            get { return _parentId; }
            set { SetPropertyValueAndDetectChanges(value, ref _parentId, Ps.Value.ParentIdSelector); }
        }

        /// <summary>
        /// Gets or sets the Child Id of the Relation (Destination)
        /// </summary>
        [DataMember]
        public int ChildId
        {
            get { return _childId; }
            set { SetPropertyValueAndDetectChanges(value, ref _childId, Ps.Value.ChildIdSelector); }
        }

        /// <summary>
        /// Gets or sets the <see cref="RelationType"/> for the Relation
        /// </summary>
        [DataMember]
        public IRelationType RelationType
        {
            get { return _relationType; }
            set { SetPropertyValueAndDetectChanges(value, ref _relationType, Ps.Value.RelationTypeSelector); }
        }

        /// <summary>
        /// Gets or sets a comment for the Relation
        /// </summary>
        [DataMember]
        public string Comment
        {
            get { return _comment; }
            set { SetPropertyValueAndDetectChanges(value, ref _comment, Ps.Value.CommentSelector); }
        }

        /// <summary>
        /// Gets the Id of the <see cref="RelationType"/> that this Relation is based on.
        /// </summary>
        [IgnoreDataMember]
        public int RelationTypeId
        {
            get { return _relationType.Id; }
        }

    }
}