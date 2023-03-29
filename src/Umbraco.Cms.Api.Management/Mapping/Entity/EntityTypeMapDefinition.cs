using Umbraco.Cms.Api.Management.ViewModels.Entity;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Entity;

public class EntityTypeMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<ILanguage, EntityResponseModel>((_, _) => new EntityResponseModel(), Map);
    }

    // Umbraco.Code.MapAll
    private static void Map(ILanguage source, EntityResponseModel target, MapperContext context)
    {
        target.DisplayName = source.CultureName;
        target.Id = source.IsoCode;
    }
}
