// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration representated as <see cref="SystemDateMigrationSettings" />.
/// </summary>
public class SystemDateMigrationSettingsValidator
    : IValidateOptions<SystemDateMigrationSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, SystemDateMigrationSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.LocalServerTimeZone))
        {
            return ValidateOptionsResult.Success;
        }

        if (TimeZoneInfo.TryFindSystemTimeZoneById(options.LocalServerTimeZone, out _) is false)
        {
            return ValidateOptionsResult.Fail(
                $"Configuration entry {Constants.Configuration.ConfigSystemDateMigration} contains an invalid time zone: {options.LocalServerTimeZone}.");
        }

        return ValidateOptionsResult.Success;
    }
}
