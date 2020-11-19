using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.HealthCheck;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Microsoft.Extensions.Logging;
using Umbraco.Web.BackOffice.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.HealthCheck
{
    /// <summary>
    /// The API controller used to display the health check info and execute any actions
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
    public class HealthCheckController : UmbracoAuthorizedJsonController
    {
        private readonly HealthCheckCollection _checks;
        private readonly IList<Guid> _disabledCheckIds;
        private readonly ILogger<HealthCheckController> _logger;

        public HealthCheckController(HealthCheckCollection checks, ILogger<HealthCheckController> logger, IOptions<HealthChecksSettings> healthChecksSettings)
        {
            _checks = checks ?? throw new ArgumentNullException(nameof(checks));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var healthCheckConfig = healthChecksSettings.Value ?? throw new ArgumentNullException(nameof(healthChecksSettings));
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
                _logger.LogError(ex, "Exception in health check: {HealthCheckName}", check.Name);
                throw;
            }
        }

        [HttpPost]
        public HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            var check = GetCheckById(action.HealthCheckId);
            return check.ExecuteAction(action);
        }

        private Core.HealthCheck.HealthCheck GetCheckById(Guid id)
        {
            var check = _checks
                .Where(x => _disabledCheckIds.Contains(x.Id) == false)
                .FirstOrDefault(x => x.Id == id);

            if (check == null) throw new InvalidOperationException($"No health check found with id {id}");

            return check;
        }
    }
}
