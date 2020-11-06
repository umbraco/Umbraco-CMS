using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Composing;
using Umbraco.Core.Mapping;
using Umbraco.Web.BackOffice.Mapping;

namespace Umbraco.Extensions
{
    public static class WebMappingProfiles
    {
        public static Composition ComposeWebMappingProfiles(this Composition composition)
        {
            composition.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
                .Add<ContentMapDefinition>()
                .Add<MediaMapDefinition>()
                .Add<MemberMapDefinition>();

            composition.Services.AddTransient<CommonTreeNodeMapper>();

            return composition;
        }
    }
}
