using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using LightInject;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class NuCacheComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            // register the NuCache published snapshot service
            composition.SetPublishedSnapshotService(factory => new PublishedSnapshotService(
                new PublishedSnapshotService.Options(),
                factory.GetInstance<MainDom>(),
                factory.GetInstance<IRuntimeState>(),
                factory.GetInstance<ServiceContext>(),
                factory.GetInstance<IPublishedContentTypeFactory>(),
                factory.GetInstance<IPublishedSnapshotAccessor>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<IScopeProvider>(),
                factory.GetInstance<IDocumentRepository>(),
                factory.GetInstance<IMediaRepository>(),
                factory.GetInstance<IMemberRepository>()));

            // add the NuCache health check (hidden from type finder)
            // todo - no NuCache health check yet
            //composition.HealthChecks().Add<NuCacheIntegrityHealthCheck>();
        }

        public void Initialize(IPublishedSnapshotService service)
        {
            // nothing - this just ensures that the service is created at boot time
        }
    }
}
