using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Web.HealthCheck.Checks.DataIntegrity;
using LightInject;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    [DisableComponent] // is not enabled by default
    public class XmlCacheComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            // register the XML facade service
            composition.SetPublishedSnapshotService(factory => new PublishedSnapshotService(
                factory.GetInstance<ServiceContext>(),
                factory.GetInstance<IPublishedContentTypeFactory>(),
                factory.GetInstance<IScopeProvider>(),
                factory.GetInstance<CacheHelper>().RequestCache,
                factory.GetInstance<UrlSegmentProviderCollection>(),
                factory.GetInstance<IPublishedSnapshotAccessor>(),
                factory.GetInstance<IDocumentRepository>(),
                factory.GetInstance<IMediaRepository>(),
                factory.GetInstance<IMemberRepository>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<IGlobalSettings>(),
                factory.GetInstance<MainDom>()));

            // add the Xml cache health check (hidden from type finder)
            composition.HealthChecks().Add<XmlDataIntegrityHealthCheck>();
        }

        public void Initialize(IPublishedSnapshotService service)
        {
            // nothing - this just ensures that the service is created at boot time
        }
    }
}
