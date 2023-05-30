using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Core.Models.Configuration;

// TODO: merge this class with relevant existing settings validators from Core and clean up
public class NewBackOfficeSettingsValidator : ConfigurationValidatorBase, IValidateOptions<NewBackOfficeSettings>
{
    public ValidateOptionsResult Validate(string? name, NewBackOfficeSettings options)
    {
        if (options.BackOfficeHost != null)
        {
            if (options.BackOfficeHost.IsAbsoluteUri == false)
            {
                return ValidateOptionsResult.Fail($"{nameof(NewBackOfficeSettings.BackOfficeHost)} must be an absolute URL");
            }
            if (options.BackOfficeHost.PathAndQuery != "/")
            {
                return ValidateOptionsResult.Fail($"{nameof(NewBackOfficeSettings.BackOfficeHost)} must not have any path or query");
            }
        }

        return ValidateOptionsResult.Success;
    }
}
