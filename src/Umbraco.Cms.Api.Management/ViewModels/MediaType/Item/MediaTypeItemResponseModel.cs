using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;

public class MediaTypeItemResponseModel : ItemResponseModelBase
{
    public string? Icon { get; set; }

    public override string Type => Constants.UdiEntityType.MediaType;
}
