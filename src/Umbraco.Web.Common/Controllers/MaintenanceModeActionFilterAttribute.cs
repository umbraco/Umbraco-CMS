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
internal sealed class MaintenanceModeActionFilterAttribute : TypeFilterAttribute
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
            if (_runtimeState.Level == RuntimeLevel.Upgrade
                && _globalSettings.CurrentValue.ShowMaintenancePageWhenInUpgradeState)
            {
                context.Result = new MaintenanceResult();
            }
            else if (_runtimeState.Level == RuntimeLevel.Upgrading
                     && _globalSettings.CurrentValue.ShowMaintenancePageWhenInUpgradeState)
            {
                context.Result = new MaintenanceResult(_globalSettings.CurrentValue.UpgradingViewPath);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
