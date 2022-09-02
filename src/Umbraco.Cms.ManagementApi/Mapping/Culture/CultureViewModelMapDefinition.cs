using System.Globalization;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.ViewModels.Culture;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Mapping.Culture;

public class CultureViewModelMapDefinition : IMapDefinition
{
    /// <inheritdoc/>
    public void DefineMaps(IUmbracoMapper mapper) =>
        mapper.Define<IEnumerable<CultureInfo>, PagedViewModel<CultureViewModel>>(
            (source, context) => new PagedViewModel<CultureViewModel>(), Map);

    // Umbraco.Code.MapAll
    private static void Map(IEnumerable<CultureInfo> source, PagedViewModel<CultureViewModel> target, MapperContext context)
    {
        CultureInfo[] cultureInfos = source.ToArray();
        target.Items = cultureInfos.Select(culture => new CultureViewModel { EnglishName = culture.EnglishName, Name = culture.Name });
        target.Total = cultureInfos.Length;
    }
}
