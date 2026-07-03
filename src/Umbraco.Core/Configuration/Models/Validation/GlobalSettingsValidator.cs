// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration representated as <see cref="GlobalSettings" />.
/// </summary>
public class GlobalSettingsValidator
    : ConfigurationValidatorBase, IValidateOptions<GlobalSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, GlobalSettings options)
    {
        if (!ValidateSmtpSetting(options.Smtp, out var message))
        {
            return ValidateOptionsResult.Fail(message);
        }

        if (!ValidateSqlWriteLockTimeOutSetting(options.DistributedLockingWriteLockDefaultTimeout, out var message2))
        {
            return ValidateOptionsResult.Fail(message2);
        }

        if (!ValidateTimeOutSetting(options.TimeOut, out var message3))
        {
            return ValidateOptionsResult.Fail(message3);
        }

        return ValidateOptionsResult.Success;
    }

    private bool ValidateSmtpSetting(SmtpSettings? value, out string message) =>
        ValidateOptionalEntry($"{Constants.Configuration.ConfigGlobal}:{nameof(GlobalSettings.Smtp)}", value, "A valid From email address is required", out message);

    private bool ValidateSqlWriteLockTimeOutSetting(TimeSpan configuredTimeOut, out string message)
    {
        // Only apply this setting if it's not excessively low
        const int minimumTimeOut = 100;

        // between 0.1 and 20 seconds
        if (configuredTimeOut.TotalMilliseconds < minimumTimeOut)
        {
            message =
                $"The `{Constants.Configuration.ConfigGlobal}:{nameof(GlobalSettings.DistributedLockingWriteLockDefaultTimeout)}` should not be configured as less than {minimumTimeOut} ms";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private bool ValidateTimeOutSetting(TimeSpan configuredTimeOut, out string message)
    {
        // JavaScript's setTimeout maximum delay is 2^31 - 1 milliseconds (~24.85 days); values
        // exceeding it cause timers to fire immediately, breaking session management.
        // Cap at a clean 24 days, comfortably below the limit.
        var maxTimeOut = TimeSpan.FromDays(24);

        if (configuredTimeOut > maxTimeOut)
        {
            message =
                $"The `{Constants.Configuration.ConfigGlobal}:{nameof(GlobalSettings.TimeOut)}` must not exceed {maxTimeOut.TotalDays:F0} days. " +
                $"Values larger than this overflow the browser's maximum timer delay and break session management. " +
                $"Consider using the `KeepUserLoggedIn` setting instead.";
            return false;
        }

        message = string.Empty;
        return true;
    }
}
