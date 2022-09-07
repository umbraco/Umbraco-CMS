﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.Analytics;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Controllers.Analytics;

[ApiVersion("1.0")]
public class GetAllAnalyticsController : AnalyticsControllerBase
{
    private readonly IPagedViewModelFactory _pagedViewModelFactory;

    public GetAllAnalyticsController(IPagedViewModelFactory pagedViewModelFactory) => _pagedViewModelFactory = pagedViewModelFactory;

    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(AnalyticsLevelViewModel), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<TelemetryLevel>> GetAll(int skip, int take)
    {
        TelemetryLevel[] levels = { TelemetryLevel.Minimal, TelemetryLevel.Basic, TelemetryLevel.Detailed };
        return _pagedViewModelFactory.Create(levels, skip, take);
    }
}
