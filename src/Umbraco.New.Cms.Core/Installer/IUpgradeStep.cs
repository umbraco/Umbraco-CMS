namespace Umbraco.New.Cms.Core.Installer;

/// <summary>
/// Defines a step that's required to upgrade Umbraco.
/// </summary>
public interface IUpgradeStep
{
    /// <summary>
    /// Executes the upgrade step.
    /// </summary>
    Task ExecuteAsync();

    /// <summary>
    /// Determines if the step is required to execute.
    /// </summary>
    /// <returns>True if the step should execute, otherwise false.</returns>
    Task<bool> RequiresExecutionAsync();
}
