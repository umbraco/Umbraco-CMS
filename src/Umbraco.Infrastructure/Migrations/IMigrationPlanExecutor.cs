using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Migrations;

public interface IMigrationPlanExecutor
{
    [Obsolete("Use ExecutePlan instead.")]
    string Execute(MigrationPlan plan, string fromState);

    ExecutedMigrationPlan ExecutePlan(MigrationPlan plan, string fromState)
    {
        var state = Execute(plan, fromState);

        // We have no real way of knowing whether it was successfull or not here, assume true.
        return new ExecutedMigrationPlan(plan, fromState, state, true, plan.Transitions.Select(x => x.Value).WhereNotNull().ToList());
    }
}
