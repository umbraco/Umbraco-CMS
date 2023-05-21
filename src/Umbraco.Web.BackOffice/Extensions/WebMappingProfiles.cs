using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Web.BackOffice.Mapping;

namespace Umbraco.Extensions;

public static class WebMappingProfiles
{
    public static IUmbracoBuilder AddWebMappingProfiles(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<ContentMapDefinition>()
            .Add<MediaMapDefinition>()
            .Add<MemberMapDefinition>();

        builder.Services.AddTransient<CommonTreeNodeMapper>();

        return builder;
    }
}
