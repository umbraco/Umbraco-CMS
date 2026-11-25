namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Specifies how the runtime handles migrations at boot when the migration state stored in the database differs from the final state of a migration plan.
/// </summary>
/// <remarks>
/// <para>
/// Applies to the Umbraco migration plan, the premigration plan and package migration plans.
/// Migrations triggered manually, for example from the backoffice, are not affected.
/// </para>
/// <para>
/// A plan is behind its final state when its stored state is missing or one of its own states.
/// A plan is at an unknown state when its stored state is not one of its own states, typically because a newer version sharing the database has migrated it further, for example during a rolling upgrade.
/// A plan that ignores its stored state always runs from its initial state, so it is never at an unknown state.
/// </para>
/// </remarks>
[Flags]
public enum MigrationBehavior
{
    /// <summary>
    /// A plan behind its final state is not upgraded and is logged as a warning, so the application boots against the database as it is.
    /// A plan at an unknown state cannot be migrated, so the boot fails.
    /// </summary>
    None = 0,

    /// <summary>
    /// A plan at an unknown state is logged as a warning and the application boots against the database as it is, instead of failing the boot.
    /// </summary>
    /// <remarks>
    /// Only use this when the running version is compatible with the database schema as it is.
    /// An unknown state can also come from a database older than the lowest version this release can upgrade from.
    /// </remarks>
    IgnoreUnknown = 1,

    /// <summary>
    /// A plan behind its final state is upgraded, either unattended or from the backoffice depending on <see cref="UnattendedSettings.UpgradeUnattended" /> and <see cref="UnattendedSettings.PackageMigrationsUnattended" />.
    /// </summary>
    Upgrade = 2,

    /// <summary>
    /// A plan behind its final state is upgraded, and a plan at an unknown state is logged as a warning instead of failing the boot.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended for when several versions share the same database for a while, for example during a rolling upgrade with deployment slot swaps or with multiple environments connected to one database.
    /// The newer version upgrades the database, while an older version keeps booting against the newer database state.
    /// </para>
    /// <para>
    /// An older version only runs correctly against a newer database state when the migrations in between are forward-compatible: they add to the schema and data, but don't rename, remove or reshape anything the older version reads.
    /// Otherwise the older version boots, but fails at runtime when it queries what has changed.
    /// Upgrades between major versions can migrate both the schema and the data itself, so they should not be deployed as a rolling upgrade, and the downtime of stopping the older version first is the accepted trade-off.
    /// </para>
    /// <para>
    /// This also boots against a database older than the lowest version this release can upgrade from, which is never compatible.
    /// </para>
    /// </remarks>
    RollingUpgrade = Upgrade | IgnoreUnknown,
}
