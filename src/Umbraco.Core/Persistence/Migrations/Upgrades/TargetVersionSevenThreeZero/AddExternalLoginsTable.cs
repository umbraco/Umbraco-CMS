using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 9, GlobalSettings.UmbracoMigrationName)]
    public class AddExternalLoginsTable : MigrationBase
    {
        public AddExternalLoginsTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the table is already there
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("umbracoExternalLogin")) return;

            Create.Table("umbracoExternalLogin")
                .WithColumn("id").AsInt32().Identity().PrimaryKey("PK_umbracoExternalLogin")
                .WithColumn("userId").AsInt32().NotNullable().ForeignKey("FK_umbracoExternalLogin_umbracoUser_id", "umbracoUser", "id")
                .WithColumn("loginProvider").AsString(4000).NotNullable()
                .WithColumn("providerKey").AsString(4000).NotNullable()
                .WithColumn("createDate").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
        }
    }
}