using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixTwoZero
{
    [Migration("7.1.0", 2, GlobalSettings.UmbracoMigrationName)]
    [Migration("6.2.0", 2, GlobalSettings.UmbracoMigrationName)]
    public class ChangePasswordColumn : MigrationBase
    {
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