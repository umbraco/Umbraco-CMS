using System.Collections.Generic;
using System.Collections.ObjectModel;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Persistence.Migrations
{
    internal class MigrationContext : IMigrationContext
    {
        public MigrationContext(DatabaseProviders databaseProvider, Database database, ILogger logger)
        {
            Expressions = new Collection<IMigrationExpression>();
            CurrentDatabaseProvider = databaseProvider;
            Database = database;
            Logger = logger;
        }

        public ICollection<IMigrationExpression> Expressions { get; set; }

        public DatabaseProviders CurrentDatabaseProvider { get; private set; }

        public Database Database { get; private set; }

        public ILogger Logger { get; private set; }
    }
}