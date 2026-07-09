using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Infrastructure.Install;

namespace Umbraco.Cms.Infrastructure.Installer.Steps;

/// <summary>
/// Represents the installation step that marks the installation process as complete.
/// </summary>
public class RegisterInstallCompleteStep : StepBase, IInstallStep, IUpgradeStep
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterInstallCompleteStep"/> class.
    /// </summary>
    /// <param name="installHelper">An <see cref="InstallHelper"/> instance used to assist with installation steps.</param>
    [Obsolete("Please use the constructor without parameters. Scheduled for removal in Umbraco 19.")]
    public RegisterInstallCompleteStep(InstallHelper installHelper)
        : this()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterInstallCompleteStep"/> class.
    /// This is the default constructor.
    /// </summary>
    public RegisterInstallCompleteStep()
    {
    }

    /// <summary>
    /// Executes the final installation step asynchronously, marking the installation as complete.
    /// </summary>
    /// <param name="_">The installation data; this parameter is not used.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the installation attempt.</returns>
    public Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => Execute();

    /// <summary>
    /// Asynchronously executes the final step of the installation process, registering that installation has completed.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{InstallationResult}"/> indicating the outcome of the installation completion step.</returns>
    public Task<Attempt<InstallationResult>> ExecuteAsync() => Execute();

    private Task<Attempt<InstallationResult>> Execute() => Task.FromResult(Success());

    /// <summary>
    /// Determines asynchronously whether this install step needs to be executed, based on the provided installation data.
    /// </summary>
    /// <param name="_">The installation data (unused).</param>
    /// <returns>A task that returns <c>true</c> if execution is required; otherwise, <c>false</c>.</returns>
    public Task<bool> RequiresExecutionAsync(InstallData _) => ShouldExecute();

    /// <summary>
    /// Determines asynchronously whether this installation step requires execution.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the step requires execution; otherwise, false.</returns>
    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private static Task<bool> ShouldExecute() => Task.FromResult(true);
}
