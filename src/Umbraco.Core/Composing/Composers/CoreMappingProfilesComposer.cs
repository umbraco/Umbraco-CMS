using AutoMapper;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Composing.Composers

{
    public static class CoreMappingProfilesComposer
    {
        public static IContainer ComposeCoreMappingProfiles(this IContainer container)
        {
            container.Register<Profile, IdentityMapperProfile>();
            return container;
        }
    }
}
