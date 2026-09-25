using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.Sqlite.Configuration;

/// <summary>
///     Downgrades a SQLite connection string from <see cref="SqliteOpenMode.ReadWriteCreate" /> to
///     <see cref="SqliteOpenMode.ReadWrite" />, to prevent accidental creation of SQLite database files.
/// </summary>
internal sealed class ConfigureSqliteConnectionStringMode : IPostConfigureOptions<ConnectionStrings>
{
    private readonly ILogger<ConfigureSqliteConnectionStringMode> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureSqliteConnectionStringMode" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ConfigureSqliteConnectionStringMode(ILogger<ConfigureSqliteConnectionStringMode> logger)
        => _logger = logger;

    /// <inheritdoc />
    public void PostConfigure(string? name, ConnectionStrings options)
    {
        if (options.IsConnectionStringConfigured() is false
            || options.ProviderName.InvariantEquals(Constants.ProviderName) is false)
        {
            return;
        }

        SqliteConnectionStringBuilder connectionStringBuilder;
        try
        {
            connectionStringBuilder = new SqliteConnectionStringBuilder(options.ConnectionString);
        }
        catch (ArgumentException exception)
        {
            // The connection string cannot be rewritten, so leave it alone rather than failing to configure
            // options - which would surface as an obscure startup failure rather than a connection error.
            const string message = "The connection mode could not be applied, because the connection string "
                + "could not be parsed. Set \"Mode=ReadWrite\" in the connection string instead.";
            _logger.LogWarning(exception, message);
            return;
        }

        if (connectionStringBuilder.Mode is not SqliteOpenMode.ReadWriteCreate)
        {
            return;
        }

        connectionStringBuilder.Mode = SqliteOpenMode.ReadWrite;
        options.ConnectionString = connectionStringBuilder.ConnectionString;
    }
}
