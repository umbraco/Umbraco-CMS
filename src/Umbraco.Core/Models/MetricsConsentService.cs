using System;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models
{
    public class MetricsConsentService : IMetricsConsentService
    {

        private const string Key = "UmbracoAnalyticsLevel";

        private readonly IKeyValueService _keyValueService;

        public MetricsConsentService(IKeyValueService keyValueService)
        {
            _keyValueService = keyValueService;
        }

        public ConsentLevel GetConsentLevel()
        {
            var analyticsLevelString = _keyValueService.GetValue(Key);

            if (analyticsLevelString is null || Enum.TryParse(analyticsLevelString, out ConsentLevel analyticsLevel) is false)
            {
                return ConsentLevel.Basic;
            }

            return analyticsLevel;
        }

        public ConsentLevel SetConsentLevel(ConsentLevel consentLevel)
        {
            _keyValueService.SetValue(Key, consentLevel.ToString());
            return consentLevel;
        }
    }
}
