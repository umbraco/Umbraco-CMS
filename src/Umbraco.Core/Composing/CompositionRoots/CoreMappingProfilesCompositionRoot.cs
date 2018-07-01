using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Composing.CompositionRoots
{
    public sealed class CoreMappingProfilesCompositionRoot : IRegistrationBundle
    {
        public void Compose(IContainer container)
        {
            container.Register<IdentityProfile>();
        }
    }
}
