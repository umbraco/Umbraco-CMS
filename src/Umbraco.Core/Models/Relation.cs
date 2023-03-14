using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Relation between two items
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class Relation : EntityBase, IRelation
{
    private int _childId;

    private string? _comment;

    // NOTE: The datetime column from umbracoRelation is set on CreateDate on the Entity
    private int _parentId;
    private IRelationType _relationType;

    /// <summary>
    ///     Constructor for constructing the entity to be created
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="childId"></param>
    /// <param name="relationType"></param>
    public Relation(int parentId, int childId, IRelationType relationType)
    {
        _parentId = parentId;
        _childId = childId;
        _relationType = relationType;
    }

    /// <summary>
    ///     Constructor for reconstructing the entity from the data source
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="childId"></param>
    /// <param name="parentObjectType"></param>
    /// <param name="childObjectType"></param>
    /// <param name="relationType"></param>
    public Relation(int parentId, int childId, Guid parentObjectType, Guid childObjectType, IRelationType relationType)
    {
        _parentId = parentId;
        _childId = childId;
        _relationType = relationType;
        ParentObjectType = parentObjectType;
        ChildObjectType = childObjectType;
    }

    /// <summary>
    ///     Gets or sets the Parent Id of the Relation (Source)
    /// </summary>
    [DataMember]
    public int ParentId
    {
        get => _parentId;
        set => SetPropertyValueAndDetectChanges(value, ref _parentId, nameof(ParentId));
    }

    [DataMember]
    public Guid ParentObjectType { get; set; }

    /// <summary>
    ///     Gets or sets the Child Id of the Relation (Destination)
    /// </summary>
    [DataMember]
    public int ChildId
    {
        get => _childId;
        set => SetPropertyValueAndDetectChanges(value, ref _childId, nameof(ChildId));
    }

    [DataMember]
    public Guid ChildObjectType { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="RelationType" /> for the Relation
    /// </summary>
    [DataMember]
    public IRelationType RelationType
    {
        get => _relationType;
        set => SetPropertyValueAndDetectChanges(value, ref _relationType!, nameof(RelationType));
    }

    /// <summary>
    ///     Gets or sets a comment for the Relation
    /// </summary>
    [DataMember]
    public string? Comment
    {
        get => _comment;
        set => SetPropertyValueAndDetectChanges(value, ref _comment, nameof(Comment));
    }

    /// <summary>
    ///     Gets the Id of the <see cref="RelationType" /> that this Relation is based on.
    /// </summary>
    [IgnoreDataMember]
    public int RelationTypeId => _relationType.Id;
}
