using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_7_0
{
    /// <summary>
    /// Ensures that the filter string has the correct case for content types
    /// references, and converts string boolean values to JSON boolean values
    /// </summary>
    public class MultiNodeTreePickerPreValueMigrator : MigrationBase
    {
        public MultiNodeTreePickerPreValueMigrator(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            var sql = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(d => d.EditorAlias == Constants.PropertyEditors.Aliases.MultiNodeTreePicker);

            var dtos = Database.Fetch<DataTypeDto>(sql);

            sql = Sql()
                .Select<ContentTypeDto>()
                .From<ContentTypeDto>();
            var cts = Database.Fetch<ContentTypeDto>(sql).Select(c => c.Alias).ToList();
            var changes = false;

            foreach (var dto in dtos)
            {
                if (!Migrate(dto, cts)) continue;

                changes = true;
                Database.Update(dto);
            }

            // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
            // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
            if (changes)
                Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        // Convert the case of the Filter string to the correct case for the document type aliases
        // Convert boolean values from "1" or "0" to true and false
        private bool Migrate(DataTypeDto dto, List<string> contentTypeAliases)
        {
            if (dto.Configuration.IsNullOrWhiteSpace() || dto.Configuration[0] != '{') return false;

            var config = JObject.Parse(dto.Configuration);
            var changes = false;

            if (config.TryGetValue("filter", out var filterToken) && filterToken.Type == JTokenType.String && filterToken?.ToString() is string filter)
            {
                var values = new List<string>();

                foreach (var contentType in filter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var match = contentTypeAliases.FirstOrDefault(a => string.Equals(a, contentType, StringComparison.InvariantCultureIgnoreCase));
                    if (!match.IsNullOrWhiteSpace())
                        values.Add(match);
                }

                var vals = string.Join(",", values);
                if (vals != filter)
                {
                    changes = true;
                    config["filter"] = vals;
                }
            }

            if (config.TryGetValue("ignoreUserStartNodes", out var ignoreToken) && ignoreToken.Type == JTokenType.String)
            {
                changes = true;
                config["ignoreUserStartNodes"] = ignoreToken.ToString() == "1";
            }

            if (config.TryGetValue("showOpenButton", out var showToken) && showToken.Type == JTokenType.String)
            {
                changes = true;
                config["showOpenButton"] = showToken.ToString() == "1";
            }

            if (changes)
            {
                dto.Configuration = config.ToString();
            }

            return changes;
        }
    }
}
