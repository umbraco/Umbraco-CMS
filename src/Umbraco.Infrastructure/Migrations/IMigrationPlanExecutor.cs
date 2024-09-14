using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Migrations;

public interface IMigrationPlanExecutor
{
    Task<ExecutedMigrationPlan> ExecutePlanAsync(MigrationPlan plan, string fromState);
}
