using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Migrations
{
    internal class MigrationContext : IMigrationContext
    {
        public MigrationContext(IUmbracoDatabase database, ILogger logger)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ILogger Logger { get; }

        public IUmbracoDatabase Database { get; }

        public ISqlContext SqlContext => Database.SqlContext;

        public int Index { get; set; }
    }
}
