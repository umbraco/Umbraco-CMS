using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Install.NewModels;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Core.Models.Mapping;

public class InstallMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<DatabaseInstallData, DatabaseModel>((source, context) => new DatabaseModel(), Map);
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
