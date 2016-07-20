using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Umbraco.Web.Editors;

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

        /// <summary>
        /// Gets a grouped list of health checks, but doesn't actively check the status of each health check.
        /// </summary>
        /// <returns>Returns a collection of anonymous objects representing each group.</returns>
        public object GetAllHealthChecks()
        {
            var context = new HealthCheckContext(new HttpContextWrapper(HttpContext.Current), UmbracoContext.Current);
            var groups = _healthCheckResolver.GetHealthChecks(context)
                .GroupBy(x => x.Group)
                .OrderBy(x => x.Key);
            var healthCheckGroups = new List<HealthCheckGroup>();
            foreach (var healthCheckGroup in groups)
            {
                var hcGroup = new HealthCheckGroup
                {
                    Name = healthCheckGroup.Key,
                    Checks = healthCheckGroup
                        .OrderBy(x => x.Name)
                        .ToList()
                };
                healthCheckGroups.Add(hcGroup);
            }

            return healthCheckGroups;
        }

        public object GetStatus(Guid id)
        {
            var context = new HealthCheckContext(new HttpContextWrapper(HttpContext.Current), UmbracoContext.Current);
            var check = _healthCheckResolver.GetHealthChecks(context).FirstOrDefault(x => x.Id == id);
            if (check == null) throw new InvalidOperationException("No health check found with ID " + id);

            return check.GetStatus();
        }

        [HttpPost]
        public HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            var context = new HealthCheckContext(new HttpContextWrapper(HttpContext.Current), UmbracoContext.Current);
            var check = _healthCheckResolver.GetHealthChecks(context).FirstOrDefault(x => x.Id == action.HealthCheckId);
            if (check == null) throw new InvalidOperationException("No health check found with id " + action.HealthCheckId);

            return check.ExecuteAction(action);
        }
    }
}