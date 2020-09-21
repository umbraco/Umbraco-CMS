using System.Net.Mail;
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
            return ValidateOptionalEntry("Global:Smtp", value, "A valid From email address is required", out message) &&
                (value == null || ValidateStringIsOneOfEnumValues("Global:Smtp:DeliveryMethod", value.DeliveryMethod, typeof(SmtpDeliveryMethod), out message));
        }
    }
}
