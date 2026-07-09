namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

/// <summary>
/// Provides functionality to track and manage the migration of local links during the upgrade process to Umbraco version 15.0.0.
/// </summary>
public class LocalLinkMigrationTracker
{
    /// <summary>
    /// Gets a value indicating whether the fixed migration has been run.
    /// </summary>
    public bool HasFixedMigrationRun { get; private set; }

    /// <summary>
    /// Sets the flag indicating that the fixed migration has been executed.
    /// </summary>
    public void MarkFixedMigrationRan() => HasFixedMigrationRun = true;
}
