using System.Collections.Generic;

namespace Umbraco.Core.Persistence.Migrations
{
    public interface IMigrationResolver
    {
        /// <summary>
        /// Gets the migrations
        /// </summary>
        IEnumerable<IMigration> GetMigrations(IMigrationContext migrationContext);
    }
}