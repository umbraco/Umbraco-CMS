using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Migrations;

public interface IMigrationPlanExecutor
{
    ExecutedMigrationPlan Execute(MigrationPlan plan, string fromState);
}
