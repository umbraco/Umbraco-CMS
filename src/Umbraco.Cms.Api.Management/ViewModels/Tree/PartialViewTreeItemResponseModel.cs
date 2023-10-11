using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class PartialViewTreeItemResponseModel : FileSystemTreeItemPresentationModel
{
    public string Type => Constants.UdiEntityType.PartialView;
}
