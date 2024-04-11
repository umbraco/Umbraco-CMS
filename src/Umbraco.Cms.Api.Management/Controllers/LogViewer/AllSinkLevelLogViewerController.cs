using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

[ApiVersion("1.0")]
public class AllSinkLevelLogViewerController : LogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllSinkLevelLogViewerController(ILogViewerService logViewerService, IUmbracoMapper umbracoMapper)
    {
        _logViewerService = logViewerService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paginated list of all loggers' levels.
    /// </summary>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>The paged result of the configured loggers and their level.</returns>
    [HttpGet("level")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<LoggerResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<LoggerResponseModel>>> AllLogLevels(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        KeyValuePair<string, LogLevel>[] logLevels = _logViewerService
            .GetLogLevelsFromSinks()
            .ToArray();

        var viewModel = new PagedViewModel<LoggerResponseModel>
        {
            Total = logLevels.Length,
            Items = _umbracoMapper.MapEnumerable<KeyValuePair<string, LogLevel>, LoggerResponseModel>(logLevels.Skip(skip).Take(take))
        };

        return await Task.FromResult(Ok(viewModel));
    }
}
