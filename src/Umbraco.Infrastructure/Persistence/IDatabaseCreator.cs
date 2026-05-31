namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Represents a service responsible for creating and managing the database schema within the persistence layer.
/// </summary>
public interface IDatabaseCreator
{
    /// <summary>
    /// Gets the name of the database provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Creates a new database instance using the provided connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to use for creating the database.</param>
    void Create(string connectionString);
}
