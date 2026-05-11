namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

/// <summary>
/// Migration step that removes macro-related tables from the database schema as part of the upgrade to version 14.0.0.
/// </summary>
public class DeleteMacroTables : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="DeleteMacroTables"/> using the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for the migration.</param>
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
