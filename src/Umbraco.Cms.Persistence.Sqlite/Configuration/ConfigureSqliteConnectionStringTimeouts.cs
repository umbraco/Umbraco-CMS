using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.Sqlite.Configuration;

/// <summary>
///     Applies the configured database command timeout to a SQLite connection string.
/// </summary>
/// <remarks>
///     SQLite has no notion of a connect timeout, so <see cref="GlobalSettings.DatabaseConnectTimeout" /> has
///     nothing to map onto and is reported as inapplicable rather than silently dropped.
/// </remarks>
internal sealed class ConfigureSqliteConnectionStringTimeouts : IPostConfigureOptions<ConnectionStrings>
{
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ILogger<ConfigureSqliteConnectionStringTimeouts> _logger;
    private bool _connectTimeoutReported;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureSqliteConnectionStringTimeouts" /> class.
    /// </summary>
    /// <param name="globalSettings">The global settings carrying the configured timeouts.</param>
    /// <param name="logger">The logger.</param>
    public ConfigureSqliteConnectionStringTimeouts(
        IOptions<GlobalSettings> globalSettings,
        ILogger<ConfigureSqliteConnectionStringTimeouts> logger)
    {
        _globalSettings = globalSettings;
        _logger = logger;
    }

    /// <inheritdoc />
    public void PostConfigure(string? name, ConnectionStrings options)
    {
        if (options.IsConnectionStringConfigured() is false
            || options.ProviderName.InvariantEquals(Constants.ProviderName) is false)
        {
            return;
        }

        GlobalSettings globalSettings = _globalSettings.Value;

        if (globalSettings.DatabaseConnectTimeout is not null && _connectTimeoutReported is false)
        {
            _connectTimeoutReported = true;
            _logger.LogInformation(
                "Umbraco:CMS:Global:DatabaseConnectTimeout is configured but does not apply to SQLite, which has no connect timeout.");
        }

        if (globalSettings.DatabaseCommandTimeout is not { } commandTimeout)
        {
            return;
        }

        SqliteConnectionStringBuilder connectionStringBuilder;
        try
        {
            connectionStringBuilder = new SqliteConnectionStringBuilder(options.ConnectionString)
            {
                DefaultTimeout = commandTimeout.ToConnectionStringTimeoutSeconds(),
            };
        }
        catch (ArgumentException exception)
        {
            // The connection string cannot be rewritten, so leave it alone rather than failing to configure
            // options - which would surface as an obscure startup failure rather than a connection error.
            const string message = "The configured database command timeout could not be applied, because the "
                + "connection string could not be parsed. Set \"Command Timeout\" in the connection string instead.";
            _logger.LogWarning(exception, message);
            return;
        }

        options.ConnectionString = connectionStringBuilder.ConnectionString;
    }
}
