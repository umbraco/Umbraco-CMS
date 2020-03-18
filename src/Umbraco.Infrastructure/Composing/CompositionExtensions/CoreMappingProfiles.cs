using Umbraco.Core.Mapping;

namespace Umbraco.Core.Composing.CompositionExtensions

{
    public static class CoreMappingProfiles
    {
        public static Composition ComposeCoreMappingProfiles(this Composition composition)
        {
            composition.RegisterUnique<UmbracoMapper>();

            return composition;
        }
    }
}
