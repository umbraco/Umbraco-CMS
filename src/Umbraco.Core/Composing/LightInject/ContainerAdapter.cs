using System;
using LightInject;

namespace Umbraco.Core.Composing.LightInject
{
    /// <summary>
    /// Implements <see cref="IContainer"/> for LightInject.
    /// </summary>
    public class ContainerAdapter : IContainer // fixme rename LightInjectContainer?
    {
        private readonly IServiceContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerAdapter"/> with a LightInject container.
        /// </summary>
        public ContainerAdapter(IServiceContainer container)
        {
            _container = container;
        }

        // fixme
        public object ConcreteContainer => _container;

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

        /// <inheritdoc />
        public T TryGetInstance<T>()
            => _container.TryGetInstance<T>();

        /// <inheritdoc />
        public T GetInstance<T>()
            => _container.GetInstance<T>();

        /// <inheritdoc />
        public T GetInstance<T>(object[] args)
            => (T) _container.GetInstance(typeof(T), args);

        /// <inheritdoc />
        public object GetInstance(Type type)
            => _container.GetInstance(type);
    }
}
