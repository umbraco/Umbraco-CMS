namespace Umbraco.Cms.Infrastructure.Migrations;

public class EFCoreMigrationContext : IEFCoreMigrationContext
{
    public EFCoreMigrationContext(MigrationPlan plan) => Plan = plan;

    public MigrationPlan Plan { get; }
}
