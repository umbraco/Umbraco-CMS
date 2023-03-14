using System.Runtime.Serialization;
using Microsoft.Data.Sqlite;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.Sqlite.Services;

[DataContract]
public class SqliteDatabaseProviderMetadata : IDatabaseProviderMetadata
{
    /// <inheritdoc />
    public Guid Id => new("530386a2-b219-4d5f-b68c-b965e14c9ac9");

    /// <inheritdoc />
    public int SortOrder => -1;

    /// <inheritdoc />
    public string DisplayName => "SQLite";

    /// <inheritdoc />
    public string DefaultDatabaseName => Core.Constants.System.UmbracoDefaultDatabaseName;

    /// <inheritdoc />
    public string ProviderName => Constants.ProviderName;

    /// <inheritdoc />
    public bool SupportsQuickInstall => true;

    /// <inheritdoc />
    public bool IsAvailable => true;

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
    /// <remarks>
    ///     <para>
    ///         Required to ensure database creator is used regardless of configured InstallMissingDatabase value.
    ///     </para>
    ///     <para>
    ///         Ensures database setup with journal_mode = wal;
    ///     </para>
    /// </remarks>
    public bool ForceCreateDatabase => true;

    /// <inheritdoc />
    public string GenerateConnectionString(DatabaseModel databaseModel)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = $"{ConnectionStrings.DataDirectoryPlaceholder}/{databaseModel.DatabaseName}.sqlite.db",
            ForeignKeys = true,
            Pooling = true,
            Cache = SqliteCacheMode.Shared
        };

        return builder.ConnectionString;
    }
}
