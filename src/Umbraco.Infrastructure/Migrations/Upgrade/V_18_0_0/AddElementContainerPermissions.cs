using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

/// <summary>
/// Migration that adds dedicated element container permissions to the admin user group.
/// </summary>
public class AddElementContainerPermissions : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddElementContainerPermissions"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddElementContainerPermissions(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        // Check if admin group already has any element container permissions
        Sql<ISqlContext> existingPermissionsSql = Database.SqlContext.Sql()
            .Select<UserGroup2PermissionDto>()
            .From<UserGroup2PermissionDto>()
            .Where<UserGroup2PermissionDto>(x =>
                x.UserGroupKey == Constants.Security.AdminGroupKey &&
                x.Permission == ActionElementContainerBrowse.ActionLetter);

        if (Database.Fetch<UserGroup2PermissionDto>(existingPermissionsSql).Count != 0)
        {
            return Task.CompletedTask;
        }

        // Add all element container permissions for admin group
        var elementContainerPermissions = new[]
        {
            ActionElementContainerNew.ActionLetter,
            ActionElementContainerUpdate.ActionLetter,
            ActionElementContainerDelete.ActionLetter,
            ActionElementContainerMove.ActionLetter,
            ActionElementContainerBrowse.ActionLetter,
        };

        UserGroup2PermissionDto[] permissionDtos = elementContainerPermissions
            .Select(permission => new UserGroup2PermissionDto
            {
                UserGroupKey = Constants.Security.AdminGroupKey,
                Permission = permission,
            })
            .ToArray();

        Database.InsertBulk(permissionDtos);
        return Task.CompletedTask;
    }
}
