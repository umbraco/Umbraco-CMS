using Umbraco.Cms.Api.Management.ViewModels.Abstract;
using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Media.Item;

public class MediaItemResponseModel : ItemResponseModelBase, ITracksTrashing
{
    public string? Icon { get; set; }
    public bool IsTrashed { get; set; }
}
