using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Creates and manages the "ambient" database.
/// </summary>
public interface IUmbracoDatabaseFactory : IDisposable
{
    /// <summary>
    ///     Gets a value indicating whether the database factory is configured, i.e. whether
    ///     its connection string and provider name have been set. The factory may however not
    ///     be initialized (see <see cref="Initialized" />).
    /// </summary>
    bool Configured { get; }

    /// <summary>
    ///     Gets a value indicating whether the database factory is initialized, i.e. whether
    ///     its internal state is ready and it has been possible to connect to the database.
    /// </summary>
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    bool Initialized { get; }

    /// <summary>
    ///     Gets the connection string.
    /// </summary>
    /// <remarks>May return <c>null</c> if the database factory is not configured.</remarks>
    string? ConnectionString { get; }

    /// <summary>
    ///     Gets the provider name.
    /// </summary>
    /// <remarks>May return <c>null</c> if the database factory is not configured.</remarks>
    string? ProviderName { get; }

    /// <summary>
    ///     Gets a value indicating whether the database factory is configured (see <see cref="Configured" />),
    ///     and it is possible to connect to the database. The factory may however not be initialized (see
    ///     <see cref="Initialized" />).
    /// </summary>
    bool CanConnect { get; }

    /// <summary>
    ///     Gets the <see cref="ISqlContext" />.
    /// </summary>
    /// <remarks>
    ///     <para>Getting the <see cref="ISqlContext" /> causes the factory to initialize if it is not already initialized.</para>
    /// </remarks>
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    ISqlContext SqlContext { get; }

    /// <summary>
    ///     Gets the <see cref="IBulkSqlInsertProvider" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Getting the <see cref="IBulkSqlInsertProvider" /> causes the factory to initialize if it is not already
    ///         initialized.
    ///     </para>
    /// </remarks>
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    IBulkSqlInsertProvider? BulkSqlInsertProvider { get; }

    /// <summary>
    ///     Creates a new database.
    /// </summary>
    /// <remarks>
    ///     <para>The new database must be disposed after being used.</para>
    ///     <para>Creating a database causes the factory to initialize if it is not already initialized.</para>
    /// </remarks>
    IUmbracoDatabase CreateDatabase();

    /// <summary>
    ///     Creates a new database.
    /// </summary>
    /// <remarks>
    ///     <para>The new database must be disposed after being used.</para>
    ///     <para>Creating a database causes the factory to initialize if it is not already initialized.</para>
    /// </remarks>
    Task<IUmbracoDatabase> CreateDatabaseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Configures the database factory.
    /// </summary>
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    void Configure(ConnectionStrings umbracoConnectionString);

    [Obsolete("Please use alternative Configure method.")]
    void Configure(string connectionString, string providerName) =>
        Configure(new ConnectionStrings { ConnectionString = connectionString, ProviderName = providerName });

    /// <summary>
    ///     Configures the database factory for upgrades.
    /// </summary>
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    void ConfigureForUpgrade();
}
