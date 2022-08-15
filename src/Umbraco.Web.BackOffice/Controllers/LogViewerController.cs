using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     Backoffice controller supporting the dashboard for viewing logs with some simple graphs & filtering
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class LogViewerController : BackOfficeNotificationsController
{
    private readonly ILogLevelLoader _logLevelLoader;
    private readonly ILogViewer _logViewer;

    [Obsolete]
    public LogViewerController(ILogViewer logViewer)
        : this(logViewer, StaticServiceProvider.Instance.GetRequiredService<ILogLevelLoader>())
    {
    }

    [ActivatorUtilitiesConstructor]
    public LogViewerController(ILogViewer logViewer, ILogLevelLoader logLevelLoader)
    {
        _logViewer = logViewer ?? throw new ArgumentNullException(nameof(logViewer));
        _logLevelLoader = logLevelLoader ?? throw new ArgumentNullException(nameof(logLevelLoader));
    }

    private bool CanViewLogs(LogTimePeriod logTimePeriod)
    {
        //Can the interface deal with Large Files
        if (_logViewer.CanHandleLargeLogs)
        {
            return true;
        }

        //Interface CheckCanOpenLogs
        return _logViewer.CheckCanOpenLogs(logTimePeriod);
    }

    [HttpGet]
    public bool GetCanViewLogs([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);
        return CanViewLogs(logTimePeriod);
    }

    [HttpGet]
    public ActionResult<int> GetNumberOfErrors([FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);
        //We will need to stop the request if trying to do this on a 1GB file
        if (CanViewLogs(logTimePeriod) == false)
        {
            return ValidationProblem("Unable to view logs, due to size");
        }

        return _logViewer.GetNumberOfErrors(logTimePeriod);
    }

    [HttpGet]
    public ActionResult<LogLevelCounts> GetLogLevelCounts([FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);
        //We will need to stop the request if trying to do this on a 1GB file
        if (CanViewLogs(logTimePeriod) == false)
        {
            return ValidationProblem("Unable to view logs, due to size");
        }

        return _logViewer.GetLogLevelCounts(logTimePeriod);
    }

    [HttpGet]
    public ActionResult<IEnumerable<LogTemplate>> GetMessageTemplates([FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);
        //We will need to stop the request if trying to do this on a 1GB file
        if (CanViewLogs(logTimePeriod) == false)
        {
            return ValidationProblem("Unable to view logs, due to size");
        }

        return new ActionResult<IEnumerable<LogTemplate>>(_logViewer.GetMessageTemplates(logTimePeriod));
    }

    [HttpGet]
    public ActionResult<PagedResult<LogMessage>> GetLogs(string orderDirection = "Descending", int pageNumber = 1,
        string? filterExpression = null, [FromQuery(Name = "logLevels[]")] string[]? logLevels = null,
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        LogTimePeriod logTimePeriod = GetTimePeriod(startDate, endDate);

        //We will need to stop the request if trying to do this on a 1GB file
        if (CanViewLogs(logTimePeriod) == false)
        {
            return ValidationProblem("Unable to view logs, due to size");
        }

        Direction direction = orderDirection == "Descending" ? Direction.Descending : Direction.Ascending;

        return _logViewer.GetLogs(logTimePeriod, filterExpression: filterExpression, pageNumber: pageNumber,
            orderDirection: direction, logLevels: logLevels);
    }

    private static LogTimePeriod GetTimePeriod(DateTime? startDate, DateTime? endDate)
    {
        if (startDate == null || endDate == null)
        {
            DateTime now = DateTime.Now;
            if (startDate == null)
            {
                startDate = now.AddDays(-1);
            }

            if (endDate == null)
            {
                endDate = now;
            }
        }

        return new LogTimePeriod(startDate.Value, endDate.Value);
    }

    [HttpGet]
    public IEnumerable<SavedLogSearch>? GetSavedSearches() => _logViewer.GetSavedSearches();

    [HttpPost]
    public IEnumerable<SavedLogSearch>? PostSavedSearch(SavedLogSearch item) =>
        _logViewer.AddSavedSearch(item.Name, item.Query);

    [HttpPost]
    public IEnumerable<SavedLogSearch>? DeleteSavedSearch(SavedLogSearch item) =>
        _logViewer.DeleteSavedSearch(item.Name, item.Query);

    [HttpGet]
    public ReadOnlyDictionary<string, LogEventLevel?> GetLogLevels() => _logLevelLoader.GetLogLevelsFromSinks();

    [Obsolete("Please use GetLogLevels() instead. Scheduled for removal in V11.")]
    [HttpGet]
    public string GetLogLevel() => _logViewer.GetLogLevel();
}
