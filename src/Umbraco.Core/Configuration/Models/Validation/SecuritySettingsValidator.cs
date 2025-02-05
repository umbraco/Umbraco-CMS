// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration representated as <see cref="SecuritySettings" />.
/// </summary>
public class SecuritySettingsValidator
    : ConfigurationValidatorBase, IValidateOptions<SecuritySettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, SecuritySettings options)
    {
        if (!ValidateMemberRequireUniqueEmailSetting(options.MemberRequireUniqueEmail, options.UsernameIsEmail, out string message))
        {
            return ValidateOptionsResult.Fail(message);
        }

        return ValidateOptionsResult.Success;
    }

    private bool ValidateMemberRequireUniqueEmailSetting(bool memberRequireUniqueEmail, bool usernameIsEmail, out string message)
    {
        // Only allow this setting if user name is email is false (as we need the user name to be unique to identify the user on login).
        if (memberRequireUniqueEmail is false && usernameIsEmail)
        {
            message =
                $"The `{Constants.Configuration.ConfigSecurity}:{nameof(SecuritySettings.MemberRequireUniqueEmail)}` value cannot be set to false if {Constants.Configuration.ConfigSecurity}:{nameof(SecuritySettings.UsernameIsEmail)} is set to true.";
            return false;
        }

        message = string.Empty;
        return true;
    }
}
