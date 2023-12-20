using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.Server;

public class ServerInformationMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IDictionary<string, string>, ServerTroubleshootingResponseModel>((_, _) => new ServerTroubleshootingResponseModel { Items = Array.Empty<ServerTroubleshootingItemResponseModel>() }, Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IDictionary<string, string> source, ServerTroubleshootingResponseModel target, MapperContext context)
        => target.Items = source.Select(kvp => new ServerTroubleshootingItemResponseModel
        {
            Name = kvp.Key,
            Data = kvp.Value
        });
}
