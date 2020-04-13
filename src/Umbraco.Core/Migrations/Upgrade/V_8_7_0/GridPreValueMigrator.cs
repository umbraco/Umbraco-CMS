using Newtonsoft.Json.Linq;
using System.Linq;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_7_0
{
    /// <summary>
    /// Updates the pre-values on the Grid data type to preserve value
    /// settings from v7
    /// </summary>
    public class GridPreValueMigrator : MigrationBase
    {
        public GridPreValueMigrator(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            var sql = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(d => d.EditorAlias == Constants.PropertyEditors.Aliases.Grid);

            var dtos = Database.Fetch<DataTypeDto>(sql);
            var changes = false;

            foreach (var dto in dtos)
            {
                if (!Migrate(dto)) continue;

                changes = true;
                Database.Update(dto);
            }

            // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
            // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
            if (changes)
                Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        // Convert RTE code setting, and ignoreUserStartNode setting
        private bool Migrate(DataTypeDto dto)
        {
            if (dto.Configuration.IsNullOrWhiteSpace() || dto.Configuration[0] != '{') return false;
            var config = JObject.Parse(dto.Configuration);
            var changes = false;

            if (config.TryGetValue("ignoreUserStartNodes", out var ignoreToken) && ignoreToken.Type == JTokenType.String)
            {
                changes = true;
                config["ignoreUserStartNodes"] = ignoreToken.ToString() == "1";
            }

            if (config.TryGetValue("rte", out var rteToken) && rteToken is JObject rte
                && rte.TryGetValue("toolbar", out var tbToken) && tbToken is JArray tb
                && tb.Any(t => t?.ToString() == "code" || t?.ToString() == "codemirror"))
            {
                changes = true;
                tb.RemoveAll(t => t?.ToString() == "code" || t?.ToString() == "codemirror");
                tb.Insert(0, "ace");
            }

            if (changes)
            {
                dto.Configuration = config.ToString();
            }

            return changes;
        }
    }
}
