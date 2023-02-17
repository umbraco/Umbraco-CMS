namespace Umbraco.Cms.Api.Management.ViewModels.RelationType;

public class RelationTypeSavingViewModel
{
    /// <summary>
    ///     Gets or sets the name of the model.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the key of the model.
    /// </summary>
    public Guid? Key { get; set; }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the RelationType is Bidirectional (true) or Parent to Child (false)
    /// </summary>
    public bool IsBidirectional { get; set; }

    /// <summary>
    ///     Gets or sets the parent object type ID.
    /// </summary>
    public Guid? ParentObjectType { get; set; }

    /// <summary>
    ///     Gets or sets the child object type ID.
    /// </summary>
    public Guid? ChildObjectType { get; set; }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the RelationType should be returned in "Used by"-queries.
    /// </summary>
    public bool IsDependency { get; set; }
}
