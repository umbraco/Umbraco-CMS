using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixTwoZero
{
    [Migration("7.1.0", 3, GlobalSettings.UmbracoMigrationName)]
    [Migration("6.2.0", 3, GlobalSettings.UmbracoMigrationName)]
    public class AddChangeDocumentTypePermission : MigrationBase
    {
        public AddChangeDocumentTypePermission(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Execute.Code(AddChangeDocumentTypePermissionDo);
        }

        public override void Down()
        {
            Execute.Code(UndoChangeDocumentTypePermissionDo);
        }

        private static string AddChangeDocumentTypePermissionDo(Database database)
        {
            var adminUserType = database.Fetch<UserTypeDto>("WHERE Id = 1").FirstOrDefault();
            
            if (adminUserType != null)
            {
                if (adminUserType.DefaultPermissions.Contains("7") == false)
                {
                    adminUserType.DefaultPermissions = adminUserType.DefaultPermissions + "7";
                    database.Save(adminUserType);
                }
            }

            return string.Empty;
        }

        private static string UndoChangeDocumentTypePermissionDo(Database database)
        {
            var adminUserType = database.Fetch<UserTypeDto>("WHERE Id = 1").FirstOrDefault();
            
            if (adminUserType != null)
            {
                if (adminUserType.DefaultPermissions.Contains("7"))
                {
                    adminUserType.DefaultPermissions = adminUserType.DefaultPermissions.Replace("7", "");
                    database.Save(adminUserType);
                }
            }

            return string.Empty;
        }
    }
}