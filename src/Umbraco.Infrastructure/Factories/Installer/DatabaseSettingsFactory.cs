using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Factories.Installer;

/// <summary>
/// Provides a factory for creating and configuring database settings used during the installation process.
/// </summary>
public class DatabaseSettingsFactory : IDatabaseSettingsFactory
{
    private readonly IEnumerable<IDatabaseProviderMetadata> _databaseProviderMetadata;
    private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Factories.Installer.DatabaseSettingsFactory"/> class,
    /// used to create and configure database settings during the installation process.
    /// </summary>
    /// <param name="databaseProviderMetadata">A collection containing metadata for available database providers.</param>
    /// <param name="connectionStrings">An <see cref="IOptionsMonitor{TOptions}"/> instance for monitoring <see cref="ConnectionStrings"/> options.</param>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used for mapping objects within Umbraco.</param>
    public DatabaseSettingsFactory(
        IEnumerable<IDatabaseProviderMetadata> databaseProviderMetadata,
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        IUmbracoMapper mapper)
    {
        _databaseProviderMetadata = databaseProviderMetadata;
        _connectionStrings = connectionStrings;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public ICollection<DatabaseSettingsModel> GetDatabaseSettings()
    {
        ConnectionStrings? connectionString = _connectionStrings.CurrentValue;

        // If the connection string is configured we just return the configured provider.
        if (connectionString.IsConnectionStringConfigured())
        {
            var providerName = connectionString.ProviderName;
            IDatabaseProviderMetadata? providerMetaData = _databaseProviderMetadata
                .FirstOrDefault(x => x.ProviderName?.Equals(providerName, StringComparison.InvariantCultureIgnoreCase) ?? false);

            if (providerMetaData is null)
            {
                throw new InvalidOperationException($"Provider {providerName} is not a registered provider");
            }

            DatabaseSettingsModel configuredProvider = _mapper.Map<DatabaseSettingsModel>(providerMetaData)!;

            configuredProvider.IsConfigured = true;

            return new[] { configuredProvider };
        }

        List<DatabaseSettingsModel> providers = _mapper.MapEnumerable<IDatabaseProviderMetadata, DatabaseSettingsModel>(_databaseProviderMetadata);
        return providers;
    }
}
