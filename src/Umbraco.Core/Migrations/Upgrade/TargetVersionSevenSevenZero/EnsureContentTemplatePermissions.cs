using System.Linq;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Migrations.Upgrade.TargetVersionSevenSevenZero
{
    /// <summary>
    /// Ensures the built-in user groups have the blueprint permission by default on upgrade
    /// </summary>
    [Migration("7.7.0", 5, Constants.System.UmbracoMigrationName)]
    public class EnsureContentTemplatePermissions : MigrationBase
    {
        public EnsureContentTemplatePermissions(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            Execute.Code(context =>
            {
                var database = context.Database;
                var userGroups = database.Fetch<UserGroupDto>(
                    Context.SqlContext.Sql().Select("*")
                        .From<UserGroupDto>()
                        .Where<UserGroupDto>(x => x.Alias == "admin" || x.Alias == "editor"));

                var local = Context.GetLocalMigration();

                foreach (var userGroup in userGroups)
                {
                    if (userGroup.DefaultPermissions.Contains('�') == false)
                    {
                        userGroup.DefaultPermissions += "�";
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
