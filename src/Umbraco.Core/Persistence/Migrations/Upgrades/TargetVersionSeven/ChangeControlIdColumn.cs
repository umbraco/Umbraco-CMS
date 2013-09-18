using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    //TODO: There's other migrations we need to run for v7 like:
    // * http://issues.umbraco.org/issue/U4-2664

    [Migration("7.0.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class ChangeControlIdColumn : MigrationBase
    {
        public override void Up()
        {   
            Alter.Table("cmsDataType").AlterColumn("controlId").AsString(255);
            Rename.Column("controlId").OnTable("cmsDataType").To("propertyEditorAlias");
        }

        public override void Down()
        {
            throw new NotSupportedException("Cannot downgrade from a version 7 database to a prior version");
        }
    }
}