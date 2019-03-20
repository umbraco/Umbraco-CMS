using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Composing.CompositionExtensions

{
    public static class CoreMappingProfiles
    {
        public static Composition ComposeCoreMappingProfiles(this Composition composition)
        {
            composition.RegisterUnique<Mapper>();
            composition.WithCollectionBuilder<MapperProfileCollectionBuilder>()
                .Append<IdentityMapperProfile>();
            return composition;
        }
    }
}
