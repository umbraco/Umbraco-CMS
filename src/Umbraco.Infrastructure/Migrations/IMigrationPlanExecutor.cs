using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Migrations;

public interface IMigrationPlanExecutor
{
    [Obsolete("Use ExecutePlan instead.")]
    string Execute(MigrationPlan plan, string fromState);

    ExecutedMigrationPlan ExecutePlan(MigrationPlan plan, string fromState)
    {
        var state = Execute(plan, fromState);
        return new ExecutedMigrationPlan(plan, fromState, state);
    }
}
