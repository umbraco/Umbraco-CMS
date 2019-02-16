using AutoMapper;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Composing.CompositionExtensions

{
    public static class CoreMappingProfiles
    {
        public static Composition ComposeCoreMappingProfiles(this Composition composition)
        {
            composition.Register<Profile, IdentityMapperProfile>();
            return composition;
        }
    }
}
