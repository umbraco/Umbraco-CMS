using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Tour;

public class UserTourStatusesResponseModel
{
    public required IEnumerable<TourStatusViewModel> TourStatuses { get; set; }
    public string Type => Constants.UdiEntityType.UserTourStatuses;
}
