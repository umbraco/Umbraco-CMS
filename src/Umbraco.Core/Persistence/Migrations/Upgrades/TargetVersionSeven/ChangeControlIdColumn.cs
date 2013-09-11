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
                            //We cannot find a map for this property editor so we're going to make it a label. This is because:
                            // * we want the upgrade to continue
                            // * we don't want any data loss
                            // * developers can change the property editor for the data type at a later time when there's a compatible one
                            // * editors cannot edit the value with an invalid editor

                            Update.Table("cmsDataType").Set(new { propertyEditorAlias = Constants.PropertyEditors.NoEditAlias }).Where(new { item.pk });
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