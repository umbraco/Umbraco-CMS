using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class PropertyTypeContainerModelBase
{
    public Guid Id { get; set; }

    public ReferenceByIdModel? Parent { get; set; }

    public string? Name { get; set; }

    // NOTE: This needs to be a string because it can be anything in the future (= not necessarily limited to "tab" or "group")
    [Required]
    public string Type { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
