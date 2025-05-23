using System.Globalization;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Api.Management.ViewModels.Culture;

namespace Umbraco.Cms.Api.Management.Mapping.Culture;

/// <inheritdoc />
public class CultureViewModelMapDefinition : IMapDefinition
{
    /// <inheritdoc/>
    public void DefineMaps(IUmbracoMapper mapper) => mapper.Define<CultureInfo, CultureReponseModel>((source, context) => new CultureReponseModel(), Map);

    // Umbraco.Code.MapAll
    private static void Map(CultureInfo source, CultureReponseModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.EnglishName = source.EnglishName;
    }
}
