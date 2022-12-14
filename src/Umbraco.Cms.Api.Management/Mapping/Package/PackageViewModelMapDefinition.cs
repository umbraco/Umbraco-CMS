using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Api.Management.Mapping.Package;

public class PackageViewModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<PackageDefinition, PackageDefinitionViewModel>(
            (source, context) => new PackageDefinitionViewModel()
            {
                Name = string.Empty,
                PackagePath = string.Empty
            },
            Map);
        mapper.Define<IEnumerable<PackageDefinition>, PagedViewModel<PackageDefinitionViewModel>>((source, context) => new PagedViewModel<PackageDefinitionViewModel>(), Map);
    }

    private static void Map(PackageDefinition source, PackageDefinitionViewModel target, MapperContext context)
    {
        target.Key = source.PackageId;
        target.Name = source.Name;
        target.PackagePath = source.PackagePath;
        target.ContentNodeId = source.ContentNodeId;
        target.ContentLoadChildNodes = source.ContentLoadChildNodes;
        target.MediaKeys = source.MediaUdis.Select(x => x.Guid).ToList();
        target.MediaLoadChildNodes = source.MediaLoadChildNodes;
        target.DocumentTypes = source.DocumentTypes;
        target.MediaTypes = source.MediaTypes;
        target.DataTypes = source.DataTypes;
        target.Templates = source.Templates;
        target.PartialViews = source.PartialViews;
        target.Stylesheets = source.Stylesheets;
        target.Scripts = source.Scripts;
        target.Languages = source.Languages;
        target.DictionaryItems = source.DictionaryItems;
    }

    private static void Map(IEnumerable<PackageDefinition> source, PagedViewModel<PackageDefinitionViewModel> target, MapperContext context)
    {
        target.Items = context.MapEnumerable<PackageDefinition, PackageDefinitionViewModel>(source);
        target.Total = source.Count();
    }
}
