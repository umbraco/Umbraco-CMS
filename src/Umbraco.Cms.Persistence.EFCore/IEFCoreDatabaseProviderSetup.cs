using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Persistence.EFCore;

/// <summary>
///     Configures any <see cref="DbContext" /> to use a database provider, so that
///     <see cref="Umbraco.Extensions.UmbracoEFCoreServiceCollectionExtensions.UseUmbracoDatabaseProvider" /> supports
///     providers that are not part of Umbraco.
/// </summary>
/// <remarks>
///     Unlike <see cref="Migrations.IMigrationProviderSetup" />, which configures Umbraco's own context and its
///     migrations, this leaves the context's migrations assembly unchanged.
/// </remarks>
public interface IEFCoreDatabaseProviderSetup
{
    /// <summary>
    ///     Gets the name of the database provider this setup supports.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    ///     Configures the options builder to use the database provider.
    /// </summary>
    /// <param name="builder">The DbContext options builder to configure.</param>
    /// <param name="connectionString">The connection string to use.</param>
    void Setup(DbContextOptionsBuilder builder, string connectionString);
}
