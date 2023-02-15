using Umbraco.Cms.Api.Management.Mapping.Package;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class PackageBuilderExtensions
{
    internal static IUmbracoBuilder AddPackages(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<PackageManifestViewModelMapDefinition>();

        return builder;
    }
}
