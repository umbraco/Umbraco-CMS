using System.Collections.Generic;
using LightInject;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Core.DependencyInjection
{
    public sealed class ServicesCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry container)
        {
            container.Register<BasePublishingStrategy, PublishingStrategy>(new PerContainerLifetime());

            //These will be replaced by the web boot manager when running in a web context
            container.Register<IEventMessagesFactory, TransientMessagesFactory>();

            container.Register<ServiceContext>(factory => new ServiceContext(
                factory.GetInstance<RepositoryFactory>(),
                factory.GetInstance<IDatabaseUnitOfWorkProvider>(),
                factory.GetInstance<IUnitOfWorkProvider>(),
                factory.GetInstance<BasePublishingStrategy>(),
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<IEventMessagesFactory>(),
                factory.GetInstance<IEnumerable<IUrlSegmentProvider>>()), 
                new PerContainerLifetime());

        }
    }
}