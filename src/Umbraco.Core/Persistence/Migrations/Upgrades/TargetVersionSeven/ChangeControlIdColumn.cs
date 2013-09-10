using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    //TODO: There's other migrations we need to run for v7 like:
    // * http://issues.umbraco.org/issue/U4-2664

    [Migration("7.0.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class ChangeControlIdColumn : MigrationBase
    {
        public override void Up()
        {
            Rename.Column("controlId").OnTable("cmsDataType").To("propertyEditorAlias");
            Alter.Column("controlId").OnTable("cmsDataType").AsString(255);

            //now that the controlId column is renamed and now a string we need to convert
            if (Context != null && Context.Database != null)
            {
                var list = Context.Database.Fetch<dynamic>("SELECT pk, propertyEditorAlias FROM cmsDataType");
                foreach (var item in list)
                {
                    Guid legacyId;
                    if (Guid.TryParse(item.propertyEditorAlias, out legacyId))
                    {
                        var alias = LegacyPropertyEditorIdToAliasConverter.GetAliasFromLegacyId(legacyId);
                        if (alias != null)
                        {
                            Update.Table("cmsDataType").Set(new {propertyEditorAlias = alias}).Where(new {item.pk});
                        }
                        else
                        {

                            /* what do do ?
                             *      throw an exception? -> I can actually ensure that all GUIDs in the table currently have a valid entry in the map before running the upgrade and if not then the upgrade will just fail?
                             *      ignore it, just leave the legacy GUID entry in there -> this will mean that the editor will not render for that property and there will probably be YSODs generated elsewhere.
                             *      delete the entry -> of course this could lead to data loss
                             *      change it to a text field -> no data loss and they are able to then change it to a different property editor later in the data type editor
                             */

                            //NOTE: I'm going to just set this to a text field, I think this is the 'safest' way without data loss and still allows an upgrade, still
                            // waiting on feedback from the core team.

                            Update.Table("cmsDataType").Set(new { propertyEditorAlias = Constants.PropertyEditors.TextboxAlias }).Where(new { item.pk });
                        }
                    }
                }
            }
        }

        public override void Down()
        {
            throw new NotSupportedException("Cannot downgrade from a version 7 database to a prior version");
        }
    }
}