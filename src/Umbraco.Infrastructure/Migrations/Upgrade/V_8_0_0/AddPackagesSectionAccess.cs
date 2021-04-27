namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0
{
    public class AddPackagesSectionAccess : MigrationBase
    {
        public AddPackagesSectionAccess(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // Any user group which had access to the Developer section should have access to Packages
            Database.Execute($@"
                insert into {Cms.Core.Constants.DatabaseSchema.Tables.UserGroup2App}
                select userGroupId, '{Cms.Core.Constants.Applications.Packages}'
                from {Cms.Core.Constants.DatabaseSchema.Tables.UserGroup2App}
                where app='developer'");
        }
    }
}
