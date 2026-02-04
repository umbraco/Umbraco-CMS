using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Factories;

/// <summary>
/// Factory for creating <see cref="UserSettingsModel"/> instances containing user-related settings for installation.
/// </summary>
public class UserSettingsFactory : IUserSettingsFactory
{
    private readonly ILocalizedTextService _localizedTextService;
    private readonly UserPasswordConfigurationSettings _passwordConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSettingsFactory"/> class.
    /// </summary>
    /// <param name="securitySettings">The user password configuration settings.</param>
    /// <param name="localizedTextService">The localized text service for retrieving localized descriptions.</param>
    public UserSettingsFactory(
        IOptions<UserPasswordConfigurationSettings> securitySettings,
        ILocalizedTextService localizedTextService)
    {
        _localizedTextService = localizedTextService;
        _passwordConfiguration = securitySettings.Value;
    }

    /// <inheritdoc />
    public UserSettingsModel GetUserSettings() =>
        new()
        {
            PasswordSettings = CreatePasswordSettingsModel(),
            ConsentLevels = CreateConsentLevelModels(),
        };

    /// <summary>
    /// Creates the password settings model from the current password configuration.
    /// </summary>
    /// <returns>A <see cref="PasswordSettingsModel"/> containing password requirements.</returns>
    private PasswordSettingsModel CreatePasswordSettingsModel() =>
        new()
        {
            MinCharLength = _passwordConfiguration.RequiredLength,
            MinNonAlphaNumericLength = _passwordConfiguration.GetMinNonAlphaNumericChars()
        };

    /// <summary>
    /// Creates consent level models for all available telemetry levels.
    /// </summary>
    /// <returns>A collection of <see cref="ConsentLevelModel"/> for each telemetry level.</returns>
    private IEnumerable<ConsentLevelModel> CreateConsentLevelModels() =>
        Enum.GetValues<TelemetryLevel>()
            .Select(level => new ConsentLevelModel
            {
                Level = level,
                Description = GetTelemetryLevelDescription(level),
            });

    /// <summary>
    /// Gets the localized description for a telemetry level.
    /// </summary>
    /// <param name="telemetryLevel">The telemetry level to get the description for.</param>
    /// <returns>The localized description string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unexpected telemetry level is provided.</exception>
    private string GetTelemetryLevelDescription(TelemetryLevel telemetryLevel) => telemetryLevel switch
    {
        TelemetryLevel.Minimal => _localizedTextService.Localize("analytics", "minimalLevelDescription"),
        TelemetryLevel.Basic => _localizedTextService.Localize("analytics", "basicLevelDescription"),
        TelemetryLevel.Detailed => _localizedTextService.Localize("analytics", "detailedLevelDescription"),
        _ => throw new ArgumentOutOfRangeException(nameof(telemetryLevel), $"Did not expect telemetry level of {telemetryLevel}")
    };
}
