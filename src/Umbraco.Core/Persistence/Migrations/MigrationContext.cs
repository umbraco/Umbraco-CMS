using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Umbraco.Core.Persistence.Migrations
{
    internal class MigrationContext : IMigrationContext
    {
        public MigrationContext(DatabaseProviders databaseProvider)
        {
            Expressions = new Collection<IMigrationExpression>();
            CurrentDatabaseProvider = databaseProvider;
        }

        public ICollection<IMigrationExpression> Expressions { get; set; }

        public DatabaseProviders CurrentDatabaseProvider { get; private set; }
    }
}