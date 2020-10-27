using Microsoft.Extensions.Options;

namespace Umbraco.Core.Configuration.Models.Validation
{
    public class HealthChecksSettingsValidator : ConfigurationValidatorBase, IValidateOptions<HealthChecksSettings>
    {
        public ValidateOptionsResult Validate(string name, HealthChecksSettings options)
        {
            string message;
            if (!ValidateNotificationFirstRunTime(options.Notification.FirstRunTime, out message))
            {
                return ValidateOptionsResult.Fail(message);
            }

            return ValidateOptionsResult.Success;
        }

        private bool ValidateNotificationFirstRunTime(string value, out string message)
        {
            return ValidateOptionalTime($"{Constants.Configuration.ConfigHealthChecks}:{nameof(HealthChecksSettings.Notification)}:{nameof(HealthChecksSettings.Notification.FirstRunTime)}", value, out message);
        }
    }
}
