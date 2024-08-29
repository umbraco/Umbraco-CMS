using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Installer.Steps;

public class RestartRuntimeStep : StepBase, IInstallStep, IUpgradeStep
{
    private readonly IRuntime _runtime;

    public RestartRuntimeStep(IRuntime runtime) => _runtime = runtime;

    public async Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => await Execute();

    public async Task<Attempt<InstallationResult>> ExecuteAsync() => await Execute();

    private async Task<Attempt<InstallationResult>> Execute()
    {
        await _runtime.RestartAsync();
        return Success();
    }

    public Task<bool> RequiresExecutionAsync(InstallData _) => ShouldExecute();

    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private Task<bool> ShouldExecute() => Task.FromResult(true);
}
