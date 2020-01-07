using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Mapping;

namespace Umbraco.Web.Composing.CompositionExtensions

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
