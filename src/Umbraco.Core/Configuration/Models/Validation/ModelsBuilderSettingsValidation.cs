using Microsoft.Extensions.Options;

namespace Umbraco.Core.Configuration.Models.Validation
{
    public class ModelsBuilderSettingsValidation : ConfigurationValidationBase, IValidateOptions<ModelsBuilderSettings>
    {
        public ValidateOptionsResult Validate(string name, ModelsBuilderSettings options)
        {
            if (!ValidateModelsMode(options.ModelsMode, out var message))
            {
                return ValidateOptionsResult.Fail(message);
            }

            return ValidateOptionsResult.Success;
        }

        private bool ValidateModelsMode(string value, out string message)
        {
            return ValidateStringIsOneOfEnumValues("ModelsBuilder:ModelsMode", value, typeof(ModelsMode), out message);
        }
    }
}
