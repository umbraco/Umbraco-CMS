using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Composing;
using Umbraco.Core.Mapping;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.Composing.CompositionExtensions
{
    public static class WebMappingProfiles
    {
        public static Composition ComposeWebMappingProfiles(this Composition composition)
        {
            // TODO: All/Most of these should be moved to ComposeCoreMappingProfiles which requires All/most of the
            // definitions to be moved to core

            composition.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
                .Add<ContentMapDefinition>()
                .Add<MediaMapDefinition>()
                .Add<MemberMapDefinition>()
                ;

            composition.Register<CommonMapper>();
            composition.Register<CommonTreeNodeMapper>();
            composition.Register<MemberTabsAndPropertiesMapper>();

            return composition;
        }
    }
}
