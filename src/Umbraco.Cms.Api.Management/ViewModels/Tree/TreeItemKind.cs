namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Specifies the types of tree items to include in search results.
/// </summary>
[Flags]
public enum TreeItemKind
{
    Item = 1,
    Folder = 2,
    All = Item | Folder
}
