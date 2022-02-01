using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Web.Editors;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.HealthCheck
{
    /// <summary>
    /// The API controller used to display the health check info and execute any actions
    /// </summary>
    [UmbracoApplicationAuthorize(Core.Constants.Applications.Settings)]
    public class HealthCheckController : UmbracoAuthorizedJsonController
    {
        private readonly HealthCheckCollection _checks;
        private readonly IList<Guid> _disabledCheckIds;
        private readonly ILogger _logger;

        public HealthCheckController(HealthCheckCollection checks, ILogger logger)
        {
            _checks = checks ?? throw new ArgumentNullException(nameof(checks));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var healthCheckConfig = Current.Configs.HealthChecks();
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
            var groups = _checks
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
            catch (Exception ex)
            {
                _logger.Error<HealthCheckController, string>(ex, "Exception in health check: {HealthCheckName}", check.Name);
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
            var check = _checks
                .Where(x => _disabledCheckIds.Contains(x.Id) == false)
                .FirstOrDefault(x => x.Id == id);

            if (check == null) throw new InvalidOperationException($"No health check found with id {id}");

            return check;
        }
    }
}
