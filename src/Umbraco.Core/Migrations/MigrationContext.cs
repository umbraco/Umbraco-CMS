using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Migrations
{
    internal class MigrationContext : IMigrationContext
    {
        public MigrationContext(IUmbracoDatabase database, ILogger logger)
        {
            Expressions = new Collection<IMigrationExpression>();
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ILogger Logger { get; }

        public IUmbracoDatabase Database { get; }

        public ISqlContext SqlContext => Database.SqlContext;

        public int Index { get; set; }

        public ICollection<IMigrationExpression> Expressions { get; set; } // fixme kill

        public ILocalMigration GetLocalMigration() => new LocalMigration(Database, Logger); // fixme kill
    }
}
