using System.Collections.Generic;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Persistence.Migrations
{
    public interface IMigrationContext : IDatabaseContext
    {
        IUmbracoDatabase Database { get; }

        ICollection<IMigrationExpression> Expressions { get; set; }

        ILogger Logger { get; }

        ILocalMigration GetLocalMigration();
    }
}
