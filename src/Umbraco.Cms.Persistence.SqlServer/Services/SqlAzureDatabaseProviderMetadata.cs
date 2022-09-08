using System.Runtime.Serialization;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.SqlServer.Services;

/// <summary>
///     Provider metadata for SQL Azure
/// </summary>
[DataContract]
public class SqlAzureDatabaseProviderMetadata : IDatabaseProviderMetadata
{
    /// <inheritdoc />
    public Guid Id => new("7858e827-8951-4fe0-a7fe-6883011b1f1b");

    /// <inheritdoc />
    public int SortOrder => 3;

    /// <inheritdoc />
    public string DisplayName => "Azure SQL";

    /// <inheritdoc />
    public string DefaultDatabaseName => string.Empty;

    /// <inheritdoc />
    public string ProviderName => Constants.ProviderName;

    /// <inheritdoc />
    public bool SupportsQuickInstall => false;

    /// <inheritdoc />
    public bool IsAvailable => true;

    /// <inheritdoc />
    public bool RequiresServer => true;

    /// <inheritdoc />
    public string ServerPlaceholder => "umbraco-database.database.windows.net";

    /// <inheritdoc />
    public bool RequiresCredentials => true;

    /// <inheritdoc />
    public bool SupportsIntegratedAuthentication => false;

    /// <inheritdoc />
    public bool RequiresConnectionTest => true;

    /// <inheritdoc />
    public bool ForceCreateDatabase => false;

    /// <inheritdoc />
    public string GenerateConnectionString(DatabaseModel databaseModel)
    {
        if (databaseModel.Server is null)
        {
            throw new ArgumentNullException(nameof(databaseModel.Server));
        }

        var server = databaseModel.Server;
        var databaseName = databaseModel.DatabaseName;
        var user = databaseModel.Login;
        var password = databaseModel.Password;

        if (server.Contains(".") && ServerStartsWithTcp(server) == false)
        {
            server = $"tcp:{server}";
        }

        if (server.Contains(".") == false && ServerStartsWithTcp(server))
        {
            var serverName = server.Contains(",")
                ? server.Substring(0, server.IndexOf(",", StringComparison.Ordinal))
                : server;

            var portAddition = string.Empty;

            if (server.Contains(","))
            {
                portAddition = server.Substring(server.IndexOf(",", StringComparison.Ordinal));
            }

            server = $"{serverName}.database.windows.net{portAddition}";
        }

        if (ServerStartsWithTcp(server) == false)
        {
            server = $"tcp:{server}.database.windows.net";
        }

        if (server.Contains(",") == false)
        {
            server = $"{server},1433";
        }

        if (user?.Contains("@") == false)
        {
            var userDomain = server;

            if (ServerStartsWithTcp(server))
            {
                userDomain = userDomain.Substring(userDomain.IndexOf(":", StringComparison.Ordinal) + 1);
            }

            if (userDomain.Contains("."))
            {
                userDomain = userDomain.Substring(0, userDomain.IndexOf(".", StringComparison.Ordinal));
            }

            user = $"{user}@{userDomain}";
        }

        return $"Server={server};Database={databaseName};User ID={user};Password={password}";
    }

    private static bool ServerStartsWithTcp(string server) => server.InvariantStartsWith("tcp:");
}
