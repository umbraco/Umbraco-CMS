using Umbraco.Cms.BackOfficeApi.Models.Installer;
using Umbraco.Cms.BackOfficeApi.ViewModels.Installer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.BackOfficeApi.Mapping;

public class InstallSettingsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<InstallSettingsModel, InstallSettingsViewModel>((source, context) => new InstallSettingsViewModel(), Map);
        mapper.Define<UserSettingsModel, UserSettingsViewModel>((source, context) => new UserSettingsViewModel(), Map);
        mapper.Define<IDatabaseProviderMetadata, DatabaseProviderViewModel>((source, context) => new DatabaseProviderViewModel(), Map);
        mapper.Define<ConsentLevelModel, ConsentLevelViewModel>((source, context) => new ConsentLevelViewModel(), Map);
    }

    // Umbraco.Code.MapAll
    private static void Map(InstallSettingsModel source, InstallSettingsViewModel target, MapperContext context)
    {
        target.User = context.Map<UserSettingsViewModel>(source.UserSettings)!;
        target.Databases = context.MapEnumerable<IDatabaseProviderMetadata, DatabaseProviderViewModel>(source.DatabaseSettings);
    }

    // Umbraco.Code.MapAll
    private static void Map(UserSettingsModel source, UserSettingsViewModel target, MapperContext context)
    {
        target.MinCharLength = source.PasswordSettings.MinCharLength;
        target.MinNonAlphaNumericLength = source.PasswordSettings.MinNonAlphaNumericLength;
        target.ConsentLevels = context.MapEnumerable<ConsentLevelModel, ConsentLevelViewModel>(source.ConsentLevels);
    }

    // Umbraco.Code.MapAll
    private static void Map(IDatabaseProviderMetadata source, DatabaseProviderViewModel target, MapperContext context)
    {
        target.DefaultDatabaseName = source.DefaultDatabaseName;
        target.DisplayName = source.DisplayName;
        target.Id = source.Id;
        target.ProviderName = source.ProviderName ?? string.Empty;
        target.RequiresConnectionTest = source.RequiresConnectionTest;
        target.RequiresCredentials = source.RequiresCredentials;
        target.RequiresServer = source.RequiresServer;
        target.ServerPlaceholder = source.ServerPlaceholder ?? string.Empty;
        target.SortOrder = source.SortOrder;
        target.SupportsIntegratedAuthentication = source.SupportsIntegratedAuthentication;
        target.IsConfigured = false; // TODO: actually read this correctly
    }

    // Umbraco.Code.MapAll
    private static void Map(ConsentLevelModel source, ConsentLevelViewModel target, MapperContext context)
    {
        target.Description = source.Description;
        target.Level = source.Level;
    }
}
