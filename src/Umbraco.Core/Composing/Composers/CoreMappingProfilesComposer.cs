using LightInject;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Composing.Composers

{
    public static class CoreMappingProfilesComposer
    {
        public static IServiceRegistry ComposeCoreMappingProfiles(this IServiceRegistry registry)
        {
            registry.Register<IdentityMapperProfile>();
            return registry;
        }
    }
}
