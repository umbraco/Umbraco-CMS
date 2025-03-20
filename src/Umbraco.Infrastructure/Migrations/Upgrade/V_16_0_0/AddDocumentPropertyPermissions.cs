using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_0_0;

[Obsolete("Remove in Umbraco 18.")]
internal class AddDocumentPropertyPermissions : MigrationBase
{
    public AddDocumentPropertyPermissions(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        List<UserGroupDto>? userGroups = Database.Fetch<UserGroupDto>();

        foreach (UserGroupDto userGroupDto in userGroups ?? [])
        {
            List<UserGroup2PermissionDto>? currentPermissions = Database.Fetch<UserGroup2PermissionDto>(
                "WHERE userGroupKey = @UserGroupKey",
                new { UserGroupKey = userGroupDto.Key });

            if (currentPermissions is null || currentPermissions.Count is 0)
            {
                continue;
            }

            if (currentPermissions.Any(p => p.Permission == ActionDocumentPropertyRead.ActionLetter))
            {
                return;
            }

            Database.InsertBulk(
            [
                new UserGroup2PermissionDto
                {
                    UserGroupKey = userGroupDto.Key, Permission = ActionDocumentPropertyRead.ActionLetter
                },
                new UserGroup2PermissionDto
                {
                    UserGroupKey = userGroupDto.Key, Permission = ActionDocumentPropertyWrite.ActionLetter
                }
            ]);
        }
    }
}
