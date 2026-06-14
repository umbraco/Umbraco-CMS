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
        private readonly IRuntimeStartupReadiness _readiness;

        public MaintenanceModeActionFilter(
            IRuntimeState runtimeState,
            IOptionsMonitor<GlobalSettings> globalSettings,
            IRuntimeStartupReadiness readiness)
        {
            _runtimeState = runtimeState;
            _globalSettings = globalSettings;
            _readiness = readiness;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<SkipMaintenanceModeFilterAttribute>().Any())
            {
                return;
            }

            if (_globalSettings.CurrentValue.ShowMaintenancePageWhenInUpgradeState is false)
            {
                return;
            }

            bool isApiController = context.ActionDescriptor.EndpointMetadata.OfType<ApiControllerAttribute>().Any();

            // After an unattended upgrade the level flips to Run before post-migration initialization
            // (e.g. document URL routing) completes; keep the front end gated until it is ready to serve.
            bool deferredStartupPending = IsDeferredStartupPending();

            // API controllers (Management/Delivery API) are only blocked during an unattended upgrade
            // (RuntimeLevel.Upgrading). During an attended upgrade (RuntimeLevel.Upgrade) the operator
            // needs API access to log in and trigger the upgrade from the backoffice.
            // MVC controllers (website, surface) are blocked during both Upgrade and Upgrading, and also
            // during the brief not-yet-ready window after the level flips to Run.
            bool shouldBlock = ShouldBlockRequest(isApiController, deferredStartupPending);

            if (shouldBlock is false)
            {
                return;
            }

            if (isApiController)
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

            // The not-yet-ready window is the tail end of an unattended upgrade, so show the same
            // "upgrade in progress" page as during the upgrade itself.
            context.Result = _runtimeState.Level == RuntimeLevel.Upgrading || deferredStartupPending
                ? new MaintenanceResult(_globalSettings.CurrentValue.UpgradingViewPath)
                : new MaintenanceResult();
        }

        private bool IsDeferredStartupPending() =>
            _runtimeState.Level == RuntimeLevel.Run && _readiness.IsReadyToServe is false;

        private bool ShouldBlockRequest(bool isApiController, bool deferredStartupPending) =>
            isApiController
                ? _runtimeState.Level == RuntimeLevel.Upgrading
                : _runtimeState.Level is RuntimeLevel.Upgrade or RuntimeLevel.Upgrading || deferredStartupPending;

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
