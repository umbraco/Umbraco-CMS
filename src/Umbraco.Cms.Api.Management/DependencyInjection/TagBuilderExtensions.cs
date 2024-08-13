using Umbraco.Cms.Api.Management.Mapping.Tag;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class TagBuilderExtensions
{
    internal static IUmbracoBuilder AddTags(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<TagResponseModelMapDefinition>();

        return builder;
    }
}
