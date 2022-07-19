using System.Runtime.Serialization;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Infrastructure.Persistence;

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
    public bool ForceCreateDatabase => false;

    /// <inheritdoc />
    public string GenerateConnectionString(DatabaseModel databaseModel) =>
        databaseModel.IntegratedAuth
            ? $"Server={databaseModel.Server};Database={databaseModel.DatabaseName};Integrated Security=true"
            : $"Server={databaseModel.Server};Database={databaseModel.DatabaseName};User Id={databaseModel.Login};Password={databaseModel.Password}";
}
