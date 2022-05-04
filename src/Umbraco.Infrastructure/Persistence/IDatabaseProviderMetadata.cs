using System.Runtime.Serialization;
using Umbraco.Cms.Core.Install.Models;

namespace Umbraco.Cms.Infrastructure.Persistence;

public interface IDatabaseProviderMetadata
{
    /// <summary>
    ///     Gets a unique identifier for this set of metadata used for filtering.
    /// </summary>
    [DataMember(Name = "id")]
    Guid Id { get; }

    /// <summary>
    ///     Gets a value to determine display order and quick install priority.
    /// </summary>
    [DataMember(Name = "sortOrder")]
    int SortOrder { get; }

    /// <summary>
    ///     Gets a friendly name to describe the provider.
    /// </summary>
    [DataMember(Name = "displayName")]
    string DisplayName { get; }

    /// <summary>
    ///     Gets the default database name for the provider.
    /// </summary>
    [DataMember(Name = "defaultDatabaseName")]
    string DefaultDatabaseName { get; }

    /// <summary>
    ///     Gets the database factory provider name.
    /// </summary>
    [DataMember(Name = "providerName")]
    string? ProviderName { get; }

    /// <summary>
    ///     Gets a value indicating whether can be used for one click install.
    /// </summary>
    [DataMember(Name = "supportsQuickInstall")]
    bool SupportsQuickInstall { get; }

    /// <summary>
    ///     Gets a value indicating whether should be available for selection.
    /// </summary>
    [DataMember(Name = "isAvailable")]
    bool IsAvailable { get; }

    /// <summary>
    ///     Gets a value indicating whether the server/hostname field must be populated.
    /// </summary>
    [DataMember(Name = "requiresServer")]
    bool RequiresServer { get; }

    /// <summary>
    ///     Gets a value used as input placeholder for server/hostnmae field.
    /// </summary>
    [DataMember(Name = "serverPlaceholder")]
    string? ServerPlaceholder { get; }

    /// <summary>
    ///     Gets a value indicating whether a username and password are required (in general) to connect to the database
    /// </summary>
    [DataMember(Name = "requiresCredentials")]
    bool RequiresCredentials { get; }

    /// <summary>
    ///     Gets a value indicating whether integrated authentication is supported (e.g. SQL Server &amp; Oracle).
    /// </summary>
    [DataMember(Name = "supportsIntegratedAuthentication")]
    bool SupportsIntegratedAuthentication { get; }

    /// <summary>
    ///     Gets a value indicating whether the connection should be tested before continuing install process.
    /// </summary>
    [DataMember(Name = "requiresConnectionTest")]
    bool RequiresConnectionTest { get; }

    /// <summary>
    ///     Gets a value indicating to ignore the value of GlobalSettings.InstallMissingDatabase
    /// </summary>
    public bool ForceCreateDatabase { get; }

    /// <summary>
    ///     Creates a connection string for this provider.
    /// </summary>
    string? GenerateConnectionString(DatabaseModel databaseModel);
}
