using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.BackOfficeApi.Models.Installer;

public class InstallSettingsModel
{
    public UserSettingsModel UserSettings { get; set; } = null!;

    public IEnumerable<IDatabaseProviderMetadata> DatabaseSettings { get; set; } = Enumerable.Empty<IDatabaseProviderMetadata>();
}
