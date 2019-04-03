using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Composing.CompositionExtensions

{
    public static class CoreMappingProfiles
    {
        public static Composition ComposeCoreMappingProfiles(this Composition composition)
        {
            composition.RegisterUnique<UmbracoMapper>();
            composition.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
                .Add<IdentityMapDefinition>();
            return composition;
        }
    }
}
