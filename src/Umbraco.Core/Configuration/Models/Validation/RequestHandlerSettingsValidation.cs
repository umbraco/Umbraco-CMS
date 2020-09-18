using Microsoft.Extensions.Options;

namespace Umbraco.Core.Configuration.Models.Validation
{
    public class RequestHandlerSettingsValidation : ConfigurationValidationBase, IValidateOptions<RequestHandlerSettings>
    {
        public ValidateOptionsResult Validate(string name, RequestHandlerSettings options)
        {
            if (!ValidateConvertUrlsToAscii(options.ConvertUrlsToAscii, out var message))
            {
                return ValidateOptionsResult.Fail(message);
            }

            return ValidateOptionsResult.Success;
        }

        private bool ValidateConvertUrlsToAscii(string value, out string message)
        {
            var validValues = new[] { "try", "true", "false" };
            return ValidateStringIsOneOfValidValues("RequestHandler:ConvertUrlsToAscii", value, validValues, out message);
        }
    }
}
