using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

/// <summary>
/// Migration that adds dedicated element folder permissions to the admin user group.
/// </summary>
public class AddElementFolderPermissions : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddElementFolderPermissions"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddElementFolderPermissions(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override void Migrate()
    {
        // Check if admin group already has any element folder permissions
        Sql<ISqlContext> existingPermissionsSql = Database.SqlContext.Sql()
            .Select<UserGroup2PermissionDto>()
            .From<UserGroup2PermissionDto>()
            .Where<UserGroup2PermissionDto>(x =>
                x.UserGroupKey == Constants.Security.AdminGroupKey &&
                x.Permission == ActionElementFolderBrowse.ActionLetter);

        if (Database.Fetch<UserGroup2PermissionDto>(existingPermissionsSql).Count != 0)
        {
            return;
        }

        // Add all element folder permissions for admin group
        var elementFolderPermissions = new[]
        {
            ActionElementFolderNew.ActionLetter,
            ActionElementFolderUpdate.ActionLetter,
            ActionElementFolderDelete.ActionLetter,
            ActionElementFolderMove.ActionLetter,
            ActionElementFolderBrowse.ActionLetter,
        };

        UserGroup2PermissionDto[] permissionDtos = elementFolderPermissions
            .Select(permission => new UserGroup2PermissionDto
            {
                UserGroupKey = Constants.Security.AdminGroupKey,
                Permission = permission,
            })
            .ToArray();

        Database.InsertBulk(permissionDtos);
    }
}
