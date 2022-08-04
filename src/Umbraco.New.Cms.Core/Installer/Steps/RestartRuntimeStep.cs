using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Installer.Steps;

public class RestartRuntimeStep : IInstallStep
{
    private readonly IRuntime _runtime;

    public RestartRuntimeStep(IRuntime runtime) => _runtime = runtime;

    public async Task ExecuteAsync(InstallData model) => await _runtime.RestartAsync();

    public Task<bool> RequiresExecutionAsync(InstallData model) => Task.FromResult(true);
}
