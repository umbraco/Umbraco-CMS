using System;
using System.Collections.Generic;

namespace Umbraco.Core.Migrations
{
    // exists so the builder can be mocked in tests
    public interface IMigrationCollectionBuilder
    {
        /// <summary>
        /// Gets all migration types.
        /// </summary>
        IEnumerable<Type> MigrationTypes { get; }

        /// <summary>
        /// Instanciates a migration.
        /// </summary>
        IMigration Instanciate(Type migrationType, IMigrationContext context);
    }
}
