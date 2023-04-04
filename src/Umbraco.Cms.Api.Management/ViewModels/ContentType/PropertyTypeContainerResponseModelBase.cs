namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class PropertyTypeContainerResponseModelBase
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string? Name { get; set; }

    // NOTE: This needs to be a string because it can be anything in the future (= not necessarily limited to "tab" or "group")
    public string Type { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
