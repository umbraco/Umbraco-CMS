namespace Umbraco.Cms.Infrastructure.Migrations;

public abstract class EfCoreMigrationBase
{
    public IEFCoreMigrationContext MigrationContext { get; }

    public EfCoreMigrationBase(IEFCoreMigrationContext migrationContext) => MigrationContext = migrationContext;

    /// <summary>
    /// Executes the migration.
    /// </summary>
    protected abstract void Migrate();

    /// <summary>
    /// Runs the migration.
    /// </summary>
    public void Run() => Migrate();
}
