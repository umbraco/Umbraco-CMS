using System.Runtime.Serialization;
using Umbraco.Cms.Core.Install.Models;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Provider metadata for custom connection string setup.
/// </summary>
[DataContract]
public class CustomConnectionStringDatabaseProviderMetadata : IDatabaseProviderMetadata
{
    /// <inheritdoc />
    public Guid Id => new("42c0eafd-1650-4bdb-8cf6-d226e8941698");

    /// <inheritdoc />
    public int SortOrder => int.MaxValue;

    /// <inheritdoc />
    public string DisplayName => "Custom";

    /// <inheritdoc />
    public string DefaultDatabaseName => string.Empty;

    /// <inheritdoc />
    public string? ProviderName => null;

    /// <inheritdoc />
    public bool SupportsQuickInstall => false;

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
    public bool RequiresConnectionTest => true;

    /// <inheritdoc />
    public bool ForceCreateDatabase => false;

    /// <inheritdoc />
    public string? GenerateConnectionString(DatabaseModel databaseModel)
        => databaseModel.ConnectionString;
}
