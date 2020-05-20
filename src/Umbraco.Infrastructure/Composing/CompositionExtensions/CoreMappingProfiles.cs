using Umbraco.Core.BackOffice;
using Umbraco.Core.Mapping;

namespace Umbraco.Core.Composing.CompositionExtensions

{
    public static class CoreMappingProfiles
    {
        /// <summary>
        /// Registers the core Umbraco mapper definitions
        /// </summary>
        /// <param name="composition"></param>
        /// <returns></returns>
        public static Composition ComposeCoreMappingProfiles(this Composition composition)
        {
            composition.RegisterUnique<UmbracoMapper>();

            composition.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
                .Add<IdentityMapDefinition>();

            return composition;
        }
    }
}
