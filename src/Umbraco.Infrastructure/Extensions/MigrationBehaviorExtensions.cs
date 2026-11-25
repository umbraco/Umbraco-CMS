using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for applying a <see cref="MigrationBehavior" /> to a migration plan at boot.
/// </summary>
internal static class MigrationBehaviorExtensions
{
    private const string SettingName = Constants.Configuration.ConfigRuntime + ":" + nameof(RuntimeSettings.MigrationBehavior);

    /// <summary>
    /// Determines whether a plan that is not at its final state should be migrated at boot.
    /// </summary>
    /// <param name="behavior">The configured migration behavior.</param>
    /// <param name="plan">The migration plan.</param>
    /// <param name="state">The state persisted in the database for the plan, which differs from its final state.</param>
    /// <param name="logger">The logger used to report a plan that is not migrated, or <c>null</c> to not report it.</param>
    /// <returns>
    ///   <c>true</c> if the plan should be migrated; <c>false</c> if it should be treated as up to date.
    /// </returns>
    /// <remarks>
    /// A plan at an unknown state that is not ignored is reported as needing migration, so the migration fails the boot with the unknown state as it would without this setting.
    /// </remarks>
    public static bool ShouldMigrate(this MigrationBehavior behavior, MigrationPlan plan, string? state, ILogger? logger)
    {
        if (IsUnknownState(plan, state))
        {
            if (behavior.HasFlag(MigrationBehavior.IgnoreUnknown) is false)
            {
                return true;
            }

            logger?.LogWarning(
                "Migration plan {PlanName} is at state {DatabaseState}, which this version does not know. "
                + "It is not migrated because {SettingName} is set to {Behavior}; this is expected while a newer version shares the database.",
                plan.Name,
                state,
                SettingName,
                behavior);

            return false;
        }

        if (behavior.HasFlag(MigrationBehavior.Upgrade))
        {
            return true;
        }

        logger?.LogWarning(
            "Migration plan {PlanName} is at state {DatabaseState}, behind its final state {FinalState}. "
            + "It is not migrated because {SettingName} is set to {Behavior}, so this version runs against a database it has not migrated, "
            + "and anything that depends on the pending migrations can fail until the plan is migrated. "
            + "Expected only while another instance migrates the database; otherwise include {UpgradeBehavior} in the setting and restart.",
            plan.Name,
            string.IsNullOrWhiteSpace(state) ? "<none>" : state,
            plan.FinalState,
            SettingName,
            behavior,
            MigrationBehavior.Upgrade);

        return false;
    }

    // A plan that ignores its current state runs from its initial state, so a state it doesn't know is only behind.
    private static bool IsUnknownState(MigrationPlan plan, string? state)
        => string.IsNullOrWhiteSpace(state) is false
            && plan.IgnoreCurrentState is false
            && plan.IsKnownState(state) is false;
}
