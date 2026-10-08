using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

/// <summary>
/// Provides telemetry data about language usage in Umbraco CMS.
/// </summary>
public class LanguagesTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly ILanguageService _languageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguagesTelemetryProvider"/> class, which provides telemetry data related to languages.
    /// </summary>
    /// <param name="languageService">An instance of <see cref="ILanguageService"/> used to retrieve language data.</param>
    public LanguagesTelemetryProvider(ILanguageService languageService) =>
        _languageService = languageService;

    /// <summary>
    /// Retrieves telemetry information about the number of languages configured in the system.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{UsageInformation}"/> containing a single entry with the total count of configured languages.
    /// </returns>
    public IEnumerable<UsageInformation> GetInformation()
    {
        var languages = _languageService.GetAllAsync().GetAwaiter().GetResult().Count();
        yield return new UsageInformation(Constants.Telemetry.LanguageCount, languages);
    }
}
