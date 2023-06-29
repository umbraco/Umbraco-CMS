namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class TreeItemPresentationModel
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public bool HasChildren { get; set; }
}
