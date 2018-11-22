using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenNineZero
{
    [Migration("7.9.0", 2, Constants.System.UmbracoMigrationName)]
    public class CreateSensitiveDataUserGroup : MigrationBase
    {
        public CreateSensitiveDataUserGroup(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Execute.Code(database =>
            {
                //Don't exeucte if the group is already there
                var exists = database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoUserGroup WHERE userGroupAlias = @userGroupAlias",
                    new {userGroupAlias = Constants.Security.SensitiveDataGroupAlias });
                if (exists == 0)
                {
                    var resultId = database.Insert("umbracoUserGroup", "id", new UserGroupDto { StartMediaId = -1, StartContentId = -1, Alias = Constants.Security.SensitiveDataGroupAlias, Name = "Sensitive data", DefaultPermissions = "", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-lock" });
                    database.Insert(new User2UserGroupDto { UserGroupId = Convert.ToInt32(resultId), UserId = 0 }); //add admin to sensitive data
                }

                return string.Empty;
            });
        }

        public override void Down()
        {
        }
    }
}
