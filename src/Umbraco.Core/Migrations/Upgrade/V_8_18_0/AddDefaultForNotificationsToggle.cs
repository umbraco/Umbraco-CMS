namespace Umbraco.Core.Migrations.Upgrade.V_8_18_0
{
    public class AddDefaultForNotificationsToggle : MigrationBase
    {
        public AddDefaultForNotificationsToggle(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            var updateSQL = Sql($"UPDATE {Constants.DatabaseSchema.Tables.UserGroup} SET userGroupDefaultPermissions = userGroupDefaultPermissions + 'N' WHERE userGroupAlias IN ('admin', 'writer', 'editor')");
            Execute.Sql(updateSQL.SQL).Do();
        }
    }
}
