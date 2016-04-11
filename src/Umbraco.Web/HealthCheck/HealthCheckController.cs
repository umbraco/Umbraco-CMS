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

        /// <summary>
        /// Gets a grouped list of health checks, but doesn't actively check the status of each health check.
        /// </summary>
        /// <returns>Returns a collection of anonymous objects representing each group.</returns>
        public object GetAllHealthChecks() {
            return (
                from gr in _healthCheckResolver.HealthChecks.GroupBy(x => x.Group) 
                select new {
                    name = gr.Key,
                    checks = (
                        from check in gr
                        select new {
                            id = check.Id,
                            name = check.Name,
                            description = check.Description
                        }
                    )
                }
            );
        }

        public object GetStatus(Guid id) {

            HealthCheck check = _healthCheckResolver.HealthChecks.FirstOrDefault(x => x.Id == id);
            if (check == null) throw new InvalidOperationException("No health check found with ID " + id);

            return check.GetStatus();

        } 

        public HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            var check = _healthCheckResolver.HealthChecks.FirstOrDefault(x => x.Id == action.HealthCheckId);
            if (check == null) throw new InvalidOperationException("No health check found with id " + action.HealthCheckId);

            return check.ExecuteAction(action);
        }
    }
}