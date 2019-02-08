using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class RadioButtonPropertyEditorsMigration : MigrationBase
    {
        public RadioButtonPropertyEditorsMigration(IMigrationContext context)
            : base(context)
        {
        }

        public override void Migrate()
        {
            //need to convert the old drop down data types to use the new one
            var dataTypes = Database.Fetch<DataTypeDto>(Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(x => x.EditorAlias == "Umbraco.RadioButtonList"));

            foreach (var dataType in dataTypes)
            {
                ValueListConfiguration config;

                if (!dataType.Configuration.IsNullOrWhiteSpace())
                {
                    // parse configuration, and update everything accordingly
                    try
                    {
                        config = (ValueListConfiguration) new ValueListConfigurationEditor().FromDatabase(
                            dataType.Configuration);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error<DropDownPropertyEditorsMigration>(
                            ex,
                            "Invalid drop down configuration detected: \"{Configuration}\", cannot convert editor, values will be cleared",
                            dataType.Configuration);

                        continue;
                    }

                    // get property data dtos
                    var propertyDataDtos = Database.Fetch<PropertyDataDto>(Sql()
                        .Select<PropertyDataDto>()
                        .From<PropertyDataDto>()
                        .InnerJoin<PropertyTypeDto>()
                        .On<PropertyTypeDto, PropertyDataDto>((pt, pd) => pt.Id == pd.PropertyTypeId)
                        .InnerJoin<DataTypeDto>()
                        .On<DataTypeDto, PropertyTypeDto>((dt, pt) => dt.NodeId == pt.DataTypeId)
                        .Where<PropertyTypeDto>(x => x.DataTypeId == dataType.NodeId));

                    // update dtos
                    var updatedDtos = propertyDataDtos.Where(x => UpdatePropertyDataDto(x, config));

                    // persist changes
                    foreach (var propertyDataDto in updatedDtos) Database.Update(propertyDataDto);
                }
            }
        }

        private bool UpdatePropertyDataDto(PropertyDataDto propData, ValueListConfiguration config)
        {
            //Get the INT ids stored for this property/drop down
            int[] ids = null;
            if (propData.IntegerValue.HasValue) ids = new[] {propData.IntegerValue.Value};

            //if there are INT ids, convert them to values based on the configuration
            if (ids == null || ids.Length <= 0) return false;

            //map the ids to values
            var values = new List<string>();
            var canConvert = true;

            foreach (var id in ids)
            {
                var val = config.Items.FirstOrDefault(x => x.Id == id);
                if (val != null)
                    values.Add(val.Value);
                else
                {
                    Logger.Warn<DropDownPropertyEditorsMigration>(
                        "Could not find associated data type configuration for stored Id {DataTypeId}", id);
                    canConvert = false;
                }
            }

            if (!canConvert) return false;

            propData.VarcharValue = string.Join(",", values);
            propData.TextValue = null;
            propData.IntegerValue = null;
            return true;
        }

        private class ValueListConfigurationEditor : ConfigurationEditor<ValueListConfiguration>
        {
        }
    }
}
