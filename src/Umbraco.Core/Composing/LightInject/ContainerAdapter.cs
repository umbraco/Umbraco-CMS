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

        public ContainerAdapter(IServiceContainer container)
        {
            this.container = container;
        }

        public object GetInstance(Type type)
        {
            return container.GetInstance(type);
        }

        public T GetInstance<T>()
        {
            return container.GetInstance<T>();
        }

        public T GetInstance<T>(object[] args)
        {
            return (T)container.GetInstance(typeof(T), args);
        }

        public T TryGetInstance<T>()
        {
            return container.TryGetInstance<T>();
        }

        public object TryGetInstance(Type type)
        {
            return container.TryGetInstance(type);
        }

        public void RegisterSingleton<TService, T>()
            where T : TService
        {
            container.RegisterSingleton<TService, T>();
        }

        public void RegisterSingleton<T>()
        {
            container.RegisterSingleton<T>();
        }

        public void RegisterSingleton<T>(Func<IContainer, T> factory)
        {
            container.RegisterSingleton(f => factory(this));
        }

        public void RegisterInstance(object obj)
        {
            container.RegisterInstance(obj);
        }
        
        public void Register<T>(Func<IContainer, T> factory)
        {
            container.Register(f => factory(this));
        }

        public void Register<TService, T>(string name)
            where T : TService
        {
            container.Register<TService, T>(name);
        }

        public void Register<T, TService>(Func<IContainer, T, TService> factory)
        {
            container.Register<T, TService>((f, x) => factory(this, x));
        }

        public void RegisterFrom<T>() where T : IRegistrationBundle, new()
        {
            var it = new T();
            it.Compose(this);
        }

        public T RegisterCollectionBuilder<T>()
        {
            return container.RegisterCollectionBuilder<T>();
        }

        public IDisposable BeginScope()
        {
            return container.BeginScope();
        }

    }
}
