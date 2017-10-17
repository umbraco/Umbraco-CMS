using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Web.HealthCheck.Checks.DataIntegrity;
using LightInject;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    [DisableComponent] // is not enabled by default
    public class XmlCacheComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            // register the XML facade service
            composition.SetFacadeService(factory => new FacadeService(
                factory.GetInstance<ServiceContext>(),
                factory.GetInstance<IPublishedContentTypeFactory>(),
                factory.GetInstance<IScopeProvider>(),
                factory.GetInstance<IScopeUnitOfWorkProvider>(),
                factory.GetInstance<CacheHelper>().RequestCache,
                factory.GetInstance<UrlSegmentProviderCollection>(),
                factory.GetInstance<IFacadeAccessor>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<MainDom>()));

            // add the Xml cache health check (hidden from type finder)
            composition.HealthChecks().Add<XmlDataIntegrityHealthCheck>();
        }

        public void Initialize(IFacadeService service)
        {
            // nothing - this just ensures that the service is created at boot time
        }
    }
}
