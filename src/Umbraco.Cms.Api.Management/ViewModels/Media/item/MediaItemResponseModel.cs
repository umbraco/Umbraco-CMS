using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Media.Item;

public class MediaItemResponseModel : ItemResponseModelBase
{
    public string? Icon { get; set; }
    public override string Type => Constants.UdiEntityType.Media;
}
