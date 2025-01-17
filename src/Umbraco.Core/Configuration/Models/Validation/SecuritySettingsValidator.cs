using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

public class SecuritySettingsValidator : ConfigurationValidatorBase, IValidateOptions<SecuritySettings>
{
    public ValidateOptionsResult Validate(string? name, SecuritySettings options)
    {
        if (options.BackOfficeHost != null)
        {
            if (options.BackOfficeHost.IsAbsoluteUri == false)
            {
                return ValidateOptionsResult.Fail($"{nameof(SecuritySettings.BackOfficeHost)} must be an absolute URL");
            }

            if (options.BackOfficeHost.PathAndQuery != "/")
            {
                return ValidateOptionsResult.Fail($"{nameof(SecuritySettings.BackOfficeHost)} must not have any path or query");
            }
        }

        return ValidateOptionsResult.Success;
    }
}
