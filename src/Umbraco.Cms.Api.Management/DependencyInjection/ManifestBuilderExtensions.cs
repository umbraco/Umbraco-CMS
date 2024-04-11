using Umbraco.Cms.Api.Management.Mapping.Manifest;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ManifestBuilderExtensions
{
    internal static IUmbracoBuilder AddManifests(this IUmbracoBuilder builder)
    {
        builder
            .WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<ManifestViewModelMapDefinition>();

        return builder;
    }
}
