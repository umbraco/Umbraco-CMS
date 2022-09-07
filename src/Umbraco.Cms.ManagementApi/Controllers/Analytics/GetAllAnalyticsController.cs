using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Controllers.Analytics;

public class GetAllAnalyticsController : AnalyticsControllerBase
{
    private readonly IViewModelFactory _viewModelFactory;

    public GetAllAnalyticsController(IViewModelFactory viewModelFactory) => _viewModelFactory = viewModelFactory;

    public PagedViewModel<TelemetryLevel> GetAll(int skip, int take)
    {
        TelemetryLevel[] levels = { TelemetryLevel.Minimal, TelemetryLevel.Basic, TelemetryLevel.Detailed };
        return _viewModelFactory.Create(levels, skip, take);
    }
}
