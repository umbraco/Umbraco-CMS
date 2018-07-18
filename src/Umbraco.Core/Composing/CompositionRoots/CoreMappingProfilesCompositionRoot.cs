using LightInject;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Composing.CompositionRoots
{
    public sealed class CoreMappingProfilesCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry container)
        {
            container.Register<IdentityMapperProfile>();
        }
    }
}
