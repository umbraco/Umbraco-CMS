using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.ActionsResults;

namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
/// Represents an action filter attribute that enforces maintenance mode by displaying a maintenance page during
/// application upgrades.
/// </summary>
public sealed class MaintenanceModeActionFilterAttribute : TypeFilterAttribute
{

    /// <summary>
    /// Initializes a new instance of the <see cref="MaintenanceModeActionFilterAttribute"/> class.
    /// </summary>
    public MaintenanceModeActionFilterAttribute()
        : base(typeof(MaintenanceModeActionFilter)) => Order = int.MinValue; // Ensures this run as the first filter.

    private sealed class MaintenanceModeActionFilter : IActionFilter
    {
        private readonly IRuntimeState _runtimeState;
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;

        public MaintenanceModeActionFilter(IRuntimeState runtimeState, IOptionsMonitor<GlobalSettings> globalSettings)
        {
            _runtimeState = runtimeState;
            _globalSettings = globalSettings;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            bool shouldBlock = _runtimeState.Level is RuntimeLevel.Upgrade or RuntimeLevel.Upgrading
                               && _globalSettings.CurrentValue.ShowMaintenancePageWhenInUpgradeState;

            if (shouldBlock is false)
            {
                return;
            }

            if (context.ActionDescriptor.EndpointMetadata.OfType<ApiControllerAttribute>().Any())
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Title = "Service Unavailable",
                    Detail = "The application is currently being upgraded. Please try again later.",
                };
                context.Result = new ObjectResult(problem) { StatusCode = StatusCodes.Status503ServiceUnavailable };
                return;
            }

            context.Result = _runtimeState.Level == RuntimeLevel.Upgrading
                ? new MaintenanceResult(_globalSettings.CurrentValue.UpgradingViewPath)
                : new MaintenanceResult();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
