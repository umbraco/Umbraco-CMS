namespace Umbraco.Cms.Api.Management.ViewModels.RelationType;

public class RelationTypeResponseModel : RelationTypeBaseModel
{
    public Guid Id { get; set; }

    public string? Alias { get; set; }

    public string Path { get; set; } = string.Empty;

    public bool IsSystemRelationType { get; set; }

    /// <summary>
    ///     Gets or sets the Parent's object type name.
    /// </summary>
    public string? ParentObjectTypeName { get; set; }

    /// <summary>
    ///     Gets or sets the Child's object type name.
    /// </summary>
    public string? ChildObjectTypeName { get; set; }
}
