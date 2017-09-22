using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Persistence.Migrations
{
    internal class MigrationContext : IMigrationContext
    {
        public MigrationContext(IUmbracoDatabase database, ILogger logger)
        {
            Expressions = new Collection<IMigrationExpression>();
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ICollection<IMigrationExpression> Expressions { get; set; }

        public IUmbracoDatabase Database { get; }

        public ISqlContext SqlContext => Database.SqlContext;

        public ILogger Logger { get; }

        public ILocalMigration GetLocalMigration() => new LocalMigration(Database, Logger);
    }
}
