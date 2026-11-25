namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Specifies which migrations the runtime performs at boot when the migration state stored in the database differs from the final state of a migration plan.
/// </summary>
/// <remarks>
/// Applies to the Umbraco migration plan, the premigration plan and package migration plans.
/// Migrations triggered manually, for example from the backoffice, are not affected.
/// </remarks>
public enum MigrationBehavior
{
    /// <summary>
    /// No migrations are performed at boot, and the application boots against the database as it is.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A plan behind its final state is logged as a warning.
    /// A plan at a state the running version does not know - typically written by a newer version sharing the database, for example during a rolling upgrade - is logged as an error.
    /// A plan that always runs from its initial state, whatever state is stored, is never at an unknown state, so it is logged as a warning.
    /// </para>
    /// <para>
    /// Only use this when the running version is compatible with the database schema as it is.
    /// An unknown state can also come from a database older than the lowest version this release can upgrade from.
    /// </para>
    /// </remarks>
    None = 0,

    /// <summary>
    /// A plan behind its final state is upgraded, either unattended or from the backoffice depending on <see cref="UnattendedSettings.UpgradeUnattended" /> and <see cref="UnattendedSettings.PackageMigrationsUnattended" />.
    /// A plan at a state the running version does not know cannot be migrated, so the boot fails.
    /// A plan that always runs from its initial state, whatever state is stored, is upgraded from that initial state instead.
    /// </summary>
    Upgrade = 1,
}
