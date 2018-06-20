using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Composing
{
    public interface IContainer
    {
        T TryGetInstance<T>();
        T GetInstance<T>();
        object GetInstance(Type parameterType);
        object ConcreteContainer { get; }
        void RegisterSingleton<T>(Func<IContainer, T> factory);
        void Register<T>(Func<IContainer, T> factory);
        T RegisterCollectionBuilder<T>();
        T GetInstance<T>(params object[] args);
    }
}
