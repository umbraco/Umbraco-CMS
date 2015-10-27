using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixTwoZero
{
    [Migration("7.1.0", 2, GlobalSettings.UmbracoMigrationName)]
    [Migration("6.2.0", 2, GlobalSettings.UmbracoMigrationName)]
    public class ChangePasswordColumn : MigrationBase
    {
        public ChangePasswordColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //up to 500 chars
            Alter.Table("umbracoUser").AlterColumn("userPassword").AsString(500).NotNullable();
        }

        public override void Down()
        {
            //back to 125 chars
            Alter.Table("umbracoUser").AlterColumn("userPassword").AsString(125).NotNullable();
        }
    }
}