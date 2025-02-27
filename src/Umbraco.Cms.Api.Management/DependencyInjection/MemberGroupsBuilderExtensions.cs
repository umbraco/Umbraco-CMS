using Umbraco.Cms.Api.Management.Mapping.MemberGroup;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class MemberGroupsBuilderExtensions
{
    internal static IUmbracoBuilder AddMemberGroups(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<MemberGroupMapDefinition>();

        return builder;
    }
}
