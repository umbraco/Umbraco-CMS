using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Persistence;

public abstract class DatabaseInfoBase : IDatabaseInfo
{
    private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;

    public DatabaseInfoBase(
        IOptionsMonitor<ConnectionStrings> connectionStrings)
    {
        _connectionStrings = connectionStrings;
    }

    public async Task<bool> IsUmbracoInstalledAsync() => await GetStateAsync() switch
    {
        DatabaseState.Ok => true,
        DatabaseState.NeedsUpgrade => true,
        DatabaseState.NeedsPackageMigration => true,
        DatabaseState.CannotConnect => false,
        DatabaseState.NotConfigured => false,
        DatabaseState.NotInstalled => false,
        _ => false,
    };

    public bool IsConfigured => !_connectionStrings.CurrentValue.ConnectionString.IsNullOrWhiteSpace() && !_connectionStrings.CurrentValue.ProviderName.IsNullOrWhiteSpace();
    public async Task<DatabaseState> GetStateAsync()
    {
        if (!IsConfigured)
        {
            return DatabaseState.NotConfigured;
        }



        return await GetConfiguredStateAsync();

    }

    protected abstract Task<DatabaseState> GetConfiguredStateAsync();
}
