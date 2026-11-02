using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Factories;

/// <summary>
///     Returns the machine identifier configured via <see cref="HostingSettings.MachineIdentifier" />.
/// </summary>
public class ConfiguredMachineIdentityProvider : IMachineIdentityProvider
{
    private readonly IOptions<HostingSettings> _hostingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfiguredMachineIdentityProvider" /> class.
    /// </summary>
    public ConfiguredMachineIdentityProvider(IOptions<HostingSettings> hostingSettings)
        => _hostingSettings = hostingSettings;

    /// <inheritdoc />
    public string? GetMachineIdentifier()
    {
        var configured = _hostingSettings.Value.MachineIdentifier;
        return string.IsNullOrWhiteSpace(configured) ? null : configured;
    }
}
