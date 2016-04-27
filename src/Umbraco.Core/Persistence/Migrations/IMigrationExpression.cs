using NPoco;

namespace Umbraco.Core.Persistence.Migrations
{
    /// <summary>
    /// Marker interface for migration expressions
    /// </summary>
    public interface IMigrationExpression
    {
        string Process(Database database);
    }
}