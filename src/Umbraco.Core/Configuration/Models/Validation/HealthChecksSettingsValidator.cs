using Microsoft.Extensions.Options;

namespace Umbraco.Core.Configuration.Models.Validation
{
    public class HealthChecksSettingsValidator : ConfigurationValidatorBase, IValidateOptions<HealthChecksSettings>
    {
        private readonly ICronTabParser _cronTabParser;

        public HealthChecksSettingsValidator(ICronTabParser cronTabParser)
        {
            _cronTabParser = cronTabParser;
        }

        public ValidateOptionsResult Validate(string name, HealthChecksSettings options)
        {
            if (!ValidateNotificationFirstRunTime(options.Notification.FirstRunTime, out var message))
            {
                return ValidateOptionsResult.Fail(message);
            }

            return ValidateOptionsResult.Success;
        }

        private bool ValidateNotificationFirstRunTime(string value, out string message)
        {
            return ValidateOptionalCronTab($"{Constants.Configuration.ConfigHealthChecks}:{nameof(HealthChecksSettings.Notification)}:{nameof(HealthChecksSettings.Notification.FirstRunTime)}", value, out message);
        }

        public bool ValidateOptionalCronTab(string configPath, string value, out string message)
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
}
