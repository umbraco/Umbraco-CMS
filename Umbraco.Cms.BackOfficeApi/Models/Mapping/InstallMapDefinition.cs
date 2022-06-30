using Umbraco.Cms.BackOfficeApi.Models.ViewModels.Installer;
using Umbraco.Cms.Core.Install.NewInstallSteps;
using Umbraco.Cms.Core.Install.NewModels;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.BackOfficeApi.Models.Mapping;

public class InstallMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<InstallViewModel, InstallData>((source, context) => new InstallData(), Map);
        mapper.Define<UserInstallViewModel, UserInstallData>((source, context) => new UserInstallData(), Map);
        mapper.Define<DatabaseInstallViewModel, DatabaseInstallData>((source, context) => new DatabaseInstallData(), Map);
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
}
