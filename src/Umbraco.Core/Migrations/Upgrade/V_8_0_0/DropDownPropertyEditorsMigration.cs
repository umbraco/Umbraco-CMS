using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class DropDownPropertyEditorsMigration : MigrationBase
    {
        private readonly CacheRefresherCollection _cacheRefreshers;
        private readonly IServerMessenger _serverMessenger;

        public DropDownPropertyEditorsMigration(IMigrationContext context, CacheRefresherCollection cacheRefreshers, IServerMessenger serverMessenger)
            : base(context)
        {
            _cacheRefreshers = cacheRefreshers;
            _serverMessenger = serverMessenger;
        }

        // dummy editor for deserialization
        private class ValueListConfigurationEditor : ConfigurationEditor<ValueListConfiguration>
        { }

        public override void Migrate()
        {
            //need to convert the old drop down data types to use the new one
            var dataTypes = Database.Fetch<DataTypeDto>(Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(x => x.EditorAlias.Contains(".DropDown")));

            foreach (var dataType in dataTypes)
            {
                ValueListConfiguration config;

                if (!dataType.Configuration.IsNullOrWhiteSpace())
                {
                    // parse configuration, and update everything accordingly
                    try
                    {
                        config = (ValueListConfiguration) new ValueListConfigurationEditor().FromDatabase(dataType.Configuration);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error<DropDownPropertyEditorsMigration>(
                            ex, "Invalid drop down configuration detected: \"{Configuration}\", cannot convert editor, values will be cleared",
                            dataType.Configuration);

                        // reset
                        config = new ValueListConfiguration();
                    }

                    // get property data dtos
                    var propertyDataDtos = Database.Fetch<PropertyDataDto>(Sql()
                        .Select<PropertyDataDto>()
                        .From<PropertyDataDto>()
                        .InnerJoin<PropertyTypeDto>().On<PropertyTypeDto, PropertyDataDto>((pt, pd) => pt.Id == pd.PropertyTypeId)
                        .InnerJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>((dt, pt) => dt.NodeId == pt.DataTypeId)
                        .Where<PropertyTypeDto>(x => x.DataTypeId == dataType.NodeId));

                    // update dtos
                    var updatedDtos = propertyDataDtos.Where(x => UpdatePropertyDataDto(x, config));

                    // persist changes
                    foreach (var propertyDataDto in updatedDtos)
                        Database.Update(propertyDataDto);
                }
                else
                {
                    // default configuration
                    config = new ValueListConfiguration();
                }

                var requiresCacheRebuild = false;
                switch (dataType.EditorAlias)
                {
                    case string ea when ea.InvariantEquals("Umbraco.DropDown"):
                        UpdateDataType(dataType, config, false);
                        break;
                    case string ea when ea.InvariantEquals("Umbraco.DropdownlistPublishingKeys"):
                        UpdateDataType(dataType, config, false);
                        requiresCacheRebuild = true;
                        break;
                    case string ea when ea.InvariantEquals("Umbraco.DropDownMultiple"):
                        UpdateDataType(dataType, config, true);
                        break;
                    case string ea when ea.InvariantEquals("Umbraco.DropdownlistMultiplePublishKeys"):
                        UpdateDataType(dataType, config, true);
                        requiresCacheRebuild = true;
                        break;
                }

                if (requiresCacheRebuild)
                {
                    var dataTypeCacheRefresher = _cacheRefreshers[Guid.Parse("35B16C25-A17E-45D7-BC8F-EDAB1DCC28D2")];
                    _serverMessenger.PerformRefreshAll(dataTypeCacheRefresher);
                }
            }
        }

        private void UpdateDataType(DataTypeDto dataType, ValueListConfiguration config, bool isMultiple)
        {
            dataType.EditorAlias = Constants.PropertyEditors.Aliases.DropDownListFlexible;
            dataType.DbType = ValueStorageType.Nvarchar.ToString();

            var flexConfig = new DropDownFlexibleConfiguration
            {
                Items = config.Items,
                Multiple = isMultiple
            };
            dataType.Configuration = ConfigurationEditor.ToDatabase(flexConfig);

            Database.Update(dataType);
        }

        private bool UpdatePropertyDataDto(PropertyDataDto propData, ValueListConfiguration config)
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
                ids = new[] { propData.IntegerValue.Value };
            }

            //if there are INT ids, convert them to values based on the configured pre-values
            if (ids != null && ids.Length > 0)
            {
                //map the ids to values
                var vals = new List<string>();
                var canConvert = true;
                foreach (var id in ids)
                {
                    var val = config.Items.FirstOrDefault(x => x.Id == id);
                    if (val != null)
                        vals.Add(val.Value);
                    else
                    {
                        Logger.Warn<DropDownPropertyEditorsMigration>(
                            "Could not find associated data type configuration for stored Id {DataTypeId}", id);
                        canConvert = false;
                    }
                }
                if (canConvert)
                {
                    propData.VarcharValue = string.Join(",", vals);
                    propData.TextValue = null;
                    propData.IntegerValue = null;
                    return true;
                }
            }

            return false;
        }

        private int[] ConvertStringValues(string val)
        {
            var splitVals = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            var intVals = splitVals
                            .Select(x => int.TryParse(x, out var i) ? i : int.MinValue)
                            .Where(x => x != int.MinValue)
                            .ToArray();

            //only return if the number of values are the same (i.e. All INTs)
            if (splitVals.Length == intVals.Length)
                return intVals;

            return null;
        }
    }
}
