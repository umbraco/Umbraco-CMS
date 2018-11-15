using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Represents a migration context.
    /// </summary>
    internal class MigrationContext : IMigrationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationContext"/> class.
        /// </summary>
        public MigrationContext(IUmbracoDatabase database, ILogger logger)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public ILogger Logger { get; }

        /// <inheritdoc />
        public IUmbracoDatabase Database { get; }

        /// <inheritdoc />
        public ISqlContext SqlContext => Database.SqlContext;

        /// <inheritdoc />
        public int Index { get; set; }

        /// <inheritdoc />
        public bool BuildingExpression { get; set; }
    }
}
