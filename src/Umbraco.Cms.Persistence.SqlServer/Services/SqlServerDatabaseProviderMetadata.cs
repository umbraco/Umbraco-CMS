using System.Data.Common;
using System.Runtime.Serialization;
using Microsoft.Data.SqlClient;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.SqlServer.Services;

/// <summary>
///     Provider metadata for SQL Server
/// </summary>
[DataContract]
public class SqlServerDatabaseProviderMetadata : IDatabaseProviderMetadata
{
    /// <inheritdoc />
    public Guid Id => new("5e1ad149-1951-4b74-90bf-2ac2aada9e73");

    /// <inheritdoc />
    public int SortOrder => 2;

    /// <inheritdoc />
    public string DisplayName => "SQL Server";

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
    public string ServerPlaceholder => "(local)\\SQLEXPRESS";

    /// <inheritdoc />
    public bool RequiresCredentials => true;

    /// <inheritdoc />
    public bool SupportsIntegratedAuthentication => true;

    /// <inheritdoc />
    public bool RequiresConnectionTest => true;

    /// <inheritdoc />
    public bool ForceCreateDatabase => true;

    /// <inheritdoc />
    public bool CanRecognizeConnectionString(string? connectionString)
    {
        if (connectionString is null)
        {
            return false;
        }

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            return string.IsNullOrEmpty(builder.AttachDBFilename);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public string GenerateConnectionString(DatabaseModel databaseModel)
    {
        string connectionString = $"Server={databaseModel.Server};Database={databaseModel.DatabaseName};";
        connectionString = HandleIntegratedAuthentication(connectionString, databaseModel);
        connectionString = HandleTrustServerCertificate(connectionString, databaseModel);

        return connectionString;
    }

    private static string HandleIntegratedAuthentication(string connectionString, DatabaseModel databaseModel)
    {
        if (databaseModel.IntegratedAuth)
        {
            connectionString += "Integrated Security=true";
        }
        else
        {
            connectionString += $"User Id={databaseModel.Login};Password={databaseModel.Password}";
        }

        return connectionString;
    }

    private static string HandleTrustServerCertificate(string connectionString, DatabaseModel databaseModel)
    {
        if (databaseModel.TrustServerCertificate)
        {
            connectionString += ";TrustServerCertificate=true;";
        }

        return connectionString;
    }

}
