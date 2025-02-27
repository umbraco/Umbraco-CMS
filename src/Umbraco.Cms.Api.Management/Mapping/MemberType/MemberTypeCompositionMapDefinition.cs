using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.MemberType;

public class MemberTypeCompositionMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IMemberType, MemberTypeCompositionResponseModel>(
            (_, _) => new MemberTypeCompositionResponseModel(), Map);

    // Umbraco.Code.MapAll
    private static void Map(IMemberType source, MemberTypeCompositionResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
        target.Icon = source.Icon ?? string.Empty;
    }
}
