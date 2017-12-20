using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Provides context to migrations.
    /// </summary>
    public interface IMigrationContext
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Gets the database instance.
        /// </summary>
        IUmbracoDatabase Database { get; }

        /// <summary>
        /// Gets the Sql context.
        /// </summary>
        ISqlContext SqlContext { get; }

        /// <summary>
        /// Gets the expression index.
        /// </summary>
        int Index { get; set; }
    }
}
