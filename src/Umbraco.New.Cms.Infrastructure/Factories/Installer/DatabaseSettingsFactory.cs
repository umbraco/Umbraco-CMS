using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Factories;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Infrastructure.Factories.Installer;

public class DatabaseSettingsFactory : IDatabaseSettingsFactory
{
    private readonly IEnumerable<IDatabaseProviderMetadata> _databaseProviderMetadata;
    private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;
    private readonly IUmbracoMapper _mapper;

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
