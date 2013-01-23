using System.Collections.Generic;

namespace Umbraco.Core.Persistence.Migrations
{
    public interface IMigrationContext
    {
        ICollection<IMigrationExpression> Expressions { get; set; }
        DatabaseProviders CurrentDatabaseProvider { get; }
        Database Database { get; }
    }
}