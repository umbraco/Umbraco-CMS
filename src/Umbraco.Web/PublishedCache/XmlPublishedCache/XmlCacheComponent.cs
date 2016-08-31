using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    [DisableComponent] // is not enabled by default
    public class XmlCacheComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(ServiceContainer container)
        {
            base.Compose(container);

            // register the XML facade service
            container.RegisterSingleton<IFacadeService>(factory => new FacadeService(
                factory.GetInstance<ServiceContext>(),
                factory.GetInstance<IDatabaseUnitOfWorkProvider>(),
                factory.GetInstance<CacheHelper>().RequestCache,
                factory.GetAllInstances<IUrlSegmentProvider>(),
                factory.GetInstance<IFacadeAccessor>()));
        }

        public void Initialize(IFacadeService service)
        {
            // nothing - this just ensures that the service is created at boot time
        }
    }
}
