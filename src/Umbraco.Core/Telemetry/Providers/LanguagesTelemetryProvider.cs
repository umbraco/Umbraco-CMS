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
            int languages = _localizationService.GetAllLanguages().Count();
            yield return new UsageInformation(Constants.Telemetry.LanguageCount, languages);
        }
    }
}
