using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Drops the <c>userGroupDefaultPermissions</c> column from the user group table. Permissions have been stored
/// in <c>umbracoUserGroup2Permission</c> since Umbraco 14, so the column has been unused since then.
/// </summary>
public class RemoveUserGroupDefaultPermissionsColumn : AsyncMigrationBase
{
    private const string ColumnName = "userGroupDefaultPermissions";

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveUserGroupDefaultPermissionsColumn"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public RemoveUserGroupDefaultPermissionsColumn(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        if (ColumnExists(UserGroupDto.TableName, ColumnName))
        {
            Delete.Column(ColumnName).FromTable(UserGroupDto.TableName).Do();
        }

        return Task.CompletedTask;
    }
}
