using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Migrations;

public interface IMigrationPlanExecutor
{
    ExecutedMigrationPlan ExecutePlan(MigrationPlan plan, string fromState);
}
