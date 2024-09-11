namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class DeleteMacroTables : MigrationBase
{
    public DeleteMacroTables(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (TableExists("cmsMacroProperty"))
        {
            Delete.Table("cmsMacroProperty").Do();
        }

        if (TableExists("cmsMacro"))
        {
            Delete.Table("cmsMacro").Do();
        }
    }
}
