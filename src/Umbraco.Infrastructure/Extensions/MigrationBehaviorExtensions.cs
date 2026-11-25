using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for applying a <see cref="MigrationBehavior" /> to a migration plan at boot.
/// </summary>
internal static class MigrationBehaviorExtensions
{
    /// <summary>
    /// Determines whether a plan that is not at its final state should be migrated at boot.
    /// </summary>
    /// <param name="behavior">The configured migration behavior.</param>
    /// <param name="plan">The migration plan.</param>
    /// <param name="state">The state persisted in the database for the plan, which differs from its final state.</param>
    /// <param name="logger">The logger used to report a plan that is not migrated.</param>
    /// <returns>
    ///   <c>true</c> if the plan should be migrated; <c>false</c> if it should be treated as up to date.
    /// </returns>
    public static bool ShouldMigrate(this MigrationBehavior behavior, MigrationPlan plan, string? state, ILogger logger)
    {
        if (behavior.HasFlag(MigrationBehavior.Upgrade))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(state) || plan.IgnoreCurrentState || plan.IsKnownState(state))
        {
            logger.LogWarning(
                "Migration plan {PlanName} is at state {DatabaseState}, behind its final state {FinalState}. "
                + "It is not migrated because {SettingName} is set to {Behavior}.",
                plan.Name,
                string.IsNullOrWhiteSpace(state) ? "<none>" : state,
                plan.FinalState,
                nameof(RuntimeSettings.MigrationBehavior),
                behavior);
        }
        else
        {
            logger.LogError(
                "Migration plan {PlanName} is at state {DatabaseState}, which this version does not know. "
                + "It is not migrated because {SettingName} is set to {Behavior}; this is expected while a newer version shares the database.",
                plan.Name,
                state,
                nameof(RuntimeSettings.MigrationBehavior),
                behavior);
        }

        return false;
    }
}
