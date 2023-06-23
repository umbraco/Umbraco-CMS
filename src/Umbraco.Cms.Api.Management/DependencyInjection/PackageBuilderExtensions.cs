using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Package;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class PackageBuilderExtensions
{
    internal static IUmbracoBuilder AddPackages(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IPackageDefinitionFactory, PackageDefinitionFactory>();

        builder
            .WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<PackageViewModelMapDefinition>()
            .Add<PackageManifestViewModelMapDefinition>();

        return builder;
    }
}
