using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.ManagementApi.ViewModels.Installer;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.Cms.ManagementApi.Mapping.Installer;

public class InstallerViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<InstallViewModel, InstallData>((source, context) => new InstallData(), Map);
        mapper.Define<UserInstallViewModel, UserInstallData>((source, context) => new UserInstallData(), Map);
        mapper.Define<DatabaseInstallViewModel, DatabaseInstallData>((source, context) => new DatabaseInstallData(), Map);
        mapper.Define<DatabaseInstallViewModel, DatabaseModel>((source, context) => new DatabaseModel(), Map);
        mapper.Define<DatabaseInstallData, DatabaseModel>((source, context) => new DatabaseModel(), Map);
        mapper.Define<InstallSettingsModel, InstallSettingsViewModel>((source, context) => new InstallSettingsViewModel(), Map);
        mapper.Define<UserSettingsModel, UserSettingsViewModel>((source, context) => new UserSettingsViewModel(), Map);
        mapper.Define<IDatabaseProviderMetadata, DatabaseSettingsModel>((source, context) => new DatabaseSettingsModel(), Map);
        mapper.Define<DatabaseSettingsModel, DatabaseSettingsViewModel>((source, context) => new DatabaseSettingsViewModel(), Map);
        mapper.Define<ConsentLevelModel, ConsentLevelViewModel>((source, context) => new ConsentLevelViewModel(), Map);
        mapper.Define<UpgradeSettingsModel, UpgradeSettingsViewModel>((source, context) => new UpgradeSettingsViewModel(), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(UpgradeSettingsModel source, UpgradeSettingsViewModel target, MapperContext context)
    {
        target.CurrentState = source.CurrentState;
        target.NewState = source.NewState;
        target.NewVersion = source.NewVersion.ToString();
        target.OldVersion = source.OldVersion.ToString();
    }

    // Umbraco.Code.MapAll
    private void Map(DatabaseInstallViewModel source, DatabaseModel target, MapperContext context)
    {
        target.ConnectionString = source.ConnectionString;
        target.DatabaseName = source.Name ?? string.Empty;
        target.DatabaseProviderMetadataId = source.Id;
        target.IntegratedAuth = source.UseIntegratedAuthentication;
        target.Login = source.Username;
        target.Password = source.Password;
        target.ProviderName = source.ProviderName;
        target.Server = source.Server!;
    }

    // Umbraco.Code.MapAll
    private static void Map(InstallViewModel source, InstallData target, MapperContext context)
    {
        target.TelemetryLevel = source.TelemetryLevel;
        target.User = context.Map<UserInstallData>(source.User)!;
        target.Database = context.Map<DatabaseInstallData>(source.Database)!;
    }

    // Umbraco.Code.MapAll
    private static void Map(UserInstallViewModel source, UserInstallData target, MapperContext context)
    {
        target.Email = source.Email;
        target.Name = source.Name;
        target.Password = source.Password;
        target.SubscribeToNewsletter = source.SubscribeToNewsletter;
    }

    // Umbraco.Code.MapAll
    private static void Map(DatabaseInstallViewModel source, DatabaseInstallData target, MapperContext context)
    {
        target.Id = source.Id;
        target.ProviderName = source.ProviderName;
        target.Server = source.Server;
        target.Name = source.Name;
        target.Username = source.Username;
        target.Password = source.Password;
        target.UseIntegratedAuthentication = source.UseIntegratedAuthentication;
        target.ConnectionString = source.ConnectionString;
    }

    // Umbraco.Code.MapAll
    private static void Map(DatabaseInstallData source, DatabaseModel target, MapperContext context)
    {
        target.ConnectionString = source.ConnectionString;
        target.DatabaseName = source.Name ?? string.Empty;
        target.DatabaseProviderMetadataId = source.Id;
        target.IntegratedAuth = source.UseIntegratedAuthentication;
        target.Login = source.Username;
        target.Password = source.Password;
        target.ProviderName = source.ProviderName;
        target.Server = source.Server!;
    }

    // Umbraco.Code.MapAll
    private static void Map(InstallSettingsModel source, InstallSettingsViewModel target, MapperContext context)
    {
        target.User = context.Map<UserSettingsViewModel>(source.UserSettings)!;
        target.Databases = context.MapEnumerable<DatabaseSettingsModel, DatabaseSettingsViewModel>(source.DatabaseSettings);
    }

    // Umbraco.Code.MapAll
    private static void Map(UserSettingsModel source, UserSettingsViewModel target, MapperContext context)
    {
        target.MinCharLength = source.PasswordSettings.MinCharLength;
        target.MinNonAlphaNumericLength = source.PasswordSettings.MinNonAlphaNumericLength;
        target.ConsentLevels = context.MapEnumerable<ConsentLevelModel, ConsentLevelViewModel>(source.ConsentLevels);
    }

    // Umbraco.Code.MapAll
    private static void Map(IDatabaseProviderMetadata source, DatabaseSettingsModel target, MapperContext context)
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
        target.IsConfigured = false; // Defaults to false, we'll set this to true if needed,
    }

    // Umbraco.Code.MapAll
    private static void Map(DatabaseSettingsModel source, DatabaseSettingsViewModel target, MapperContext context)
    {
        target.DefaultDatabaseName = source.DefaultDatabaseName;
        target.DisplayName = source.DisplayName;
        target.Id = source.Id;
        target.IsConfigured = source.IsConfigured;
        target.ProviderName = source.ProviderName;
        target.RequiresConnectionTest = source.RequiresConnectionTest;
        target.RequiresCredentials = source.RequiresCredentials;
        target.RequiresServer = source.RequiresServer;
        target.ServerPlaceholder = source.ServerPlaceholder;
        target.SortOrder = source.SortOrder;
        target.SupportsIntegratedAuthentication = source.SupportsIntegratedAuthentication;
    }

    // Umbraco.Code.MapAll
    private static void Map(ConsentLevelModel source, ConsentLevelViewModel target, MapperContext context)
    {
        target.Description = source.Description;
        target.Level = source.Level;
    }
}
