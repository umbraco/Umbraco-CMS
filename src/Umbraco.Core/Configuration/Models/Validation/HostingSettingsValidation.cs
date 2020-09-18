using Microsoft.Extensions.Options;

namespace Umbraco.Core.Configuration.Models.Validation
{
    public class HostingSettingsValidation : ConfigurationValidationBase, IValidateOptions<HostingSettings>
    {
        public ValidateOptionsResult Validate(string name, HostingSettings options)
        {
            if (!ValidateLocalTempStorageLocation(options.LocalTempStorageLocation, out var message))
            {
                return ValidateOptionsResult.Fail(message);
            }

            return ValidateOptionsResult.Success;
        }

        private bool ValidateLocalTempStorageLocation(string value, out string message)
        {
            return ValidateStringIsOneOfEnumValues("Hosting:LocalTempStorageLocation", value, typeof(LocalTempStorage), out message);
        }
    }
}
