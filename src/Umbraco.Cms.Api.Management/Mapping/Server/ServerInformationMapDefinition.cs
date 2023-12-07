using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.Server;

public class ServerInformationMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IDictionary<string, string>, ServerInformationResponseModel>((_, _) => new ServerInformationResponseModel { Items = Array.Empty<ServerInformationItemResponseModel>() }, Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IDictionary<string, string> source, ServerInformationResponseModel target, MapperContext context)
        => target.Items = source.Select(kvp => new ServerInformationItemResponseModel
        {
            Name = kvp.Key,
            Data = kvp.Value
        });
}
