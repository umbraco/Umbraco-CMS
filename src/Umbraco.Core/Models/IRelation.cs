using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface IRelation : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets or sets the Parent Id of the Relation (Source)
    /// </summary>
    [DataMember]
    int ParentId { get; set; }

    [DataMember]
    Guid ParentObjectType { get; set; }

    /// <summary>
    ///     Gets or sets the Child Id of the Relation (Destination)
    /// </summary>
    [DataMember]
    int ChildId { get; set; }

    [DataMember]
    Guid ChildObjectType { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="RelationType" /> for the Relation
    /// </summary>
    [DataMember]
    IRelationType RelationType { get; set; }

    /// <summary>
    ///     Gets or sets a comment for the Relation
    /// </summary>
    [DataMember]
    string? Comment { get; set; }

    /// <summary>
    ///     Gets the Id of the <see cref="RelationType" /> that this Relation is based on.
    /// </summary>
    [IgnoreDataMember]
    int RelationTypeId { get; }
}
