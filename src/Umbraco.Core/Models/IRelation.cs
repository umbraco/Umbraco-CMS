using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public interface IRelation :  IRememberBeingDirty, IRelationReadOnly
    {

        /// <summary>
        /// Gets or sets the integer identifier of the entity.
        /// </summary>
        new int Id { get; set; }

        /// <summary>
        /// Gets or sets the Parent Id of the Relation (Source)
        /// </summary>
        [DataMember]
        new int ParentId { get; set; }

        [DataMember]
        new Guid ParentObjectType { get; set; }

        /// <summary>
        /// Gets or sets the Child Id of the Relation (Destination)
        /// </summary>
        [DataMember]
        new int ChildId { get; set; }

        [DataMember]
        new Guid ChildObjectType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RelationType"/> for the Relation
        /// </summary>
        [DataMember]
        new IRelationType RelationType { get; set; }

        /// <summary>
        /// Gets or sets a comment for the Relation
        /// </summary>
        [DataMember]
        new string Comment { get; set; }

        /// <summary>
        /// Gets the Id of the <see cref="RelationType"/> that this Relation is based on.
        /// </summary>
        [IgnoreDataMember]
        new int RelationTypeId { get; }
    }

    public interface IRelationReadOnly : IEntity
    {
        /// <summary>
        /// Gets the integer identifier of the entity.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets  the Parent Id of the Relation (Source)
        /// </summary>
        int ParentId { get; }

        Guid ParentObjectType { get; }

        /// <summary>
        /// Gets or sets the Child Id of the Relation (Destination)
        /// </summary>
        int ChildId { get; }

        Guid ChildObjectType { get; }

        /// <summary>
        /// Gets the <see cref="RelationType"/> for the Relation
        /// </summary>
        IRelationType RelationType { get; }

        /// <summary>
        /// Gets a comment for the Relation
        /// </summary>
        string Comment { get; }

        /// <summary>
        /// Gets the Id of the <see cref="RelationType"/> that this Relation is based on.
        /// </summary>
        [IgnoreDataMember]
        int RelationTypeId { get; }
    }
}
