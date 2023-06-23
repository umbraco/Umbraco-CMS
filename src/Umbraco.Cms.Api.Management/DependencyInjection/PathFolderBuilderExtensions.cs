using Umbraco.Cms.Api.Management.Mapping.Folder;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class PathFolderBuilderExtensions
{
    internal static IUmbracoBuilder AddPathFolders(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<PathFolderViewModelMapDefinition>();

        return builder;
    }
}
