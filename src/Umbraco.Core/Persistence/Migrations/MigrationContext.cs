using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Umbraco.Core.Persistence.Migrations
{
    internal class MigrationContext : IMigrationContext
    {
        public MigrationContext(DatabaseProviders databaseProvider, Database database)
        {
            Expressions = new Collection<IMigrationExpression>();
            CurrentDatabaseProvider = databaseProvider;
            Database = database;
        }

        public ICollection<IMigrationExpression> Expressions { get; set; }

        public DatabaseProviders CurrentDatabaseProvider { get; private set; }

        public Database Database { get; private set; }
    }
}