using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.Services
{
    public class MetricsConsentService : IMetricsConsentService
    {
        internal const string Key = "UmbracoAnalyticsLevel";

        private readonly IKeyValueService _keyValueService;
        private readonly ILogger<MetricsConsentService> _logger;
        private readonly IBackOfficeSecurity _backOfficeSecurity;

        // Scheduled for removal in V11
        [Obsolete("Please use the constructor that takes and ILogger and IBackOfficeSecurity instead")]
        public MetricsConsentService(IKeyValueService keyValueService)
        : this(
            keyValueService,
            StaticServiceProvider.Instance.GetRequiredService<ILogger<MetricsConsentService>>(),
            StaticServiceProvider.Instance.GetRequiredService<IBackOfficeSecurity>())
        {
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
            _logger.LogInformation($"Telemetry level changed by {currentUser.Email}");
            _keyValueService.SetValue(Key, telemetryLevel.ToString());
        }
    }
}
