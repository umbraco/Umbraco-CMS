using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    public class PreValueMigratorCollection : BuilderCollectionBase<IPreValueMigrator>
    {
        private readonly ILogger<PreValueMigratorCollection> _logger;

        public PreValueMigratorCollection(IEnumerable<IPreValueMigrator> items, ILogger<PreValueMigratorCollection> logger)
            : base(items)
        {
            _logger = logger;
            _logger.LogDebug("Migrators: " + string.Join(", ", items.Select(x => x.GetType().Name)));
        }

        public IPreValueMigrator GetMigrator(string editorAlias)
        {
            var migrator = this.FirstOrDefault(x => x.CanMigrate(editorAlias));
            _logger.LogDebug("Getting migrator for \"{EditorAlias}\" = {MigratorType}", editorAlias, migrator == null ? "<null>" : migrator.GetType().Name);
            return migrator;
        }
    }
}
