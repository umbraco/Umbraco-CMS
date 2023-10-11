using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class StaticFileTreeItemResponseModel : FileSystemTreeItemPresentationModel
{
    public string Type => Constants.UdiEntityType.StaticFile;
}
