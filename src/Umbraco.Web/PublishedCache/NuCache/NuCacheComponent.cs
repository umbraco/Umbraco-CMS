using LightInject;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class NuCacheComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(ServiceContainer container)
        {
            base.Compose(container);

            // register the NuCache facade service
            container.RegisterSingleton<IFacadeService>(factory => new FacadeService(
                new FacadeService.Options { FacadeCacheIsApplicationRequestCache = true },
                factory.GetInstance<MainDom>(),
                factory.GetInstance<IRuntimeState>(),
                factory.GetInstance<ServiceContext>(),
                factory.GetInstance<IDatabaseUnitOfWorkProvider>(),
                factory.GetInstance<IFacadeAccessor>(),
                factory.GetInstance<ILogger>()));
        }

        public void Initialize(IFacadeService service)
        {
            // nothing - this just ensures that the service is created at boot time
        }
    }
}
