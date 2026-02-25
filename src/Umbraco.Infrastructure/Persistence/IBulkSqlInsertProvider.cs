namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Provides an interface for implementing bulk SQL insert operations in a database.
/// </summary>
public interface IBulkSqlInsertProvider
{
    /// <summary>
    /// Gets the unique name that identifies the bulk SQL insert provider implementation.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Inserts a collection of records of type <typeparamref name="T"/> into the database in bulk.
    /// </summary>
    /// <typeparam name="T">The type of records to insert.</typeparam>
    /// <param name="database">The <see cref="IUmbracoDatabase"/> instance to use for the bulk insert operation.</param>
    /// <param name="records">The collection of records to insert.</param>
    /// <returns>The number of records that were successfully inserted into the database.</returns>
    int BulkInsertRecords<T>(IUmbracoDatabase database, IEnumerable<T> records);
}
