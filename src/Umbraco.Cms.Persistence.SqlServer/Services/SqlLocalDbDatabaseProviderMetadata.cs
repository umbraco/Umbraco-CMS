using System.Runtime.Serialization;
using Microsoft.Data.SqlClient;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.SqlServer.Services;

/// <summary>
///     Provider metadata for SQL Server LocalDb
/// </summary>
[DataContract]
public class SqlLocalDbDatabaseProviderMetadata : IDatabaseProviderMetadata
{
    /// <inheritdoc />
    public Guid Id => new("05a7e9ed-aa6a-43af-a309-63422c87c675");

    /// <inheritdoc />
    public int SortOrder => 1;

    /// <inheritdoc />
    public string DisplayName => "SQL Server Express LocalDB";

    /// <inheritdoc />
    public string DefaultDatabaseName => Core.Constants.System.UmbracoDefaultDatabaseName;

    /// <inheritdoc />
    public string ProviderName => Constants.ProviderName;

    /// <inheritdoc />
    public bool SupportsQuickInstall => true;

    /// <inheritdoc />
    public bool IsAvailable => new LocalDb().IsAvailable;

    /// <inheritdoc />
    public bool RequiresServer => false;

    /// <inheritdoc />
    public string? ServerPlaceholder => null;

    /// <inheritdoc />
    public bool RequiresCredentials => false;

    /// <inheritdoc />
    public bool SupportsIntegratedAuthentication => false;

    /// <inheritdoc />
    public bool RequiresConnectionTest => false;

    /// <inheritdoc />
    public bool ForceCreateDatabase => true;

    /// <inheritdoc />
    public string GenerateConnectionString(DatabaseModel databaseModel)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\MSSQLLocalDB",
            AttachDBFilename = @$"{ConnectionStrings.DataDirectoryPlaceholder}\{databaseModel.DatabaseName}.mdf",
            IntegratedSecurity = true
        };

        return builder.ConnectionString;
    }
}
