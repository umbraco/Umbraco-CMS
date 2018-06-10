using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightInject;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Core.Composing
{
    public class ContainerFactory
    {
        public object ConcreteContainer { get; private set; }
        private static ContainerFactory instance;

        // fixme: split into abstract + registration + lightinject concrete
        public static IServiceProvider CreateContainer(IServiceCollection services)
        {
            instance = new ContainerFactory();
            return instance.Create(services);
        }

        public virtual IServiceProvider Create(IServiceCollection services)
        {
            ConcreteContainer = new ServiceContainer();
            return LightInject.DependencyInjectionContainerExtensions.CreateServiceProvider((ServiceContainer)ConcreteContainer, services);
        }
    }
}
