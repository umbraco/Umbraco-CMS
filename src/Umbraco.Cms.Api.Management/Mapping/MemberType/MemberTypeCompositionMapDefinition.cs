using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.MemberType;

    /// <summary>
    /// Provides mapping configuration for composing member types within the Umbraco CMS management API.
    /// </summary>
public class MemberTypeCompositionMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures object-object mapping definitions between <see cref="IMemberType"/> and <see cref="MemberTypeCompositionResponseModel"/> for member type composition responses.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used to register the mapping configuration.</param>
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
