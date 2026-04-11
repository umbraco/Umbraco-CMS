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
        var identifier = MachineInfoFactory.BuildMachineIdentifier(Environment.MachineName, options.SiteName);

        if (identifier.Length > MachineInfoFactory.MaxMachineIdentifierLength)
        {
            return ValidateOptionsResult.Fail(
                $"The combined machine identifier '{identifier}' ({identifier.Length} characters) exceeds the maximum allowed length of {MachineInfoFactory.MaxMachineIdentifierLength} characters. " +
                $"Please shorten the value of '{Constants.Configuration.ConfigHostingPrefix}{nameof(HostingSettings.SiteName)}'.");
        }

        return ValidateOptionsResult.Success;
    }
}
