using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Mapping;
using Umbraco.Web.BackOffice.Mapping;

namespace Umbraco.Extensions
{
    public static class WebMappingProfiles
    {
        public static IUmbracoBuilder ComposeWebMappingProfiles(this IUmbracoBuilder builder)
        {
            builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
                .Add<ContentMapDefinition>()
                .Add<MediaMapDefinition>()
                .Add<MemberMapDefinition>();

            builder.Services.AddTransient<CommonTreeNodeMapper>();

            return builder;
        }
    }
}
