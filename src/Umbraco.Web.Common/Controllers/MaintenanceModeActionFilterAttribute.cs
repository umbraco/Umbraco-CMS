using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.ActionsResults;

namespace Umbraco.Cms.Web.Common.Controllers;

internal sealed class MaintenanceModeActionFilterAttribute : TypeFilterAttribute
{

    public MaintenanceModeActionFilterAttribute() : base(typeof(MaintenanceModeActionFilter)) => Order = int.MinValue; // Ensures this run as the first filter.

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
            if (_runtimeState.Level == RuntimeLevel.Upgrade && _globalSettings.CurrentValue.ShowMaintenancePageWhenInUpgradeState)
            {
                context.Result = new MaintenanceResult();
            }

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
