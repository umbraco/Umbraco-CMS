using System.Linq;
using Umbraco.Core.Migrations.Upgrade.V_8_0_0.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_8_0
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
            //get UmbracoTags datatypes that need to be migrated
            var sqlDataTypes = Sql()
               .Select<DataTypeDto>()
               .From<DataTypeDto>()
               .Where<DataTypeDto>(x => x.EditorAlias == Constants.PropertyEditors.Aliases.Tags);
            var dataTypes = Database.Fetch<DataTypeDto>(sqlDataTypes).ToList();

            foreach (var dataType in dataTypes)
            {
                //update the storage type, moving data from VarcharValue to TextValue for Umbraco.Tags controls
                var propertyDataDtos = Database.Fetch<PropertyDataDto>(Sql()
                .Select<PropertyDataDto>()
                .From<PropertyDataDto>()
                .InnerJoin<PropertyTypeDto>().On<PropertyTypeDto, PropertyDataDto>((pt, pd) => pt.Id == pd.PropertyTypeId)
                .InnerJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>((dt, pt) => dt.NodeId == pt.DataTypeId)
                .Where<PropertyTypeDto>(x => x.DataTypeId == dataType.NodeId));

                foreach (var propertyDataDto in propertyDataDtos)
                {
                    propertyDataDto.TextValue = propertyDataDto.VarcharValue;
                    propertyDataDto.VarcharValue = null;
                    Database.Update(propertyDataDto);
                }

                //change DbType to Ntext
                dataType.DbType = ValueStorageType.Ntext.ToString();
                Database.Update(dataType);

            }

        }
    }
}
