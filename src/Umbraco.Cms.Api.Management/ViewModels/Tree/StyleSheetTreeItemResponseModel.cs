using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class StyleSheetTreeItemResponseModel : FileSystemTreeItemPresentationModel
{
    public string Type => Constants.UdiEntityType.Stylesheet;
}
