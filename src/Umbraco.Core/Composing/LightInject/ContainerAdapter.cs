using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightInject;

namespace Umbraco.Core.Composing.LightInject
{
    public class ContainerAdapter : IContainer
    {
        private readonly IServiceContainer container;

        public object ConcreteContainer => container;

        public void RegisterSingleton<T>(Func<IContainer, T> factory)
        {
            container.RegisterSingleton(f => factory(this));
        }

        public void Register<T>(Func<IContainer, T> factory)
        {
            container.Register(f => factory(this));
        }

        public T RegisterCollectionBuilder<T>()
        {
            return container.RegisterCollectionBuilder<T>();
        }

        public ContainerAdapter(IServiceContainer container)
        {
            this.container = container;
        }

        public T TryGetInstance<T>()
        {
            return container.TryGetInstance<T>();
        }

        public T GetInstance<T>()
        {
            return container.GetInstance<T>();
        }
    }
}
