namespace Umbraco.Cms.Api.Management.ViewModels.Entity;

public class TreeEntityModelBase : EntityModelBase
{
    public bool IsContainer { get; set; }

    public string? ParentId { get; set; }

    public required string Icon { get; set; }

    public bool HasChildren { get; set; }
}
