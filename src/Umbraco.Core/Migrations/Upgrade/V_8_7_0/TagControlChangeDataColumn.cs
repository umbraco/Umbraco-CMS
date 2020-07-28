using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_7_0
{
    public class TagControlChangeDataColumn : MigrationBase
    {
        public TagControlChangeDataColumn(IMigrationContext context)
            : base(context)
        {

        }

        /// <summary>
        /// Changes the configured storage column <c>dbType</c> in <c>umbracoDataType</c> to nText where the <c>Umbraco.Tags</c> property editor is used.
        /// Migrates data in <c>umbracoPropertyData</c> from varcharValue to textValue for affected properties. 
        /// </summary>
        public override void Migrate()
        {
            //update the storage type of Umbraco.Tags controls
            Execute.Sql("UPDATE umbracoDataType SET dbType = 'Ntext' WHERE propertyEditorAlias = 'Umbraco.Tags'").Do();

            //move data from varcharValue to textValue (tested that you can null a column used as a data source in sql later in the update)
            Execute.Sql("UPDATE umbracoPropertyData SET textValue = varcharValue, varcharValue = null WHERE propertyTypeId IN (select id FROM cmsPropertyType WHERE dataTypeId IN (SELECT nodeid FROM umbracoDataType WHERE propertyEditorAlias = 'Umbraco.Tags')) AND textValue IS NULL AND varcharValue IS NOT NULL").Do();
        }
    }
}
