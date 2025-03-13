using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Extension methods for <see cref="IDatabaseProviderMetadata" />.
/// </summary>
public static class DatabaseProviderMetadataExtensions
{
    /// <summary>
    /// Gets the available database provider metadata.
    /// </summary>
    /// <param name="databaseProviderMetadata">The database provider metadata.</param>
    /// <param name="onlyQuickInstall">If set to <c>true</c> only returns providers that support quick install.</param>
    /// <returns>
    /// The available database provider metadata.
    /// </returns>
    public static IEnumerable<IDatabaseProviderMetadata> GetAvailable(this IEnumerable<IDatabaseProviderMetadata> databaseProviderMetadata, bool onlyQuickInstall = false)
        => databaseProviderMetadata.Where(x => (!onlyQuickInstall || x.SupportsQuickInstall) && x.IsAvailable).OrderBy(x => x.SortOrder);

    /// <summary>
    /// Determines whether a database can be created for the specified provider name while ignoring the value of <see cref="GlobalSettings.InstallMissingDatabase" />.
    /// </summary>
    /// <param name="databaseProviderMetadata">The database provider metadata.</param>
    /// <param name="providerName">The name of the provider.</param>
    /// <returns>
    ///   <c>true</c> if a database can be created for the specified provider name; otherwise, <c>false</c>.
    /// </returns>
    [Obsolete("Use CanForceCreateDatabase that takes an IUmbracoDatabaseFactory. Scheduled for removal in Umbraco 13.")]
    public static bool CanForceCreateDatabase(this IEnumerable<IDatabaseProviderMetadata> databaseProviderMetadata, string? providerName)
    {
        return databaseProviderMetadata
            .FirstOrDefault(x =>
                string.Equals(x.ProviderName, providerName, StringComparison.InvariantCultureIgnoreCase))
            ?.ForceCreateDatabase == true;
    }

    /// <summary>
    /// Determines whether a database can be created for the specified provider name while ignoring the value of <see cref="GlobalSettings.InstallMissingDatabase" />.
    /// </summary>
    /// <param name="databaseProviderMetadata">The database provider metadata.</param>
    /// <param name="umbracoDatabaseFactory">The database factory.</param>
    /// <returns>
    ///   <c>true</c> if a database can be created for the specified database; otherwise, <c>false</c>.
    /// </returns>
    public static bool CanForceCreateDatabase(this IEnumerable<IDatabaseProviderMetadata> databaseProviderMetadata, IUmbracoDatabaseFactory umbracoDatabaseFactory)
    {
        // In case more metadata providers can recognize the connection string, we need to check if any can force create.
        // E.g. Both SqlServer and SqlAzure will recognize an azure connection string, but luckily none of those can force create.
        return databaseProviderMetadata
            .Where(x =>
                string.Equals(x.ProviderName, umbracoDatabaseFactory.SqlContext.SqlSyntax.ProviderName, StringComparison.InvariantCultureIgnoreCase)
                && x.CanRecognizeConnectionString(umbracoDatabaseFactory.ConnectionString) && x.IsAvailable).Any(x => x.ForceCreateDatabase == true);
    }

    /// <summary>
    /// Generates the connection string.
    /// </summary>
    /// <param name="databaseProviderMetadata">The database provider metadata.</param>
    /// <param name="databaseName">The name of the database, uses the default database name when <c>null</c>.</param>
    /// <param name="server">The server.</param>
    /// <param name="login">The login.</param>
    /// <param name="password">The password.</param>
    /// <param name="integratedAuth">Indicates whether integrated authentication should be used (when supported by the provider).</param>
    /// <returns>
    /// The generated connection string.
    /// </returns>
    public static string? GenerateConnectionString(this IDatabaseProviderMetadata databaseProviderMetadata, string? databaseName = null, string? server = null, string? login = null, string? password = null, bool? integratedAuth = null)
        => databaseProviderMetadata.GenerateConnectionString(new DatabaseModel()
        {
            DatabaseProviderMetadataId = databaseProviderMetadata.Id,
            ProviderName = databaseProviderMetadata.ProviderName,
            DatabaseName = databaseName ?? databaseProviderMetadata.DefaultDatabaseName,
            Server = server ?? string.Empty,
            Login = login ?? string.Empty,
            Password = password ?? string.Empty,
            IntegratedAuth = integratedAuth == true && databaseProviderMetadata.SupportsIntegratedAuthentication
        });
}
