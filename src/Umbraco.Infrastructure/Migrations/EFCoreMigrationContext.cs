namespace Umbraco.Cms.Infrastructure.Migrations;

public class EFCoreMigrationContext : IEFCoreMigrationContext
{
    public EFCoreMigrationContext(EFCoreMigrationPlan plan) => Plan = plan;

    public EFCoreMigrationPlan Plan { get; }
}
