using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

[Obsolete("This is not used anymore and will be removed in Umbraco 13")]
public class AddPackagesSectionAccess : MigrationBase
{
    public AddPackagesSectionAccess(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() =>

        // Any user group which had access to the Developer section should have access to Packages
        Database.Execute($@"
                insert into {Constants.DatabaseSchema.Tables.UserGroup2App}
                select userGroupId, '{Constants.Applications.Packages}'
                from {Constants.DatabaseSchema.Tables.UserGroup2App}
                where app='developer'");
}
