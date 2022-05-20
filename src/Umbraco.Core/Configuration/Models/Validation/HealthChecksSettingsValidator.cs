// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration representated as <see cref="HealthChecksSettings" />.
/// </summary>
public class HealthChecksSettingsValidator : ConfigurationValidatorBase, IValidateOptions<HealthChecksSettings>
{
    private readonly ICronTabParser _cronTabParser;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthChecksSettingsValidator" /> class.
    /// </summary>
    /// <param name="cronTabParser">Helper for parsing crontab expressions.</param>
    public HealthChecksSettingsValidator(ICronTabParser cronTabParser) => _cronTabParser = cronTabParser;

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string name, HealthChecksSettings options)
    {
        if (!ValidateNotificationFirstRunTime(options.Notification.FirstRunTime, out var message))
        {
            return ValidateOptionsResult.Fail(message);
        }

        return ValidateOptionsResult.Success;
    }

    private bool ValidateNotificationFirstRunTime(string value, out string message) =>
        ValidateOptionalCronTab(
            $"{Constants.Configuration.ConfigHealthChecks}:{nameof(HealthChecksSettings.Notification)}:{nameof(HealthChecksSettings.Notification.FirstRunTime)}",
            value,
            out message);

    private bool ValidateOptionalCronTab(string configPath, string value, out string message)
    {
        if (!string.IsNullOrEmpty(value) && !_cronTabParser.IsValidCronTab(value))
        {
            message = $"Configuration entry {configPath} contains an invalid cron expression.";
            return false;
        }

        message = string.Empty;
        return true;
    }
}
