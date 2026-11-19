using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Persistence.EFCore;

/// <summary>
///     Configures the database provider on a <see cref="DbContextOptionsBuilder" /> without setting migration assemblies.
/// </summary>
/// <remarks>
///     <para>
///         <see cref="Umbraco.Extensions.UmbracoEFCoreServiceCollectionExtensions.UseUmbracoDatabaseProvider" /> applies
///         every registered configurator that can handle the configured provider name, so that any
///         <see cref="DbContext" /> can use providers that are not part of Umbraco. The <see cref="CanHandle" /> method
///         lets the implementation own provider-name matching.
///     </para>
///     <para>
///         Unlike <see cref="Migrations.IMigrationProviderSetup" />, which configures Umbraco's own context and its
///         migrations, this leaves the context's migrations assembly unchanged. A database provider package typically
///         registers both.
///     </para>
/// </remarks>
public interface IDatabaseConfigurator
{
    /// <summary>
    ///     Determines whether this configurator can handle the specified provider name.
    /// </summary>
    /// <param name="providerName">The database provider name from the connection string configuration.</param>
    /// <returns><c>true</c> if this configurator can handle the provider; otherwise, <c>false</c>.</returns>
    bool CanHandle(string providerName);

    /// <summary>
    ///     Configures the <see cref="DbContextOptionsBuilder" /> for the database provider.
    /// </summary>
    /// <param name="builder">The DbContext options builder to configure.</param>
    /// <param name="connectionString">The connection string to use.</param>
    void Configure(DbContextOptionsBuilder builder, string connectionString);
}
