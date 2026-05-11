using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

/// <summary>
/// Represents a migration that updates the data type of the <c>Permission</c> column in the <c>UserGroup2Permission</c> table.
/// </summary>
[Obsolete("Remove in Umbraco 18.")]
public class MigrateUserGroup2PermissionPermissionColumnType : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateUserGroup2PermissionPermissionColumnType"/> class, used to migrate the column type of the permission in the UserGroup2Permission table during upgrade to version 14.0.0.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> for the migration.</param>
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
