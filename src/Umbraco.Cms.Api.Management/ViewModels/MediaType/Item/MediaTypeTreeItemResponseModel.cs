using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;

public class MediaTypeTreeItemResponseModel : FolderTreeItemResponseModel
{
    public string Icon { get; set; } = string.Empty;
}
