using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Web.Editors;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.HealthCheck
{
    /// <summary>
    /// The API controller used to display the health check info and execute any actions
    /// </summary>
    [UmbracoApplicationAuthorize(Core.Constants.Applications.Developer)]
    public class HealthCheckController : UmbracoAuthorizedJsonController
    {
        private readonly IHealthCheckResolver _healthCheckResolver;
        private readonly IList<Guid> _disabledCheckIds;

        public HealthCheckController()
        {
            _healthCheckResolver = HealthCheckResolver.Current;

            var healthCheckConfig = UmbracoConfig.For.HealthCheck();
            _disabledCheckIds = healthCheckConfig.DisabledChecks
                .Select(x => x.Id)
                .ToList();
        }

        [Obsolete("Use the contructor specifying all parameters instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public HealthCheckController(IHealthCheckResolver healthCheckResolver)
        {
            _healthCheckResolver = healthCheckResolver;
            var healthCheckConfig = UmbracoConfig.For.HealthCheck();
            _disabledCheckIds = healthCheckConfig.DisabledChecks
                .Select(x => x.Id)
                .ToList();
        }

        public HealthCheckController(IHealthCheckResolver healthCheckResolver, IHealthChecks healthCheckConfig)
        {
            if (healthCheckResolver == null) throw new ArgumentNullException("healthCheckResolver");
            if (healthCheckConfig == null) throw new ArgumentNullException("healthCheckConfig");

            _healthCheckResolver = healthCheckResolver;
            _disabledCheckIds = healthCheckConfig.DisabledChecks
                .Select(x => x.Id)
                .ToList();
        }

        /// <summary>
        /// Gets a grouped list of health checks, but doesn't actively check the status of each health check.
        /// </summary>
        /// <returns>Returns a collection of anonymous objects representing each group.</returns>
        public object GetAllHealthChecks()
        {
            var groups = _healthCheckResolver.HealthChecks
                .Where(x => _disabledCheckIds.Contains(x.Id) == false)
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

        [HttpGet]
        public object GetStatus(Guid id)
        {
            var check = GetCheckById(id);
            
            try
            {
                //Core.Logging.LogHelper.Debug<HealthCheckController>("Running health check: " + check.Name);
                return check.GetStatus();
            }
            catch (Exception e)
            {
                Core.Logging.LogHelper.Error<HealthCheckController>("Exception in health check: " + check.Name, e);
                throw;
            }
        }

        [HttpPost]
        public HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            var check = GetCheckById(action.HealthCheckId);
            return check.ExecuteAction(action);
        }

        private HealthCheck GetCheckById(Guid id)
        {
            var check = _healthCheckResolver.HealthChecks
                .Where(x => _disabledCheckIds.Contains(x.Id) == false)
                .FirstOrDefault(x => x.Id == id);

            if (check == null) throw new InvalidOperationException(string.Format("No health check found with id {0}", id));

            return check;
        }
    }
}