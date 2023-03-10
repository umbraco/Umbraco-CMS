namespace Umbraco.Cms.Infrastructure.Migrations;

public interface IEFCoreMigrationPlanExecutor
{
    ExecutedEFCoreMigrationPlan ExecutePlan(EFCoreMigrationPlan plan, string fromState);
}
