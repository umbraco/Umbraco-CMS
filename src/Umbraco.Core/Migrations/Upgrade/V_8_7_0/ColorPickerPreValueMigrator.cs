using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_7_0
{
    /// <summary>
    /// Updates the pre-values on the Color Picker data types to preserve
    /// settings from v7
    /// </summary>
    public class ColorPickerPreValueMigrator : MigrationBase
    {
        private static readonly Regex OldPreValuesPattern = new Regex("\\s*{.*\"[0-9]+\"\\s*:\\s*(\"[0-9a-f]+\"|{[^}]*}).*}\\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ColorPickerPreValueMigrator(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            var sql = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(d => d.EditorAlias == Constants.PropertyEditors.Aliases.ColorPicker);

            var dtos = Database.Fetch<DataTypeDto>(sql);
            var changes = false;

            foreach (var dto in dtos)
            {
                var config = ConvertPreValues(dto.Configuration);
                if (config == dto.Configuration) continue;

                changes = true;
                dto.Configuration = config;
                Database.Update(dto);
            }

            // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
            // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
            if (changes)
                Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        private string ConvertPreValues(string configString)
        {
            if (configString.IsNullOrWhiteSpace() || !OldPreValuesPattern.IsMatch(configString)) return configString;

            var obj = JObject.Parse(configString);
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
                        Value = JsonConvert.SerializeObject(prop.Value is JObject val ? Convert(id, val) : Convert(id, prop.Value))
                    });
                }
            }

            return JsonConvert.SerializeObject(config);
        }

        private ItemValue Convert(int index, JToken token)
        {
            var value = token.ToString();
            return new ItemValue
            {
                Color = value,
                Label = value,
                SortOrder = index
            };
        }

        private ItemValue Convert(int index, JObject obj)
        {
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
