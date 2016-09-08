using LightInject;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.DependencyInjection;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.Checks.DataIntegrity;

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
                factory.GetInstance<IFacadeAccessor>(),
                factory.GetInstance<MainDom>()));

            // add the Xml cache health check (hidden from type finder)
            var builder = container.GetInstance<HealthCheckCollectionBuilder>();
            builder.Exclude<XmlDataIntegrityHealthCheck>();
        }

        public void Initialize(IFacadeService service)
        {
            // nothing - this just ensures that the service is created at boot time
        }
    }
}
