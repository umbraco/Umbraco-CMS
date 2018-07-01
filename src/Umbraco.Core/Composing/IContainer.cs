using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Umbraco.Core.Composing
{
    public interface IContainer : IDisposable
    {
        object ConcreteContainer { get; }

        IEnumerable<T> GetAllInstances<T>();
        object GetInstance(Type parameterType);
        T GetInstance<T>(string name);
        T GetInstance<T>();
        T GetInstance<T>(object[] args);
        T TryGetInstance<T>();
        object TryGetInstance(Type type);

        void RegisterAuto(Type type);

        void RegisterSingleton<T>();
        void RegisterSingleton<TService, T>() where T : TService;
        void RegisterSingleton<T>(Func<IContainer, T> factory);
        void RegisterSingleton<T>(Func<IContainer, T> factory, string name);
        void RegisterInstance(object obj);

        void Register<TService, T>() where T : TService;
        void Register<TService, T>(string name) where T : TService;
        void Register<TService, T>(Lifetime lifetime) where T : TService;
        void Register<T>();
        void Register<T>(Lifetime lifetime);
        void Register<T>(Func<IContainer, T> factory);
        void Register<T>(Func<IContainer, T> factory, Lifetime lifetime);
        void Register<TParam, TService>(Func<IContainer, TParam, TService> factory);

        void RegisterConstructorDependency<T>(Func<IContainer, T> factory);

        void RegisterOrdered(Type serviceType, Type[] implementingTypes, Func<Type, Lifetime> lifetimeFactory);

        void RegisterFrom<T>() where T : IRegistrationBundle, new();

        T RegisterCollectionBuilder<T>();

        IDisposable BeginScope();
    }

    public enum Lifetime
    {
        // fixme - What's transient in LightInject?
        // Transient,
        PerRequest,
        PerScope,
        Singleton
    }
}
