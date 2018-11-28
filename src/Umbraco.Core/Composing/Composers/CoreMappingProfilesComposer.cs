using AutoMapper;
using Umbraco.Core.Components;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Composing.Composers

{
    public static class CoreMappingProfilesComposer
    {
        public static Composition ComposeCoreMappingProfiles(this Composition composition)
        {
            composition.Register<Profile, IdentityMapperProfile>();
            return composition;
        }
    }
}
