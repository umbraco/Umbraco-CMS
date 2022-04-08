using System;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{
    public class MetricsConsentService : IMetricsConsentService
    {
        internal const string Key = "UmbracoAnalyticsLevel";

        private readonly IKeyValueService _keyValueService;

        public MetricsConsentService(IKeyValueService keyValueService)
        {
            _keyValueService = keyValueService;
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
            _keyValueService.SetValue(Key, telemetryLevel.ToString());
        }
    }
}
