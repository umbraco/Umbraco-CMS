using Newtonsoft.Json.Linq;
using System.Linq;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_7_0
{
    /// <summary>
    /// Updates the pre-values on the Multiple Textstrings data type to
    /// preserve value settings from v7
    /// </summary>
    public class MultipleTextstringPreValueMigrator : MigrationBase
    {
        public MultipleTextstringPreValueMigrator(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            var sql = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(d => d.EditorAlias == Constants.PropertyEditors.Aliases.MultipleTextstring);

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

        // Convert from old names to new names
        private bool Migrate(DataTypeDto dto)
        {
            if (dto.Configuration.IsNullOrWhiteSpace() || dto.Configuration[0] != '{') return false;
            var config = JObject.Parse(dto.Configuration);
            var changes = false;

            if (config.TryGetValue("0", out var token) && token is JObject obj)
            {
                changes = true;
                config = obj;
            }

            if (changes)
            {
                dto.Configuration = config.ToString();
            }

            return changes;
        }
    }
}
