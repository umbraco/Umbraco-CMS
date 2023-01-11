using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Migrations;

public interface IMigrationPlanExecutor
{
    [Obsolete("This will return an ExecutedMigrationPlan in V13")]
    string Execute(MigrationPlan plan, string fromState);
}
