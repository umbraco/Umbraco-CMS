namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public abstract class TreeItemPresentationModel
{
    public string Name { get; set; } = string.Empty;

    public bool HasChildren { get; set; }
}
