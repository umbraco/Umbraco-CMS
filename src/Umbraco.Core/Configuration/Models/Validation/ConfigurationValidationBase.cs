using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Configuration.Models.Validation
{
    public abstract class ConfigurationValidationBase
    {
        public bool ValidateStringIsOneOfValidValues(string configPath, string value, IEnumerable<string> validValues, out string message)
        {
            if (!validValues.InvariantContains(value))
            {
                message = $"Configuration entry {configPath} contains an invalid value '{value}', it should be one of the following: '{string.Join(", ", validValues)}'.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public bool ValidateStringIsOneOfEnumValues(string configPath, string value, Type enumType, out string message)
        {
            var validValues = Enum.GetValues(enumType).OfType<object>().Select(x => x.ToString().ToFirstLowerInvariant());
            return ValidateStringIsOneOfValidValues(configPath, value, validValues, out message);
        }

        public bool ValidateCollection(string configPath, IEnumerable<ValidatableEntryBase> values, string validationDescription, out string message)
        {
            if (values.Any(x => !x.IsValid()))
            {
                message = $"Configuration entry {configPath} contains one or more invalid values. {validationDescription}.";
                return false;
            }

            message = string.Empty;
            return true;
        }
    }
}
