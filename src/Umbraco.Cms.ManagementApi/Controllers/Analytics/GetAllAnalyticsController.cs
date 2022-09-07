using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.Analytics;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Controllers.Analytics;

[ApiVersion("1.0")]
public class GetAllAnalyticsController : AnalyticsControllerBase
{
    private readonly IViewModelFactory _viewModelFactory;

    public GetAllAnalyticsController(IViewModelFactory viewModelFactory) => _viewModelFactory = viewModelFactory;

    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(AnalyticsLevelViewModel), StatusCodes.Status200OK)]
    public PagedViewModel<TelemetryLevel> GetAll(int skip, int take)
    {
        TelemetryLevel[] levels = { TelemetryLevel.Minimal, TelemetryLevel.Basic, TelemetryLevel.Detailed };
        return _viewModelFactory.Create(levels, skip, take);
    }
}
