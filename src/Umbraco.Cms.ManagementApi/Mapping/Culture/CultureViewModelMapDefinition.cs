using System.Globalization;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.ViewModels.Culture;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Mapping.Culture;

/// <inheritdoc />
public class CultureViewModelMapDefinition : IMapDefinition
{
    /// <inheritdoc/>
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IEnumerable<CultureInfo>, PagedViewModel<CultureViewModel>>((source, context) => new PagedViewModel<CultureViewModel>(), Map);
        mapper.Define<CultureInfo, CultureViewModel>((source, context) => new CultureViewModel(), Map);
    }

    // Umbraco.Code.MapAll
    private static void Map(CultureInfo source, CultureViewModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.EnglishName = source.EnglishName;
    }

    // Umbraco.Code.MapAll
    private static void Map(IEnumerable<CultureInfo> source, PagedViewModel<CultureViewModel> target, MapperContext context)
    {
        CultureInfo[] cultureInfos = source.ToArray();
        target.Items = context.MapEnumerable<CultureInfo, CultureViewModel>(cultureInfos);
        target.Total = cultureInfos.Length;
    }
}
