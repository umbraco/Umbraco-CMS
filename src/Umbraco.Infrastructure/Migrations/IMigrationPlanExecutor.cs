using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Migrations;

public interface IMigrationPlanExecutor
{
    /// <summary>
    /// Executes the migration plan.
    /// </summary>
    /// <param name="plan">The migration plan to execute.</param>
    /// <param name="fromState">The state to start execution at.</param>
    /// <returns><see cref="ExecutedMigrationPlan"/> containing information about the plan execution, such as completion state and the steps that ran.</returns>
    /// <remarks>
    /// <para>Each migration in the plan, may or may not run in a scope depending on the type of plan.</para>
    /// <para>A plan can complete partially, the changes of each completed migration will be saved.</para>
    /// </remarks>
    [Obsolete("Use ExecutePlanAsync instead. Scheduled for removal in Umbraco 18.")]
    ExecutedMigrationPlan ExecutePlan(MigrationPlan plan, string fromState);

    /// <summary>
    /// Executes the migration plan asynchronously.
    /// </summary>
    /// <param name="plan">The migration plan to execute.</param>
    /// <param name="fromState">The state to start execution at.</param>
    /// <returns>A Task of <see cref="ExecutedMigrationPlan"/> containing information about the plan execution, such as completion state and the steps that ran.</returns>
    /// <remarks>
    /// <para>Each migration in the plan, may or may not run in a scope depending on the type of plan.</para>
    /// <para>A plan can complete partially, the changes of each completed migration will be saved.</para>
    /// </remarks>
    Task<ExecutedMigrationPlan> ExecutePlanAsync(MigrationPlan plan, string fromState);
}
