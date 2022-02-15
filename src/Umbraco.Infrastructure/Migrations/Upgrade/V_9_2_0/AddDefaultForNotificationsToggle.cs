using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_2_0
{
    public class AddDefaultForNotificationsToggle : MigrationBase
    {
        public AddDefaultForNotificationsToggle(IMigrationContext context)
            : base(context)
        { }

        protected override void Migrate()
        {
            var updateSQL = Sql($"UPDATE {Constants.DatabaseSchema.Tables.UserGroup} SET userGroupDefaultPermissions = userGroupDefaultPermissions + 'N' WHERE userGroupAlias IN ('admin', 'writer', 'editor')");
            Execute.Sql(updateSQL.SQL).Do();
        }
    }
}
