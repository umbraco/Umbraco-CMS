using LightInject;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.PublishedCache
{
    // resolves the IFacadeService
    // from the IServiceContainer
    // best to inject the service - this is for when it's not possible
    public class FacadeServiceResolver : ContainerSingleObjectResolver<FacadeServiceResolver, IFacadeService>
    {
        public FacadeServiceResolver(IServiceContainer container)
            : base(container)
        { }

        public IFacadeService Service => Value;
    }
}
