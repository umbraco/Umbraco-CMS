namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public abstract class PropertyTypeContainerModelBase
{
    public Guid Key { get; set; }

    public Guid? ParentKey { get; set; }

    public string? Name { get; set; }

    // NOTE: This needs to be a string because it can be anything in the future (= not necessarily limited to "tab" or "group")
    public string Type { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
