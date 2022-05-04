using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

public class LanguagesTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly ILocalizationService _localizationService;

    public LanguagesTelemetryProvider(ILocalizationService localizationService) =>
        _localizationService = localizationService;

    public IEnumerable<UsageInformation> GetInformation()
    {
        var languages = _localizationService.GetAllLanguages().Count();
        yield return new UsageInformation(Constants.Telemetry.LanguageCount, languages);
    }
}
