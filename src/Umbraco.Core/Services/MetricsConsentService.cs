using System;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.Services
{
    public class MetricsConsentService : IMetricsConsentService
    {
        internal const string Key = "UmbracoAnalyticsLevel";

        private readonly IKeyValueService _keyValueService;
        private readonly ILogger<MetricsConsentService> _logger;
        private readonly IBackOfficeSecurity _backOfficeSecurity;

        public MetricsConsentService(IKeyValueService keyValueService)
        {
            _keyValueService = keyValueService;
        }

        public MetricsConsentService(
            IKeyValueService keyValueService,
            ILogger<MetricsConsentService> logger,
            IBackOfficeSecurity backOfficeSecurity)
        {
            _keyValueService = keyValueService;
            _logger = logger;
            _backOfficeSecurity = backOfficeSecurity;
        }

        public TelemetryLevel GetConsentLevel()
        {
            var analyticsLevelString = _keyValueService.GetValue(Key);

            if (analyticsLevelString is null || Enum.TryParse(analyticsLevelString, out TelemetryLevel analyticsLevel) is false)
            {
                return TelemetryLevel.Basic;
            }

            return analyticsLevel;
        }

        public void SetConsentLevel(TelemetryLevel telemetryLevel)
        {
            var currentUser = _backOfficeSecurity.CurrentUser;
            _logger.LogInformation($"Telemetry level changed by {currentUser.Name}");
            _keyValueService.SetValue(Key, telemetryLevel.ToString());
        }
    }
}
