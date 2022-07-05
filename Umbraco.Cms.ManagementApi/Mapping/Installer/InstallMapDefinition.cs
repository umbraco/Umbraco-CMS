using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.ViewModels.Installer;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.Cms.ManagementApi.Mapping.Installer;

public class InstallMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<InstallViewModel, InstallData>((source, context) => new InstallData(), Map);
        mapper.Define<UserInstallViewModel, UserInstallData>((source, context) => new UserInstallData(), Map);
        mapper.Define<DatabaseInstallViewModel, DatabaseInstallData>((source, context) => new DatabaseInstallData(), Map);
        mapper.Define<DatabaseInstallData, DatabaseModel>((source, context) => new DatabaseModel(), Map);
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
        target.Server = source.Server;
    }
}
