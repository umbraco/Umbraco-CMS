using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class DropDownPropertyEditorsMigration : MigrationBase
    {
        public DropDownPropertyEditorsMigration(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            //need to convert the old drop down data types to use the new one
            var oldDropDowns = Database.Fetch<DataTypeDto>(Sql().Select<DataTypeDto>().Where<DataTypeDto>(x => x.EditorAlias.Contains(".DropDown")));
            foreach(var dd in oldDropDowns)
            {
                //nothing to change if there is no config
                if (dd.Configuration.IsNullOrWhiteSpace()) continue;

                ValueListConfiguration config;
                try
                {
                     config = JsonConvert.DeserializeObject<ValueListConfiguration>(dd.Configuration);
                }
                catch (Exception ex)
                {
                    Logger.Error<DropDownPropertyEditorsMigration>(ex, $"Invalid drop down configuration detected: \"{dd.Configuration}\", cannot convert editor, values will be cleared");
                    dd.Configuration = null;
                    Database.Update(dd);
                    continue;
                }

                var propDataSql = Sql().Select("*").From<PropertyDataDto>()
                    .InnerJoin<PropertyTypeDto>().On<PropertyTypeDto, PropertyDataDto>(x => x.Id, x => x.PropertyTypeId)
                    .InnerJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(x => x.NodeId, x => x.DataTypeId)
                    .Where<DataTypeDto>(x => x.EditorAlias == dd.EditorAlias);

                var propDatas = Database.Query<PropertyDataDto>(propDataSql);
                var toUpdate = new List<PropertyDataDto>();
                foreach (var propData in propDatas)
                {
                    if (UpdatePropertyDataDto(propData, config))
                    {
                        //update later, we are iterating all values right now so no SQL can be run inside of this iteration (i.e. Query<T>)
                        toUpdate.Add(propData);
                    }
                }

                //run the property data updates
                foreach(var propData in toUpdate)
                {
                    Database.Update(propData);
                }

                var requiresCacheRebuild = false;
                switch (dd.EditorAlias)
                {
                    case "Umbraco.DropDown":
                        UpdateDataType(dd, config, false);
                        break;
                    case "Umbraco.DropdownlistPublishingKeys":
                        UpdateDataType(dd, config, false);
                        requiresCacheRebuild = true;
                        break;
                    case "Umbraco.DropDownMultiple":
                        UpdateDataType(dd, config, false);
                        break;
                    case "Umbraco.DropdownlistMultiplePublishKeys":
                        UpdateDataType(dd, config, true);
                        requiresCacheRebuild = true;
                        break;
                }

                if (requiresCacheRebuild)
                {
                    //TODO: How to force rebuild the cache?
                }
            }
        }

        private void UpdateDataType(DataTypeDto dataType, ValueListConfiguration config, bool isMultiple)
        {
            dataType.EditorAlias = Constants.PropertyEditors.Aliases.DropDownListFlexible;
            var flexConfig = new
            {
                multiple = isMultiple,
                items = config.Items
            };
            dataType.Configuration = JsonConvert.SerializeObject(flexConfig);
        }

        private bool UpdatePropertyDataDto(PropertyDataDto propData, ValueListConfiguration config)
        {
            //Get the INT ids stored for this property/drop down
            IEnumerable<int> ids = null;
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
            if (ids != null)
            {
                //map the ids to values
                var vals = new List<string>();
                foreach (var id in ids)
                {
                    var val = config.Items.FirstOrDefault(x => x.Id == id);
                    if (val != null)
                        vals.Add(val.Value);
                }

                propData.VarcharValue = string.Join(",", vals);
                propData.TextValue = null;
                propData.IntegerValue = null;
                return true;
            }

            return false;
        }

        private IEnumerable<int> ConvertStringValues(string val)
        {
            return val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.TryParse(x, out var i) ? i : int.MinValue)
                        .Where(x => x != int.MinValue);
        }

    }
}
