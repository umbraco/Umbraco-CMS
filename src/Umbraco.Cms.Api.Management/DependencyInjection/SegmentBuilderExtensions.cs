using Umbraco.Cms.Api.Management.Mapping.Segment;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class SegmentBuilderExtensions
{
    internal static IUmbracoBuilder AddSegment(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<SegmentMapDefinition>();

        return builder;
    }
}
