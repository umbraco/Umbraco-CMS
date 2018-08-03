using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSevenZero
{
    /// <summary>
    /// Ensures the built-in user groups have the blueprint permission by default on upgrade
    /// </summary>
    [Migration("7.7.0", 6, Constants.System.UmbracoMigrationName)]
    public class EnsureContentTemplatePermissions : MigrationBase
    {
        public EnsureContentTemplatePermissions(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Execute.Code(database =>
            {
                var userGroups = database.Fetch<UserGroupDto>(
                    new Sql().Select("*")
                        .From<UserGroupDto>(SqlSyntax)
                        .Where<UserGroupDto>(x => x.Alias == "admin" || x.Alias == "editor", SqlSyntax));

                var localContext = new LocalMigrationContext(Context.CurrentDatabaseProvider, database, SqlSyntax, Logger);

                foreach (var userGroup in userGroups)
                {
                    if (userGroup.DefaultPermissions.Contains('ï') == false)
                    {
                        userGroup.DefaultPermissions += "ï";
                        localContext.Update.Table("umbracoUserGroup")
                            .Set(new { userGroupDefaultPermissions = userGroup.DefaultPermissions })
                            .Where(new { id = userGroup.Id });
                    }
                }

                return localContext.GetSql();
            });
            
        }

        public override void Down()
        {            
        }
    }
}