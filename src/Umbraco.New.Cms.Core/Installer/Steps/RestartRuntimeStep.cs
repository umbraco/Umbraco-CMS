using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Installer.Steps;

public class RestartRuntimeStep : IInstallStep, IUpgradeStep
{
    private readonly IRuntime _runtime;

    public RestartRuntimeStep(IRuntime runtime) => _runtime = runtime;

    public async Task ExecuteAsync(InstallData _) => await Execute();

    public async Task ExecuteAsync() => await Execute();

    private async Task Execute() => await _runtime.RestartAsync();

    public Task<bool> RequiresExecutionAsync(InstallData _) => ShouldExecute();

    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private Task<bool> ShouldExecute() => Task.FromResult(true);
}
