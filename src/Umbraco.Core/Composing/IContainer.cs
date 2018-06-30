using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Composing
{
    public interface IContainer
    {
        object ConcreteContainer { get; }

        object GetInstance(Type parameterType);
        T GetInstance<T>();
        T GetInstance<T>(object[] args);
        T TryGetInstance<T>();
        object TryGetInstance(Type type);

        void RegisterSingleton<T>();
        void RegisterSingleton<TService, T>() where T : TService;
        void RegisterSingleton<T>(Func<IContainer, T> factory);
        void RegisterInstance(object obj);

        void Register<TService, T>(string name) where T : TService;
        void Register<T>(Func<IContainer, T> factory);
        void Register<T, TService>(Func<IContainer, T, TService> factory);

        void RegisterFrom<T>() where T : IRegistrationBundle, new();

        T RegisterCollectionBuilder<T>();

        IDisposable BeginScope();
    }
}
