using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Infrastructure.Persistence;

public interface IUmbracoDatabase : IDatabase, IUmbracoDatabaseContract
{
    /// <summary>
    ///     Gets the Sql context.
    /// </summary>
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    ISqlContext SqlContext { get; }

    /// <summary>
    ///     Gets the database instance unique identifier as a string.
    /// </summary>
    /// <remarks>
    ///     UmbracoDatabase returns the first eight digits of its unique Guid and, in some
    ///     debug mode, the underlying database connection identifier (if any).
    /// </remarks>
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    string InstanceId { get; }

    /// <summary>
    ///     Gets a value indicating whether the database is currently in a transaction.
    /// </summary>
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    bool InTransaction { get; }

    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    bool EnableSqlCount { get; set; }

    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    int SqlCount { get; }

    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    int BulkInsertRecords<T>(IEnumerable<T> records);

    bool IsUmbracoInstalled();

    DatabaseSchemaResult ValidateSchema();
}
