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
        Dictionary<Lifetime, Func<ILifetime>> lifetimes = new Dictionary<Lifetime, Func<ILifetime>>
        { 
            { Lifetime.PerRequest, () => new PerRequestLifeTime() },
            { Lifetime.PerScope, () => new PerScopeLifetime() },
            { Lifetime.Singleton, () => new PerContainerLifetime() }
        };

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

        public IEnumerable<T> GetAllInstances<T>()
        {
            return container.GetAllInstances<T>();
        }

        public T GetInstance<T>()
        {
            return container.GetInstance<T>();
        }

        public T GetInstance<T>(string name)
        {
            return container.GetInstance<T>(name);
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

        public void RegisterAuto(Type type)
        {
            container.RegisterAuto(type);
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

        public void RegisterSingleton<T>(Func<IContainer, T> factory, string name)
        {
            container.RegisterSingleton(f => factory(this), name);
        }

        public void RegisterInstance(object obj)
        {
            container.RegisterInstance(obj);
        }
        
        public void Register<T>(Func<IContainer, T> factory)
        {
            container.Register(f => factory(this));
        }

        public void Register<T>(Func<IContainer, T> factory, Lifetime lifetime)
        {
            container.Register(f => factory(this), lifetimes[lifetime]());
        }

        public void Register<T>()
        {
            container.Register<T>();
        }

        public void Register<T>(Lifetime lifetime)
        {
            container.Register<T>(lifetimes[lifetime]());
        }

        public void Register<TService, T>()
            where T : TService
        {
            container.Register<TService, T>();
        }

        public void Register<TService, T>(string name)
            where T : TService
        {
            container.Register<TService, T>(name);
        }

        public void Register<TService, T>(Lifetime lifetime)
            where T : TService
        {
            container.Register<TService, T>(lifetimes[lifetime]());
        }

        public void Register<T, TService>(Func<IContainer, T, TService> factory)
        {
            container.Register<T, TService>((f, x) => factory(this, x));
        }

        public void RegisterOrdered(Type serviceType, Type[] implementingTypes, Func<Type, Lifetime> lifetimeFactory)
        {
            container.RegisterOrdered(serviceType, implementingTypes, t => lifetimes[lifetimeFactory(t)]()); 
        }

        public void RegisterFrom<T>() where T : IRegistrationBundle, new()
        {
            var it = new T();
            it.Compose(this);
        }

        public void RegisterConstructorDependency<T>(Func<IContainer, T> factory)
        {
            // fixme - abstract parameterinfo somehow?
            container.RegisterConstructorDependency((f, i) => factory(this));
        }

        public T RegisterCollectionBuilder<T>()
        {
            return container.RegisterCollectionBuilder<T>();
        }

        public IDisposable BeginScope()
        {
            return container.BeginScope();
        }

        public void Dispose()
        {
            container.Dispose();
        }
    }
}
