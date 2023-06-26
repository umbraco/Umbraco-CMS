namespace Umbraco.Cms.Core.Models.ContentTypeEditing.PropertyTypes;

public class PropertyTypeContainerBase
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string? Name { get; set; }

    // NOTE: This needs to be a string because it can be anything in the future (= not necessarily limited to "tab" or "group")
    public required string Type { get; set; }

    public int SortOrder { get; set; }
}
