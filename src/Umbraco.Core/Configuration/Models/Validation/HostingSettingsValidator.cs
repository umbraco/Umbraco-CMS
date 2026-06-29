// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Factories;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration represented as <see cref="HostingSettings" />.
/// </summary>
public class HostingSettingsValidator
    : ConfigurationValidatorBase, IValidateOptions<HostingSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, HostingSettings options)
    {
        // WEBSITE_INSTANCE_ID is an environment variable, not a config value, so it cannot be validated here.
        // MachineInfoFactory.GetMachineIdentifier() carries its own runtime length guard for that path.
        var baseName = string.IsNullOrWhiteSpace(options.MachineIdentifier)
            ? Environment.MachineName
            : options.MachineIdentifier;

        var identifier = MachineInfoFactory.BuildMachineIdentifier(baseName, options.SiteName);

        if (identifier.Length > MachineInfoFactory.MaxMachineIdentifierLength)
        {
            var settingHint = string.IsNullOrWhiteSpace(options.MachineIdentifier)
                ? $"'{Constants.Configuration.ConfigHostingPrefix}{nameof(HostingSettings.SiteName)}'"
                : $"'{Constants.Configuration.ConfigHostingPrefix}{nameof(HostingSettings.MachineIdentifier)}' or '{Constants.Configuration.ConfigHostingPrefix}{nameof(HostingSettings.SiteName)}'";

            return ValidateOptionsResult.Fail(
                $"The combined machine identifier '{identifier}' ({identifier.Length} characters) exceeds the maximum allowed length of {MachineInfoFactory.MaxMachineIdentifierLength} characters. " +
                $"Please shorten the value of {settingHint}.");
        }

        return ValidateOptionsResult.Success;
    }
}
