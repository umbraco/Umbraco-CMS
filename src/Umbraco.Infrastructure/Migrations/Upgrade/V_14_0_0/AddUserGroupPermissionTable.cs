using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class AddUserGroup2PermisionTable : MigrationBase
{
    public AddUserGroup2PermisionTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.UserGroup2Permission))
        {
            return;
        }

        Create.Table<UserGroup2PermissionDto>().Do();
    }
}
