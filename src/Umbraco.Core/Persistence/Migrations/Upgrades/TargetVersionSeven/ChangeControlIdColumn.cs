using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class ChangeControlIdColumn : MigrationBase
    {
        public override void Up()
        {
            Rename.Column("controlId").OnTable("cmsDataType").To("propertyEditorAlias");
            Alter.Column("controlId").OnTable("cmsDataType").AsString(255);
        }

        public override void Down()
        {
            throw new NotSupportedException("Cannot downgrade from a version 7 database to a prior version");
        }
    }
}