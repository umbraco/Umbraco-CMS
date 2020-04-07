using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_7_0
{
    public class ColorPickerPreValues : MigrationBase
    {
        private static readonly Regex OldPreValuesPattern1 = new Regex("\\s*{(\\s*\"[0-9]+\"\\s*:\\s*\"[0-9a-fA-F]+\"\\s*,)*\\s*\"useLabel\"\\s*:\\s*\"[01]\"\\s*}\\s*", RegexOptions.Compiled);
        private static readonly Regex OldPreValuesPattern2 = new Regex("\\s*{(\\s*\"[0-9]+\"\\s*:\\s*{\\s*\"value\"\\s*:\\s*\"[0-9a-fA-F]+\"\\s*(,\\s*\"label\"\\s*:\\s*\"[^\"]*\"\\s*)?(,\\s*\"sortOrder\"\\s*:\\s*[0-9]+\\s*)?}\\s*,)*\\s*\"useLabel\"\\s*:\\s*\"[01]\"\\s*}\\s*", RegexOptions.Compiled);

        public ColorPickerPreValues(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            var sql = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(d => d.EditorAlias == Constants.PropertyEditors.Aliases.ColorPicker);

            var dtos = Database.Fetch<DataTypeDto>(sql);

            foreach (var dto in dtos)
            {
                if (dto.Configuration.IsNullOrWhiteSpace()) continue;

                if (OldPreValuesPattern1.IsMatch(dto.Configuration)) ConvertPreValues(dto, ConvertStyle1);
                else if (OldPreValuesPattern2.IsMatch(dto.Configuration)) ConvertPreValues(dto, ConvertStyle2);
                else continue;

                Database.Update(dto);
            }
        }

        private void ConvertPreValues(DataTypeDto dto, Func<int, JToken, ItemValue> converter)
        {
            var obj = JObject.Parse(dto.Configuration);
            var config = new ColorPickerConfiguration();
            var id = 0;

            foreach (var prop in obj.Properties())
            {
                if (prop.Name.ToLowerInvariant() == "uselabel")
                {
                    config.UseLabel = prop.Value.ToString() == "1";
                }
                else
                {
                    id++;
                    config.Items.Add(new ValueListConfiguration.ValueListItem
                    {
                        Id = id,
                        Value = JsonConvert.SerializeObject(converter(id, prop.Value))
                    });
                }
            }

            dto.Configuration = JsonConvert.SerializeObject(config);
        }

        private ItemValue ConvertStyle1(int index, JToken token)
        {
            var value = token.ToString();
            return new ItemValue
            {
                Color = value,
                Label = value,
                SortOrder = index
            };
        }

        private ItemValue ConvertStyle2(int index, JToken token)
        {
            var obj = (JObject)token;
            var value = obj["value"].ToString();
            var label = obj["label"]?.ToString();
            var order = obj["sortOrder"]?.ToString();

            return new ItemValue
            {
                Color = value,
                Label = label.IsNullOrWhiteSpace() ? value : label,
                SortOrder = int.TryParse(order, out var o) ? o : index
            };
        }

        private class ItemValue
        {
            [JsonProperty("value")]
            public string Color { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }

            [JsonProperty("sortOrder")]
            public int SortOrder { get; set; }
        }
    }
}
