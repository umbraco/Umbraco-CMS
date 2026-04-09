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
        var identifier = BuildMachineIdentifier(Environment.MachineName, _hostingSettings.Value.SiteName);

        if (identifier.Length > MaxMachineIdentifierLength)
        {
            throw new InvalidOperationException(
                $"The combined machine identifier '{identifier}' ({identifier.Length} characters) exceeds the maximum allowed length of {MaxMachineIdentifierLength} characters. " +
                $"Please shorten the value of '{Constants.Configuration.ConfigHostingPrefix}SiteName'.");
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
