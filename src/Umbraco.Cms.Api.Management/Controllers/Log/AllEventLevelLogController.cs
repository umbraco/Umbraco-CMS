using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog.Events;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Controllers.LogViewer;
using Umbraco.Cms.Api.Management.ViewModels.Log;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.Log;

public class AllEventLevelLogController : LogControllerBase
{
    private readonly ILogLevelLoader _logLevelLoader;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllEventLevelLogController(ILogLevelLoader logLevelLoader, ILogViewer logViewer, IUmbracoMapper umbracoMapper)
        : base(logViewer)
    {
        _logLevelLoader = logLevelLoader;
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
    [ProducesResponseType(typeof(PagedViewModel<LoggerViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<LoggerViewModel>>> AllLogLevels(int skip = 0, int take = 100)
    {
        IEnumerable<KeyValuePair<string, LogEventLevel?>> logLevels = _logLevelLoader
            .GetLogLevelsFromSinks()
            .Skip(skip)
            .Take(take);

        return await Task.FromResult(Ok(_umbracoMapper.Map<PagedViewModel<LoggerViewModel>>(logLevels)));
    }
}
