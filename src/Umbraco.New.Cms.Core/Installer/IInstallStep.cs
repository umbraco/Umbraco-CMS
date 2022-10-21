using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Installer;

/// <summary>
/// Defines a step that's required to install Umbraco.
/// </summary>
public interface IInstallStep
{
    /// <summary>
    /// Executes the install step.
    /// </summary>
    /// <param name="model">InstallData model containing the data provided by the installer UI.</param>
    /// <returns></returns>
    Task ExecuteAsync(InstallData model);

    /// <summary>
    /// Determines if the step is required to execute.
    /// </summary>
    /// <param name="model">InstallData model containing the data provided by the installer UI.</param>
    /// <returns>True if the step should execute, otherwise false.</returns>
    Task<bool> RequiresExecutionAsync(InstallData model);
}
