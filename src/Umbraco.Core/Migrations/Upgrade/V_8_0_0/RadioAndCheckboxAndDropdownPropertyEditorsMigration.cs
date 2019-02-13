using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class RadioAndCheckboxAndDropdownPropertyEditorsMigration : MigrationBase
    {
        public RadioAndCheckboxAndDropdownPropertyEditorsMigration(IMigrationContext context)
            : base(context)
        {
        }

        public override void Migrate()
        {
            var refreshCache = false;

            refreshCache |= Migrate(Constants.PropertyEditors.Aliases.RadioButtonList, (dto, configuration) =>  UpdateRadioOrCheckboxPropertyDataDto(dto, configuration, true));
            refreshCache |= Migrate(Constants.PropertyEditors.Aliases.CheckBoxList, (dto, configuration) =>  UpdateRadioOrCheckboxPropertyDataDto(dto, configuration, false));
            refreshCache |= Migrate(Constants.PropertyEditors.Aliases.DropDownListFlexible, UpdateDropDownPropertyDataDto);

            if (refreshCache)
            {
                //FIXME: trigger cache rebuild. Currently the data in the database tables is wrong.
            }
        }

        private bool Migrate(string editorAlias, Func<PropertyDataDto, ValueListConfiguration, bool> updateRadioPropertyDataFunc)
        {
            var refreshCache = false;
            var dataTypes = GetDataTypes(editorAlias);

            foreach (var dataType in dataTypes)
            {
                ValueListConfiguration config;

                if (dataType.Configuration.IsNullOrWhiteSpace())
                    continue;

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
                        "Invalid property editor configuration detected: \"{Configuration}\", cannot convert editor, values will be cleared",
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
                var updatedDtos = propertyDataDtos.Where(x => updateRadioPropertyDataFunc(x, config));

                // persist changes
                foreach (var propertyDataDto in updatedDtos) Database.Update(propertyDataDto);

                UpdateDataType(dataType);
                refreshCache = true;
            }

            return refreshCache;
        }

        private List<DataTypeDto> GetDataTypes(string editorAlias)
        {
            //need to convert the old drop down data types to use the new one
            var dataTypes = Database.Fetch<DataTypeDto>(Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(x => x.EditorAlias == editorAlias));
            return dataTypes;
        }

        private void UpdateDataType(DataTypeDto dataType)
        {
            dataType.DbType = ValueStorageType.Nvarchar.ToString();
            Database.Update(dataType);
        }

        private bool UpdateRadioOrCheckboxPropertyDataDto(PropertyDataDto propData, ValueListConfiguration config, bool singleValue)
        {
            //Get the INT ids stored for this property/drop down
            int[] ids = null;
            if (!propData.VarcharValue.IsNullOrWhiteSpace())
            {
                ids = ConvertStringValues(propData.VarcharValue);
            }
            else if (!propData.TextValue.IsNullOrWhiteSpace())
            {
                ids = ConvertStringValues(propData.TextValue);
            }
            else if (propData.IntegerValue.HasValue)
            {
                ids = new[] {propData.IntegerValue.Value};
            }

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

            //The radio button only supports selecting a single value, so if there are multiple for some insane reason we can only use the first
            propData.VarcharValue = singleValue ? values[0] : JsonConvert.SerializeObject(values);
            propData.TextValue = null;
            propData.IntegerValue = null;
            return true;
        }

        private bool UpdateDropDownPropertyDataDto(PropertyDataDto propData, ValueListConfiguration config)
        {
            //Get the INT ids stored for this property/drop down
            var values = propData.VarcharValue.Split(new []{","}, StringSplitOptions.RemoveEmptyEntries);

            //if there are INT ids, convert them to values based on the configuration
            if (values == null || values.Length <= 0) return false;

            //The radio button only supports selecting a single value, so if there are multiple for some insane reason we can only use the first
            propData.VarcharValue = JsonConvert.SerializeObject(values);
            propData.TextValue = null;
            propData.IntegerValue = null;
            return true;
        }

        private int[] ConvertStringValues(string val)
        {
            var splitVals = val.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

            var intVals = splitVals
                .Select(x => int.TryParse(x, out var i) ? i : int.MinValue)
                .Where(x => x != int.MinValue)
                .ToArray();

            //only return if the number of values are the same (i.e. All INTs)
            if (splitVals.Length == intVals.Length)
                return intVals;

            return null;
        }

        private class ValueListConfigurationEditor : ConfigurationEditor<ValueListConfiguration>
        {
        }
    }
}
