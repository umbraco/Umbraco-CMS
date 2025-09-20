using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

public class RenameEventNameColumn : MigrationBase
{
    public RenameEventNameColumn(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        // This check is here because we renamed a column from 13-rc1 to 13-rc2, the previous migration adds the table
        // so if you are upgrading from 13-rc1 to 13-rc2 then this column will not exist.
        // If you are however upgrading from 12, then this column will exist, and thus there is no need to rename it.
        if (ColumnExists(Constants.DatabaseSchema.Tables.WebhookLog, "eventName") is false)
        {
            return;
        }

        Rename
            .Column("eventName")
            .OnTable(Constants.DatabaseSchema.Tables.WebhookLog)
            .To("eventAlias")
            .Do();
    }

}
