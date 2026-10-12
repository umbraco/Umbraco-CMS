using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.SqlServer.Configuration;

/// <summary>
///     Applies the configured database timeouts to a SQL Server connection string.
/// </summary>
/// <remarks>
///     A connection string is the only way to set a connect timeout, as <c>DbConnection.ConnectionTimeout</c>
///     is read-only, so both timeouts are applied the same way for consistency. Doing so here also means every
///     consumer of the connection string observes them, not just NPoco.
/// </remarks>
internal sealed class ConfigureSqlServerConnectionStringTimeouts : IPostConfigureOptions<ConnectionStrings>
{
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ILogger<ConfigureSqlServerConnectionStringTimeouts> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureSqlServerConnectionStringTimeouts" /> class.
    /// </summary>
    /// <param name="globalSettings">The global settings carrying the configured timeouts.</param>
    /// <param name="logger">The logger.</param>
    public ConfigureSqlServerConnectionStringTimeouts(
        IOptions<GlobalSettings> globalSettings,
        ILogger<ConfigureSqlServerConnectionStringTimeouts> logger)
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
        if (globalSettings.DatabaseCommandTimeout is null && globalSettings.DatabaseConnectTimeout is null)
        {
            return;
        }

        SqlConnectionStringBuilder connectionStringBuilder;
        try
        {
            connectionStringBuilder = new SqlConnectionStringBuilder(options.ConnectionString);

            if (globalSettings.DatabaseCommandTimeout is { } commandTimeout)
            {
                connectionStringBuilder.CommandTimeout = commandTimeout.ToConnectionStringTimeoutSeconds();
            }

            if (globalSettings.DatabaseConnectTimeout is { } connectTimeout)
            {
                connectionStringBuilder.ConnectTimeout = connectTimeout.ToConnectionStringTimeoutSeconds();
            }
        }
        catch (ArgumentException exception)
        {
            // The connection string cannot be rewritten, so leave it alone rather than failing to configure
            // options - which would surface as an obscure startup failure rather than a connection error.
            const string message = "The configured database timeouts could not be applied, because the connection "
                + "string could not be parsed. Set \"Command Timeout\" and \"Connect Timeout\" in the connection "
                + "string instead.";
            _logger.LogWarning(exception, message);
            return;
        }

        options.ConnectionString = connectionStringBuilder.ConnectionString;
    }
}
