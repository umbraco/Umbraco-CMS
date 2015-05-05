using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core.LightInject;
using Umbraco.Core.Logging;

namespace Umbraco.Core.ObjectResolution
{

    /// <summary>
    /// A lazy many objects resolver that uses IoC
    /// </summary>
    /// <typeparam name="TResolver"></typeparam>
    /// <typeparam name="TResolved"></typeparam>
    public abstract class ContainerLazyManyObjectsResolver<TResolver, TResolved> : LazyManyObjectsResolverBase<TResolver, TResolved>
        where TResolved : class
        where TResolver : ResolverBase
    {
        private IServiceContainer _container;
        private object _locker = new object();
        private bool _isInitialized = false;

        internal ContainerLazyManyObjectsResolver(IServiceContainer container, ILogger logger, Func<IEnumerable<Type>> typeListProducerList, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : base(logger, typeListProducerList, scope)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;

            //Register ourselves in the case that a resolver instance should be injected someplace
            _container.Register<TResolver>(factory => (TResolver)(object)this);
        }

        /// <summary>
        /// Creates the instances from IoC
        /// </summary>
        /// <returns>A list of objects of type <typeparamref name="TResolved"/>.</returns>
        protected override IEnumerable<TResolved> CreateValues(ObjectLifetimeScope scope)
        {
            //Before we can do anything, the first time this happens we need to setup the container with the resolved types
            LazyInitializer.EnsureInitialized(ref _container, ref _isInitialized, ref _locker, () =>
            {
                foreach (var type in InstanceTypes)
                {
                    _container.Register(type, GetLifetime(LifetimeScope));
                }
                return _container;
            });

            //NOTE: we ignore scope because objects are registered under this scope and not build based on the scope.

            return _container.GetAllInstances<TResolved>();
        }

        /// <summary>
        /// Convert the ObjectLifetimeScope to ILifetime
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        private static ILifetime GetLifetime(ObjectLifetimeScope scope)
        {
            switch (scope)
            {
                case ObjectLifetimeScope.HttpRequest:
                    return new PerRequestLifeTime();
                case ObjectLifetimeScope.Application:
                    return new PerContainerLifetime();
                case ObjectLifetimeScope.Transient:
                default:
                    return null;
            }
        }
    }
}