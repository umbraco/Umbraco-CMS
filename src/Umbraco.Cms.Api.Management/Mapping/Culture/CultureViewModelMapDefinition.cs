using System.Globalization;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Api.Management.ViewModels.Culture;

namespace Umbraco.Cms.Api.Management.Mapping.Culture;

/// <inheritdoc />
public class CultureViewModelMapDefinition : IMapDefinition
{
    /// <inheritdoc/>
    public void DefineMaps(IUmbracoMapper mapper) => mapper.Define<CultureInfo, CultureViewModel>((source, context) => new CultureViewModel(), Map);

    // Umbraco.Code.MapAll
    private static void Map(CultureInfo source, CultureViewModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.EnglishName = source.EnglishName;
    }
}
