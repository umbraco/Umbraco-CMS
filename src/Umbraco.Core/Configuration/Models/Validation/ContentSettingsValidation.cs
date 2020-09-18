using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Core.Macros;

namespace Umbraco.Core.Configuration.Models.Validation
{
    public class ContentSettingsValidation : ConfigurationValidationBase, IValidateOptions<ContentSettings>
    {
        public ValidateOptionsResult Validate(string name, ContentSettings options)
        {
            string message;
            if (!ValidateMacroErrors(options.MacroErrors, out message))
            {
                return ValidateOptionsResult.Fail(message);
            }

            if (!ValidateError404Collection(options.Error404Collection, out message))
            {
                return ValidateOptionsResult.Fail(message);
            }

            return ValidateOptionsResult.Success;
        }

        private bool ValidateMacroErrors(string value, out string message)
        {
            return ValidateStringIsOneOfEnumValues("Content:MacroErrors", value, typeof(MacroErrorBehaviour), out message);
        }

        private bool ValidateError404Collection(IEnumerable<ContentErrorPage> values, out string message)
        {
            if (values.Any(x => !x.IsValid()))
            {
                message = $"Configuration entry Content:Error404Collection contains one or more invalid values.  Culture and one and only one of ContentId, ContentKey and ContentXPath must be specified for each entry.";
                return false;
            }

            message = string.Empty;
            return true;
        }
    }
}
