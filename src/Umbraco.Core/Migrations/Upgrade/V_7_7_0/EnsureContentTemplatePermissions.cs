using System.Linq;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_7_7_0
{
    /// <summary>
    /// Ensures the built-in user groups have the blueprint permission by default on upgrade
    /// </summary>
    public class EnsureContentTemplatePermissions : MigrationBase
    {
        public EnsureContentTemplatePermissions(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var database = Database;
            var userGroups = database.Fetch<UserGroupDto>(
                Context.SqlContext.Sql().Select("*")
                    .From<UserGroupDto>()
                    .Where<UserGroupDto>(x => x.Alias == "admin" || x.Alias == "editor"));

            foreach (var userGroup in userGroups)
            {
                if (userGroup.DefaultPermissions.Contains('�') == false)
                {
                    userGroup.DefaultPermissions += "�";
                    Update.Table("umbracoUserGroup")
                        .Set(new { userGroupDefaultPermissions = userGroup.DefaultPermissions })
                        .Where(new { id = userGroup.Id })
                        .Do();
                }
            }
        }
    }
}
