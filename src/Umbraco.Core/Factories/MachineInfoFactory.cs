using System.Diagnostics;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Core.Factories;

/// <summary>
/// Default implementation of <see cref="IMachineInfoFactory"/> that fetches information about the host machine.
/// </summary>
internal sealed class MachineInfoFactory : IMachineInfoFactory
{
    // machineId is stored as NVARCHAR(255) in umbracoLastSynced
    internal const int MaxMachineIdentifierLength = 255;

    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly MachineIdentityProviderCollection _providers;
    private readonly IOptions<HostingSettings> _hostingSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineInfoFactory"/> class.
    /// </summary>
    public MachineInfoFactory(
        IHostingEnvironment hostingEnvironment,
        MachineIdentityProviderCollection providers,
        IOptions<HostingSettings> hostingSettings)
    {
        _hostingEnvironment = hostingEnvironment;
        _providers = providers;
        _hostingSettings = hostingSettings;
    }

    /// <summary>Combines a machine name and an optional site name into a single machine identifier.</summary>
    internal static string BuildMachineIdentifier(string machineName, string? siteName)
        => string.IsNullOrWhiteSpace(siteName) ? machineName : $"{machineName}/{siteName}";

    /// <inheritdoc />
    public string GetMachineIdentifier()
    {
        (IMachineIdentityProvider provider, string? identifier) = _providers
            .Select(p => (Provider: p, Identifier: p.GetMachineIdentifier()))
            .FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Identifier) is false);

        if (provider is null)
        {
            throw new InvalidOperationException($"No {nameof(IMachineIdentityProvider)} returned a machine identifier.");
        }

        var siteName = _hostingSettings.Value.SiteName;
        var machineIdentifier = BuildMachineIdentifier(identifier!, siteName);

        if (machineIdentifier.Length > MaxMachineIdentifierLength)
        {
            var siteNameHint = string.IsNullOrWhiteSpace(siteName)
                ? "Shorten the base identifier at its source."
                : $"Shorten '{Constants.Configuration.ConfigHostingPrefix}{nameof(HostingSettings.SiteName)}' or the base identifier at its source.";

            throw new InvalidOperationException(
                $"The machine identifier '{machineIdentifier}' ({machineIdentifier.Length} characters) exceeds the maximum allowed length of {MaxMachineIdentifierLength} characters. " +
                $"The base identifier was provided by '{provider.GetType().Name}'. {siteNameHint}");
        }

        return machineIdentifier;
    }

    private string? _localIdentity;

    /// <inheritdoc />
    public string GetLocalIdentity()
    {
        if (_localIdentity is not null)
        {
            return _localIdentity;
        }

        using var process = Process.GetCurrentProcess();
        _localIdentity = Environment.MachineName // eg DOMAIN\SERVER
                         + "/" + _hostingEnvironment.ApplicationId // eg /LM/S3SVC/11/ROOT
                         + " [P" + process.Id // eg 1234
                         + "/D" + AppDomain.CurrentDomain.Id // eg 22
                         + "] " + Guid.NewGuid().ToString("N").ToUpper(); // make it truly unique

        return _localIdentity;
    }
}
