using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Factories;

public class UserSettingsFactory : IUserSettingsFactory
{
    private readonly ILocalizedTextService _localizedTextService;
    private readonly UserPasswordConfigurationSettings _passwordConfiguration;

    public UserSettingsFactory(
        IOptions<UserPasswordConfigurationSettings> securitySettings,
        ILocalizedTextService localizedTextService)
    {
        _localizedTextService = localizedTextService;
        _passwordConfiguration = securitySettings.Value;
    }

    public UserSettingsModel GetUserSettings() =>
        new()
        {
            PasswordSettings = CreatePasswordSettingsModel(),
            ConsentLevels = CreateConsentLevelModels(),
        };

    private PasswordSettingsModel CreatePasswordSettingsModel() =>
        new()
        {
            MinCharLength = _passwordConfiguration.RequiredLength,
            MinNonAlphaNumericLength = _passwordConfiguration.GetMinNonAlphaNumericChars()
        };

    private IEnumerable<ConsentLevelModel> CreateConsentLevelModels() =>
        Enum.GetValues<TelemetryLevel>()
            .ToList()
            .Select(level => new ConsentLevelModel
            {
                Level = level,
                Description = GetTelemetryLevelDescription(level),
            });

    private string GetTelemetryLevelDescription(TelemetryLevel telemetryLevel) => telemetryLevel switch
    {
        TelemetryLevel.Minimal => _localizedTextService.Localize("analytics", "minimalLevelDescription"),
        TelemetryLevel.Basic => _localizedTextService.Localize("analytics", "basicLevelDescription"),
        TelemetryLevel.Detailed => _localizedTextService.Localize("analytics", "detailedLevelDescription"),
        _ => throw new ArgumentOutOfRangeException(nameof(telemetryLevel), $"Did not expect telemetry level of {telemetryLevel}")
    };
}
