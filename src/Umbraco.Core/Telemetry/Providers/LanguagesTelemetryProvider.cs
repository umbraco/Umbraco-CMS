using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class LanguagesTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly ILocalizationService _localizationService;

        public LanguagesTelemetryProvider(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public IEnumerable<UsageInformation> GetInformation()
        {
            var result = new List<UsageInformation>();
            int languages = _localizationService.GetAllLanguages().Count();

            result.Add(new ("LanguageCount", languages));
            return result;
        }
    }
}
