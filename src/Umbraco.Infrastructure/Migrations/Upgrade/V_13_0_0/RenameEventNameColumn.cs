using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

public class RenameEventNameColumn : MigrationBase
{
    public RenameEventNameColumn(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate() =>
        Rename
            .Column("eventName")
            .OnTable(Constants.DatabaseSchema.Tables.WebhookLog)
            .To("eventAlias")
            .Do();
}
