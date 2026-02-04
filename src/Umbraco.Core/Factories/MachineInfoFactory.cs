using System.Diagnostics;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Core.Factories;

/// <summary>
/// Default implementation of <see cref="IMachineInfoFactory"/> that fetches information about the host machine.
/// </summary>
internal sealed class MachineInfoFactory : IMachineInfoFactory
{
    private readonly IHostingEnvironment _hostingEnvironment;

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineInfoFactory"/> class.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    public MachineInfoFactory(IHostingEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    /// <inheritdoc />
    public string GetMachineIdentifier() => Environment.MachineName;

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
