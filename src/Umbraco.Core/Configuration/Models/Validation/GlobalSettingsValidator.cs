using Microsoft.Extensions.Options;

namespace Umbraco.Core.Configuration.Models.Validation
{
    public class GlobalSettingsValidator
        : ConfigurationValidatorBase, IValidateOptions<GlobalSettings>
    {
        public ValidateOptionsResult Validate(string name, GlobalSettings options)
        {
            if (!ValidateSmtpSetting(options.Smtp, out var message))
            {
                return ValidateOptionsResult.Fail(message);
            }

            return ValidateOptionsResult.Success;
        }

        private bool ValidateSmtpSetting(SmtpSettings value, out string message)
        {
            return ValidateOptionalEntry($"{Constants.Configuration.ConfigGlobal}:{nameof(GlobalSettings.Smtp)}", value, "A valid From email address is required", out message);
        }
    }
}
