// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Factories;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration represented as <see cref="HostingSettings" />.
/// </summary>
/// <remarks>
///     This is a best-effort, config-time check on the machine identifier length. It only covers the two
///     config-supplied inputs — <see cref="HostingSettings.MachineIdentifier" /> and
///     <see cref="HostingSettings.SiteName" /> — and assumes the default provider order in which an explicit
///     <see cref="HostingSettings.MachineIdentifier" /> takes priority over <c>Environment.MachineName</c>.
///     It intentionally does NOT run the <see cref="Umbraco.Cms.Core.Factories.IMachineIdentityProvider" /> chain,
///     so it does not cover the base name produced by the Azure <c>WEBSITE_INSTANCE_ID</c> provider (an environment
///     variable, not config) or by any custom provider, nor a reordered or replaced provider collection. Those
///     cases are caught only by the runtime length guard in
///     <see cref="Umbraco.Cms.Core.Factories.IMachineInfoFactory.GetMachineIdentifier" />, which throws on boot.
/// </remarks>
public class HostingSettingsValidator
    : ConfigurationValidatorBase, IValidateOptions<HostingSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, HostingSettings options)
    {
        // See the class remarks: this validates only the config-supplied inputs and mirrors the default provider
        // order (explicit MachineIdentifier over MachineName). It does not run the IMachineIdentityProvider chain,
        // so provider-derived base names (e.g. WEBSITE_INSTANCE_ID, custom providers) rely on the runtime guard.
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
                $"The machine identifier '{identifier}' ({identifier.Length} characters) exceeds the maximum allowed length of {MachineInfoFactory.MaxMachineIdentifierLength} characters. " +
                $"Please shorten the value of {settingHint}.");
        }

        return ValidateOptionsResult.Success;
    }
}
