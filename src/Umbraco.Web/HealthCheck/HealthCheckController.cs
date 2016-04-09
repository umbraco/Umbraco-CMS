using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Umbraco.Web.Editors;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.HealthCheck
{
    /// <summary>
    /// The API controller used to display the health check info and execute any actions
    /// </summary>
    public class HealthCheckController : UmbracoAuthorizedJsonController
    {
        private readonly IHealthCheckResolver _healthCheckResolver;

        public HealthCheckController()
        {
            _healthCheckResolver = HealthCheckResolver.Current;
        }

        public HealthCheckController(IHealthCheckResolver healthCheckResolver)
        {
            _healthCheckResolver = healthCheckResolver;
        }

        //TODO: make this happen
        // * Get all checks
        // * Execute action
        // * more?

        public IEnumerable<HealthCheckStatus> GetAllHealthChecks()
        {
            //get the health check instances
            var checks = _healthCheckResolver.HealthChecks;

            //return their statuses
            return checks.SelectMany(x => x.GetStatus());
        }

        public HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            var check = _healthCheckResolver.HealthChecks.FirstOrDefault(x => x.Id == action.HealthCheckId);
            if (check == null) throw new InvalidOperationException("No health check found with id " + action.HealthCheckId);

            return check.ExecuteAction(action);
        }
    }
}