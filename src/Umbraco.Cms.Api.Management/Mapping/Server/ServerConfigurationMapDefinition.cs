using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.Server;

public class ServerConfigurationMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IDictionary<string, string>, ServerTroubleshootingResponseModel>((_, _) => new ServerTroubleshootingResponseModel { Items = Array.Empty<ServerConfigurationItemResponseModel>() }, Map);
        mapper.Define<IDictionary<string, object>, ServerInformationResponseModel>((_, _) => new ServerInformationResponseModel() { Items = Array.Empty<ServerConfigurationItemResponseModel>() }, Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IDictionary<string, string> source, ServerTroubleshootingResponseModel target, MapperContext context)
        => target.Items = source.Select(kvp => new ServerConfigurationItemResponseModel
        {
            Name = kvp.Key,
            Data = kvp.Value
        });

    // Umbraco.Code.MapAll
    private void Map(IDictionary<string, object> source, ServerInformationResponseModel target, MapperContext context)
        => target.Items = source.Select(kvp => new ServerConfigurationItemResponseModel
        {
            Name = kvp.Key,
            Data = kvp.Value
        });

}
