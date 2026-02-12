namespace Umbraco.Cms.Core.Models;

/// <summary>
///     A read only relation. Can be used to bulk save witch performs better than the normal save operation,
///     but do not populate Ids back to the model
/// </summary>
public class ReadOnlyRelation
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadOnlyRelation" /> class with all properties.
    /// </summary>
    /// <param name="id">The identifier of the relation.</param>
    /// <param name="parentId">The identifier of the parent entity.</param>
    /// <param name="childId">The identifier of the child entity.</param>
    /// <param name="relationTypeId">The identifier of the relation type.</param>
    /// <param name="createDate">The date and time when the relation was created.</param>
    /// <param name="comment">An optional comment about the relation.</param>
    public ReadOnlyRelation(int id, int parentId, int childId, int relationTypeId, DateTime createDate, string comment)
    {
        Id = id;
        ParentId = parentId;
        ChildId = childId;
        RelationTypeId = relationTypeId;
        CreateDate = createDate;
        Comment = comment;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadOnlyRelation" /> class for a new relation.
    /// </summary>
    /// <param name="parentId">The identifier of the parent entity.</param>
    /// <param name="childId">The identifier of the child entity.</param>
    /// <param name="relationTypeId">The identifier of the relation type.</param>
    public ReadOnlyRelation(int parentId, int childId, int relationTypeId)
        : this(0, parentId, childId, relationTypeId, DateTime.UtcNow, string.Empty)
    {
    }

    /// <summary>
    ///     Gets the identifier of the relation.
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     Gets the identifier of the parent entity.
    /// </summary>
    public int ParentId { get; }

    /// <summary>
    ///     Gets the identifier of the child entity.
    /// </summary>
    public int ChildId { get; }

    /// <summary>
    ///     Gets the identifier of the relation type.
    /// </summary>
    public int RelationTypeId { get; }

    /// <summary>
    ///     Gets the date and time when the relation was created.
    /// </summary>
    public DateTime CreateDate { get; }

    /// <summary>
    ///     Gets the comment associated with the relation.
    /// </summary>
    public string Comment { get; }

    /// <summary>
    ///     Gets a value indicating whether this relation has been persisted and has an identity.
    /// </summary>
    public bool HasIdentity => Id != 0;
}
