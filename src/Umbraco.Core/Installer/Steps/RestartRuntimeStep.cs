using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Installer.Steps;

/// <summary>
/// An installation and upgrade step that restarts the Umbraco runtime.
/// </summary>
public class RestartRuntimeStep : StepBase, IInstallStep, IUpgradeStep
{
    private readonly IRuntime _runtime;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestartRuntimeStep"/> class.
    /// </summary>
    /// <param name="runtime">The runtime service used to restart Umbraco.</param>
    public RestartRuntimeStep(IRuntime runtime) => _runtime = runtime;

    /// <inheritdoc />
    public async Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => await Execute();

    /// <inheritdoc />
    public async Task<Attempt<InstallationResult>> ExecuteAsync() => await Execute();

    /// <summary>
    /// Executes the runtime restart operation.
    /// </summary>
    /// <returns>A task containing an attempt with the installation result.</returns>
    private async Task<Attempt<InstallationResult>> Execute()
    {
        await _runtime.RestartAsync();
        return Success();
    }

    /// <inheritdoc />
    public Task<bool> RequiresExecutionAsync(InstallData _) => ShouldExecute();

    /// <inheritdoc />
    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    /// <summary>
    /// Determines whether this step should be executed.
    /// </summary>
    /// <returns>A task containing <c>true</c> if the step should execute; otherwise, <c>false</c>.</returns>
    private Task<bool> ShouldExecute() => Task.FromResult(true);
}
