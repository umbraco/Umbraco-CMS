// Copyright (c) Umbraco.
// See LICENSE for more details.

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

            return ValidateOptionsResult.Success;
        }

        private bool ValidateSmtpSetting(SmtpSettings value, out string message) =>
            ValidateOptionalEntry($"{Constants.Configuration.ConfigGlobal}:{nameof(GlobalSettings.Smtp)}", value, "A valid From email address is required", out message);
    }
}
