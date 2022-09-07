using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Controllers.Analytics;

public class GetAllAnalyticsController : AnalyticsControllerBase
{
    public PagedViewModel<TelemetryLevel> GetAll(int skip, int take)
    {
        TelemetryLevel[] levels = { TelemetryLevel.Minimal, TelemetryLevel.Basic, TelemetryLevel.Detailed };
        return new PagedViewModel<TelemetryLevel>
        {
            Total = levels.Length,
            Items = levels.Skip(skip).Take(take),
        };
    }
}
