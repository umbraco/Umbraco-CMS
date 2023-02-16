namespace Umbraco.Cms.Api.Management.ViewModels.RelationType;

public class RelationTypeViewModel
{
    public Guid Key { get; set; }

    public string? Name { get; set; }

    public string? Alias { get; set; }

    public string Path { get; set; } = string.Empty;

    public bool IsSystemRelationType { get; set; }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the RelationType is Bidirectional (true) or Parent to Child (false)
    /// </summary>
    public bool IsBidirectional { get; set; }

    /// <summary>
    ///     Gets or sets the Parents object type id
    /// </summary>
    /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
    public Guid? ParentObjectType { get; set; }

    /// <summary>
    ///     Gets or sets the Parent's object type name.
    /// </summary>
    public string? ParentObjectTypeName { get; set; }

    /// <summary>
    ///     Gets or sets the Child's object type id
    /// </summary>
    /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
    public Guid? ChildObjectType { get; set; }

    /// <summary>
    ///     Gets or sets the Child's object type name.
    /// </summary>
    public string? ChildObjectTypeName { get; set; }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the RelationType should be returned in "Used by"-queries.
    /// </summary>
    public bool IsDependency { get; set; }
}
