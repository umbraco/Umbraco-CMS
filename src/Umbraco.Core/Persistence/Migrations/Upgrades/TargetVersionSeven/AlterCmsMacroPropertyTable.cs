using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    /// <summary>
    /// We are removing the cmsMacroPropertyType which the cmsMacroProperty references and the cmsMacroProperty.macroPropertyType column
    /// needs to be changed to editorAlias, we'll do this by removing the constraint,changing the macroPropertyType to the new 
    /// editorAlias column (and maintaing data so we can reference it)
    /// </summary>
    [Migration("7.0.0", 6, GlobalSettings.UmbracoMigrationName)]
    public class AlterCmsMacroPropertyTable : MigrationBase
    {
        public override void Up()
        {
            //now that the controlId column is renamed and now a string we need to convert
            if (Context == null || Context.Database == null) return;

            //"DF_cmsMacroProperty_macroPropertyHidden""
            Delete.DefaultConstraint().OnTable("cmsMacroProperty").OnColumn("macroPropertyHidden");
            
            Delete.Column("macroPropertyHidden").FromTable("cmsMacroProperty");

            Delete.ForeignKey().FromTable("cmsMacroProperty").ForeignColumn("macroPropertyType").ToTable("cmsMacroPropertyType").PrimaryColumn("id");
            
            Alter.Table("cmsMacroProperty").AddColumn("editorAlias").AsString(255).NotNullable().WithDefaultValue("");

            //we need to get the data and create the migration scripts before we change the actual schema bits below!
            var list = Context.Database.Fetch<dynamic>("SELECT * FROM cmsMacroPropertyType");
            foreach (var item in list)
            {

                var alias = item.macroPropertyTypeAlias;
                //check if there's a map created 
                var newAlias = (string)LegacyParameterEditorAliasConverter.GetNewAliasFromLegacyAlias(alias);
                if (newAlias.IsNullOrWhiteSpace() == false)
                {
                    alias = newAlias;
                }

                //update the table with the alias, the current macroPropertyType will contain the original id
                Update.Table("cmsMacroProperty").Set(new { editorAlias = alias }).Where(new { macroPropertyType = item.id });
            }

            //drop the column now
            Delete.Column("macroPropertyType").FromTable("cmsMacroProperty");

            //drop the default constraing
            Delete.DefaultConstraint().OnTable("cmsMacroProperty").OnColumn("editorAlias");
        }

        public override void Down()
        {
            throw new DataLossException("Cannot downgrade from a version 7 database to a prior version, the database schema has already been modified");
        }
    }
}