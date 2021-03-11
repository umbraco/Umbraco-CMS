// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation
{
    /// <summary>
    /// Validator for configuration representated as <see cref="GlobalSettings"/>.
    /// </summary>
    public class GlobalSettingsValidator
        : ConfigurationValidatorBase, IValidateOptions<GlobalSettings>
    {
        /// <inheritdoc/>
        public ValidateOptionsResult Validate(string name, GlobalSettings options)
        {
            if (!ValidateSmtpSetting(options.Smtp, out var message))
            {
                return ValidateOptionsResult.Fail(message);
            }

            if (!ValidateSqlWriteLockTimeOutSetting(options.SqlWriteLockTimeOut, out var message2))
            {
                return ValidateOptionsResult.Fail(message2);
            }

            return ValidateOptionsResult.Success;
        }

        private bool ValidateSmtpSetting(SmtpSettings value, out string message) =>
            ValidateOptionalEntry($"{Constants.Configuration.ConfigGlobal}:{nameof(GlobalSettings.Smtp)}", value, "A valid From email address is required", out message);

        private bool ValidateSqlWriteLockTimeOutSetting(TimeSpan configuredTimeOut, out string message) {
            // Only apply this setting if it's not excessively high or low
            const int minimumTimeOut = 100;
            const int maximumTimeOut = 20000;
            if (configuredTimeOut.TotalMilliseconds < minimumTimeOut || configuredTimeOut.TotalMilliseconds > maximumTimeOut) // between 0.1 and 20 seconds
            {
                message = $"The `{Constants.Configuration.ConfigGlobal}:{nameof(GlobalSettings.SqlWriteLockTimeOut)}` setting is not between the minimum of {minimumTimeOut} ms and maximum of {maximumTimeOut} ms";
                return false;
            }

            message = string.Empty;
            return true;
        }
    }
}
