using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;

public class PartialViewItemResponseModel : FileItemResponseModelBase
{
    public override string Type => Constants.UdiEntityType.PartialView;
}
