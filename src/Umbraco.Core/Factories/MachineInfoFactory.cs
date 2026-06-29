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

    internal const string AzureWebsiteInstanceIdEnvironmentVariable = "WEBSITE_INSTANCE_ID";

    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IOptions<HostingSettings> _hostingSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineInfoFactory"/> class.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    /// <param name="hostingSettings">The hosting settings.</param>
    public MachineInfoFactory(IHostingEnvironment hostingEnvironment, IOptions<HostingSettings> hostingSettings)
    {
        _hostingEnvironment = hostingEnvironment;
        _hostingSettings = hostingSettings;
    }

    internal static string BuildMachineIdentifier(string machineName, string? siteName)
    {
        if (string.IsNullOrWhiteSpace(siteName))
        {
            return machineName;
        }

        return $"{machineName}/{siteName}";
    }

    /// <inheritdoc />
    public string GetMachineIdentifier()
    {
        var explicitMachineIdentifier = _hostingSettings.Value.MachineIdentifier;

        // Resolve the base name: explicit config → Azure instance ID (stable across container recycles) → OS hostname.
        string baseName;
        if (string.IsNullOrWhiteSpace(explicitMachineIdentifier) is false)
        {
            baseName = explicitMachineIdentifier;
        }
        else
        {
            var instanceId = Environment.GetEnvironmentVariable(AzureWebsiteInstanceIdEnvironmentVariable);
            baseName = string.IsNullOrWhiteSpace(instanceId) ? Environment.MachineName : instanceId;
        }

        var identifier = BuildMachineIdentifier(baseName, _hostingSettings.Value.SiteName);

        if (identifier.Length > MaxMachineIdentifierLength)
        {
            var settingHint = string.IsNullOrWhiteSpace(explicitMachineIdentifier)
                ? $"'{Constants.Configuration.ConfigHostingPrefix}{nameof(HostingSettings.SiteName)}'"
                : $"'{Constants.Configuration.ConfigHostingPrefix}{nameof(HostingSettings.MachineIdentifier)}' or '{Constants.Configuration.ConfigHostingPrefix}{nameof(HostingSettings.SiteName)}'";

            throw new InvalidOperationException(
                $"The combined machine identifier '{identifier}' ({identifier.Length} characters) exceeds the maximum allowed length of {MaxMachineIdentifierLength} characters. " +
                $"Please shorten the value of {settingHint}.");
        }

        return identifier;
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
