using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

[Obsolete("Remove in Umbraco 18.")]
public class MigrateUserGroup2PermissionPermissionColumnType : MigrationBase
{
    public MigrateUserGroup2PermissionPermissionColumnType(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        // We don't need to update the column for SQLite since it just uses TEXT.
        if (DatabaseType == DatabaseType.SQLite)
        {
            return;
        }

        // The action column is part of the primary key, so we must drop the constraint before we can alter the column.
        AlterColumn<UserGroup2PermissionDto>(Constants.DatabaseSchema.Tables.UserGroup2Permission, "permission");
    }
}
