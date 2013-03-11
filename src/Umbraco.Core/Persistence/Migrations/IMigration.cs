namespace Umbraco.Core.Persistence.Migrations
{
    /// <summary>
    /// Marker interface for database migrations
    /// </summary>
    public interface IMigration
    {
        void Up();
        void Down();
    }
}