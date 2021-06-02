using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public abstract class PropertyEditorsMigrationBase : MigrationBase
    {
        protected PropertyEditorsMigrationBase(IMigrationContext context)
            : base(context)
        { }

        internal List<DataTypeDto> GetDataTypes(string editorAlias, bool strict = true)
        {
            var sql = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>();

            sql = strict
                ? sql.Where<DataTypeDto>(x => x.EditorAlias == editorAlias)
                : sql.Where<DataTypeDto>(x => x.EditorAlias.Contains(editorAlias));

            return Database.Fetch<DataTypeDto>(sql);
        }

        protected int[] ConvertStringValues(string val)
        {
            var splitVals = val.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);

            var intVals = splitVals
                .Select(x => int.TryParse(x, out var i) ? i : int.MinValue)
                .Where(x => x != int.MinValue)
                .ToArray();

            //only return if the number of values are the same (i.e. All INTs)
            if (splitVals.Length == intVals.Length)
                return intVals;

            return null;
        }

        internal bool UpdatePropertyDataDto(PropertyDataDto propData, ValueListConfiguration config, bool isMultiple)
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

            // if there are INT ids, convert them to values based on the configuration
            if (ids == null || ids.Length <= 0) return false;

            // map ids to values
            var values = new List<string>();
            var canConvert = true;

            foreach (var id in ids)
            {
                var val = config.Items.FirstOrDefault(x => x.Id == id);
                if (val != null)
                {
                    values.Add(val.Value);
                    continue;
                }

                Logger.Warn(GetType(), "Could not find PropertyData {PropertyDataId} value '{PropertyValue}' in the datatype configuration: {Values}.",
                    propData.Id, id, string.Join(", ", config.Items.Select(x => x.Id + ":" + x.Value)));
                canConvert = false;
            }

            if (!canConvert) return false;

            propData.VarcharValue = isMultiple ? JsonConvert.SerializeObject(values) : values[0];
            propData.TextValue = null;
            propData.IntegerValue = null;
            return true;
        }

        // dummy editor for deserialization
        protected class ValueListConfigurationEditor : ConfigurationEditor<ValueListConfiguration>
        { }
    }
}
