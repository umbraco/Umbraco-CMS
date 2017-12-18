using Umbraco.Core.Composing;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Marker interface for database migrations
    /// </summary>
    public interface IMigration : IDiscoverable
    {
        void Up();
        void Down();
    }
}
