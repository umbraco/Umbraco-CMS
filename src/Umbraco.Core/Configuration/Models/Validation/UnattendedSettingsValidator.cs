// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration representated as <see cref="UnattendedSettings" />.
/// </summary>
public class UnattendedSettingsValidator
    : IValidateOptions<UnattendedSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string name, UnattendedSettings options)
    {
        if (options.InstallUnattended)
        {
            var setValues = 0;
            if (!string.IsNullOrEmpty(options.UnattendedUserName))
            {
                setValues++;
            }

            if (!string.IsNullOrEmpty(options.UnattendedUserEmail))
            {
                setValues++;
            }

            if (!string.IsNullOrEmpty(options.UnattendedUserPassword))
            {
                setValues++;
            }

            if (setValues > 0 && setValues < 3)
            {
                return ValidateOptionsResult.Fail(
                    $"Configuration entry {Constants.Configuration.ConfigUnattended} contains invalid values.\nIf any of the {nameof(options.UnattendedUserName)}, {nameof(options.UnattendedUserEmail)}, {nameof(options.UnattendedUserPassword)} are set, all of them are required.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}
