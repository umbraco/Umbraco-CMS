using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_2_0;

[Obsolete("This is not used anymore and will be removed in Umbraco 13")]
public class AddDefaultForNotificationsToggle : MigrationBase
{
    public AddDefaultForNotificationsToggle(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        Sql<ISqlContext> updateSQL =
            Sql(
                $"UPDATE {Constants.DatabaseSchema.Tables.UserGroup} SET userGroupDefaultPermissions = userGroupDefaultPermissions + 'N' WHERE userGroupAlias IN ('admin', 'writer', 'editor')");
        Execute.Sql(updateSQL.SQL).Do();
    }
}
