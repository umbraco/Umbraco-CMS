using System.Xml.Linq;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class MigrateCharPermissionsToStrings : MigrationBase
{


    public MigrateCharPermissionsToStrings(IMigrationContext context)
        : base(context)
    {

    }

    protected override void Migrate()
    {
        var userGroups = Database.Fetch<UserGroupDto>();

        foreach (UserGroupDto userGroupDto in userGroups)
        {
            if (userGroupDto.DefaultPermissions == null)
            {
                continue;
            }

            var permissions = userGroupDto.DefaultPermissions.Select(x => new UserGroup2PermissionDto()
            {
                Permission = x.ToString(), UserGroupKey = userGroupDto.Key
            }).ToHashSet();

            Database.InsertBulk(permissions);

            userGroupDto.DefaultPermissions = null;

            Database.Update(userGroupDto);
        }
    }
}
