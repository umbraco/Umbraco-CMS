using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    /// <summary>
    /// We are removing the cmsMacroPropertyType which the cmsMacroProperty references and the cmsMacroProperty.macroPropertyType column
    /// needs to be changed to editorAlias, we'll do this by removing the constraint,changing the macroPropertyType to the new 
    /// editorAlias column (and maintaing data so we can reference it)
    /// </summary>
    [Migration("7.0.0", 5, GlobalSettings.UmbracoMigrationName)]
    public class AlterCmsMacroPropertyTable : MigrationBase
    {
        public override void Up()
        {
            //"DF_cmsMacroProperty_macroPropertyHidden""
            Delete.DefaultConstraint().OnTable("cmsMacroProperty").OnColumn("macroPropertyHidden");
            
            Delete.Column("macroPropertyHidden").FromTable("cmsMacroProperty");

            Delete.ForeignKey("FK_cmsMacroProperty_cmsMacroPropertyType_id").OnTable("cmsMacroProperty");

            //change the type (keep the data)
            Alter.Table("cmsMacroProperty").AlterColumn("macroPropertyType").AsString(255);
            //rename the column
            Rename.Column("macroPropertyType").OnTable("cmsMacroProperty").To("editorAlias");
            
        }

        public override void Down()
        {
            throw new NotSupportedException("Cannot downgrade from a version 7 database to a prior version");
        }
    }
}