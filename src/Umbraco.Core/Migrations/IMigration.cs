using Umbraco.Core.Composing;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Represents a migration.
    /// </summary>
    public interface IMigration : IDiscoverable
    {
        void Migrate();
    }
}
