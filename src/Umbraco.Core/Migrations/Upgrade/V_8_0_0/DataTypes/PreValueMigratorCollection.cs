using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    public class PreValueMigratorCollection : BuilderCollectionBase<IPreValueMigrator>
    {
        private readonly ILogger _logger;

        public PreValueMigratorCollection(IEnumerable<IPreValueMigrator> items, ILogger logger)
            : base(items)
        {
            _logger = logger;
            _logger.Debug(GetType(), "Migrators: " + string.Join(", ", items.Select(x => x.GetType().Name)));
        }

        public IPreValueMigrator GetMigrator(string editorAlias)
        {
            var migrator = this.FirstOrDefault(x => x.CanMigrate(editorAlias));
            _logger.Debug<string,string>(GetType(), "Getting migrator for \"{EditorAlias}\" = {MigratorType}", editorAlias, migrator == null ? "<null>" : migrator.GetType().Name);
            return migrator;
        }
    }
}
