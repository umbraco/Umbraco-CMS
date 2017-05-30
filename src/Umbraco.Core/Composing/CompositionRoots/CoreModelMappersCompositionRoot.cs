using LightInject;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Composing.CompositionRoots
{
    public sealed class CoreModelMappersCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry container)
        {
            container.Register<IdentityModelMappings>();
        }
    }
}