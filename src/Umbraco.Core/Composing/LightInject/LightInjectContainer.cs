using System;
using LightInject;

namespace Umbraco.Core.Composing.LightInject
{
    /// <summary>
    /// Implements <see cref="IContainer"/> with LightInject.
    /// </summary>
    public class LightInjectContainer : IContainer
    {
        private readonly IServiceContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectContainer"/> with a LightInject container.
        /// </summary>
        public LightInjectContainer(IServiceContainer container)
        {
            _container = container;
        }

        /// <inheritdoc />
        public object ConcreteContainer => _container;

        /// <inheritdoc />
        public object GetInstance(Type type)
            => _container.GetInstance(type);

        /// <inheritdoc />
        public object GetInstance(Type type, object[] args)
            => _container.GetInstance(type, args);

        /// <inheritdoc />
        public object TryGetInstance(Type type)
            => _container.TryGetInstance(type);

        /// <inheritdoc />
        public void RegisterSingleton<T>(Func<IContainer, T> factory)
            => _container.RegisterSingleton(f => factory(this));

        /// <inheritdoc />
        public void Register<T>(Func<IContainer, T> factory)
            => _container.Register(f => factory(this));

        /// <inheritdoc />
        public void Register<T, TService>(Func<IContainer, T, TService> factory)
            => _container.Register<T, TService>((f, x) => factory(this, x));

        /// <inheritdoc />
        public T RegisterCollectionBuilder<T>()
            => _container.RegisterCollectionBuilder<T>();
    }
}
