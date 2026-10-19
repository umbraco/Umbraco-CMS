using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration represented as <see cref="SecuritySettings" />.
/// </summary>
public class SecuritySettingsValidator : ConfigurationValidatorBase, IValidateOptions<SecuritySettings>
{
    // Mirrors Microsoft.AspNetCore.Http.SameSiteMode, which Umbraco.Core cannot reference.
    // SecuritySettingsValidatorTests guards this list against the real enum drifting away from it.
    private static readonly string[] AuthCookieSameSiteNames = ["Unspecified", "None", "Lax", "Strict"];

    // Enum.TryParse also resolves the underlying numbers, so configuration spelled that way works and
    // must not be rejected here. Only the names are advertised in the failure message.
    private static readonly string[] AuthCookieSameSiteNumbers = ["-1", "0", "1", "2"];

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, SecuritySettings options)
    {
        if (ValidateAuthCookieSameSite(options.AuthCookieSameSite, out var message) is false)
        {
            return ValidateOptionsResult.Fail(message);
        }

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

    private bool ValidateAuthCookieSameSite(string value, out string message)
    {
        if (AuthCookieSameSiteNumbers.Contains(value))
        {
            message = string.Empty;
            return true;
        }

        return ValidateStringIsOneOfValidValues(
            $"{Constants.Configuration.ConfigSecurity}:{nameof(SecuritySettings.AuthCookieSameSite)}",
            value,
            AuthCookieSameSiteNames,
            out message);
    }
}
