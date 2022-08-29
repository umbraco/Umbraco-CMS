namespace Umbraco.Cms.Core.Persistence;

public interface IDatabaseInfo
{
    bool IsConfigured { get; }

    Task<bool> IsUmbracoInstalledAsync();
    Task<DatabaseState> GetStateAsync();
}
