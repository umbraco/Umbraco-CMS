using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;

public class PartialViewItemResponseModel : FileItemResponseModelBase
{
    public override string Type => Constants.ResourceObjectTypes.PartialView;
}
