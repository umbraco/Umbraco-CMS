using System.Diagnostics;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Core.Factories;

internal sealed class MachineInfoFactory : IMachineInfoFactory
{
    private readonly IHostingEnvironment _hostingEnvironment;

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
