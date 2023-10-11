using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Media.Item;

public class MediaTreeItemResponseModel : ContentTreeItemResponseModel
{
    public string Icon { get; set; } = string.Empty;
    public override string Type => Constants.UdiEntityType.Media;
}
