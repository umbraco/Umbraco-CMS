using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Relation between two items
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Relation : Entity, IAggregateRoot
    {
        //NOTE: The datetime column from umbracoRelation is set on CreateDate on the Entity
        private int _parentId;
        private int _childId;
        private RelationType _relationType;
        private string _comment;

        public Relation(int parentId, int childId)
        {
            _parentId = parentId;
            _childId = childId;
        }

        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<Relation, int>(x => x.ParentId);
        private static readonly PropertyInfo ChildIdSelector = ExpressionHelper.GetPropertyInfo<Relation, int>(x => x.ChildId);
        private static readonly PropertyInfo RelationTypeSelector = ExpressionHelper.GetPropertyInfo<Relation, RelationType>(x => x.RelationType);
        private static readonly PropertyInfo CommentSelector = ExpressionHelper.GetPropertyInfo<Relation, string>(x => x.Comment);

        /// <summary>
        /// Gets or sets the Parent Id of the Relation (Source)
        /// </summary>
        [DataMember]
        public int ParentId
        {
            get { return _parentId; }
            set
            {
                _parentId = value;
                OnPropertyChanged(ParentIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Child Id of the Relation (Destination)
        /// </summary>
        [DataMember]
        public int ChildId
        {
            get { return _childId; }
            set
            {
                _childId = value;
                OnPropertyChanged(ChildIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="RelationType"/> for the Relation
        /// </summary>
        [DataMember]
        public RelationType RelationType
        {
            get { return _relationType; }
            set
            {
                _relationType = value;
                OnPropertyChanged(RelationTypeSelector);
            }
        }

        /// <summary>
        /// Gets or sets a comment for the Relation
        /// </summary>
        [DataMember]
        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                OnPropertyChanged(CommentSelector);
            }
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