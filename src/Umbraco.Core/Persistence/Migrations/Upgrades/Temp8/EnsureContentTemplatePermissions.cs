using System.Linq;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.Temp8
{
    /// <summary>
    /// Ensures the built-in user groups have the blueprint permission by default on upgrade
    /// </summary>
    [Migration("8.0.0", 6, Constants.System.UmbracoMigrationName)]
    public class EnsureContentTemplatePermissions : MigrationBase
    {
        public EnsureContentTemplatePermissions(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            Execute.Code(database =>
            {
                var userGroups = database.Fetch<UserGroupDto>(
                    Context.Sql().Select("*")
                        .From<UserGroupDto>()
                        .Where<UserGroupDto>(x => x.Alias == "admin" || x.Alias == "editor"));

                var local = Context.GetLocalMigration();

                foreach (var userGroup in userGroups)
                {
                    if (userGroup.DefaultPermissions.Contains('ï') == false)
                    {
                        userGroup.DefaultPermissions += "ï";
                        local.Update.Table("umbracoUserGroup")
                            .Set(new { userGroupDefaultPermissions = userGroup.DefaultPermissions })
                            .Where(new { id = userGroup.Id });
                    }
                }

                return local.GetSql();
            });
        }
    }
}