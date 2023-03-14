using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Infrastructure.Persistence;

public interface IUmbracoDatabase : IDatabase
{
    /// <summary>
    ///     Gets the Sql context.
    /// </summary>
    ISqlContext SqlContext { get; }

    /// <summary>
    ///     Gets the database instance unique identifier as a string.
    /// </summary>
    /// <remarks>
    ///     UmbracoDatabase returns the first eight digits of its unique Guid and, in some
    ///     debug mode, the underlying database connection identifier (if any).
    /// </remarks>
    string InstanceId { get; }

    /// <summary>
    ///     Gets a value indicating whether the database is currently in a transaction.
    /// </summary>
    bool InTransaction { get; }

    bool EnableSqlCount { get; set; }

    int SqlCount { get; }

    int BulkInsertRecords<T>(IEnumerable<T> records);

    bool IsUmbracoInstalled();

    DatabaseSchemaResult ValidateSchema();
}
