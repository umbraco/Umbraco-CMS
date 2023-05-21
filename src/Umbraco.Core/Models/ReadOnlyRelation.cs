namespace Umbraco.Cms.Core.Models;

/// <summary>
///     A read only relation. Can be used to bulk save witch performs better than the normal save operation,
///     but do not populate Ids back to the model
/// </summary>
public class ReadOnlyRelation
{
    public ReadOnlyRelation(int id, int parentId, int childId, int relationTypeId, DateTime createDate, string comment)
    {
        Id = id;
        ParentId = parentId;
        ChildId = childId;
        RelationTypeId = relationTypeId;
        CreateDate = createDate;
        Comment = comment;
    }

    public ReadOnlyRelation(int parentId, int childId, int relationTypeId)
        : this(0, parentId, childId, relationTypeId, DateTime.Now, string.Empty)
    {
    }

    public int Id { get; }

    public int ParentId { get; }

    public int ChildId { get; }

    public int RelationTypeId { get; }

    public DateTime CreateDate { get; }

    public string Comment { get; }

    public bool HasIdentity => Id != 0;
}
