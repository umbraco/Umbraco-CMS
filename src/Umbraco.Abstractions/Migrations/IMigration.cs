using Umbraco.Core.Composing;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Represents a migration.
    /// </summary>
    public interface IMigration : IDiscoverable
    {
        /// <summary>
        /// Executes the migration.
        /// </summary>
        void Migrate();
    }
}
