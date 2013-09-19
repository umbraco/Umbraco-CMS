using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    /// <summary>
    /// We are removing the cmsMacroPropertyType which the cmsMacroProperty references and the cmsMacroProperty.macroPropertyType column
    /// needs to be changed to editorAlias. Then running a data migration to populate the editorAlias column based on the data in the cmsMacroPropertyType
    /// table. 
    /// </summary>
    [Migration("7.0.0", 6, GlobalSettings.UmbracoMigrationName)]
    public class UpdateCmsMacroPropertyEditorAliasFromCmsMacroPropertyType : MigrationBase
    {
        public override void Up()
        {
            //now that the controlId column is renamed and now a string we need to convert
            if (Context == null || Context.Database == null) return;

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

                //update the table with the alias, the current editorAlias will contain the original id
                Update.Table("cmsMacroProperty").Set(new { editorAlias = alias }).Where(new { editorAlias = item.id });

            }
        }

        public override void Down()
        {
            throw new NotSupportedException("Cannot downgrade from a version 7 database to a prior version");
        }
    }
}