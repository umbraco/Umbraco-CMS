using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    /// <summary>
    /// Updates the data in the changed propertyEditorAlias column after it has been changed by ChangeControlIdColumn
    /// </summary>
    [Migration("7.0.0", 1, GlobalSettings.UmbracoMigrationName)]
    public class UpdateControlIdToPropertyEditorAlias : MigrationBase
    {
        public UpdateControlIdToPropertyEditorAlias(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //now that the controlId column is renamed and now a string we need to convert
            if (Context == null || Context.Database == null) return;

            //we need to get the data and create the migration scripts before we change the actual schema bits below!
            var list = Context.Database.Fetch<dynamic>("SELECT pk, controlId FROM cmsDataType");
            foreach (var item in list)
            {
                Guid legacyId = item.controlId;
                var alias = LegacyPropertyEditorIdToAliasConverter.GetAliasFromLegacyId(legacyId);
                if (alias != null)
                {
                    //check that the new property editor exists with that alias
                    var editor = PropertyEditorResolver.Current.GetByAlias(alias);
                    if (editor == null)
                    {
                        //We cannot find a map for this property editor so we're going to make it a label. This is because:
                        // * we want the upgrade to continue
                        // * we don't want any data loss
                        // * developers can change the property editor for the data type at a later time when there's a compatible one
                        // * editors cannot edit the value with an invalid editor

                        Update.Table("cmsDataType").Set(new { propertyEditorAlias = Constants.PropertyEditors.NoEditAlias }).Where(new { item.pk });
                    }
                    else
                    {
                        Update.Table("cmsDataType").Set(new { propertyEditorAlias = alias }).Where(new { item.pk });
                    }
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

        public override void Down()
        {
            throw new DataLossException("Cannot downgrade from a version 7 database to a prior version, the database schema has already been modified");
        }
    }
}