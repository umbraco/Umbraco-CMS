using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Installer.Steps;

public class RestartRuntimeStep : IInstallStep
{
    private readonly IRuntime _runtime;

    public RestartRuntimeStep(IRuntime runtime)
    {
        _runtime = runtime;
    }

    public InstallationType InstallationTypeTarget => InstallationType.NewInstall | InstallationType.Upgrade;

    public async Task ExecuteAsync(InstallData model) => await _runtime.RestartAsync();

    public Task<bool> RequiresExecutionAsync(InstallData model) => Task.FromResult(true);
}
