using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LightInject;
using Umbraco.Core.Logging;

namespace Umbraco.Core.ObjectResolution
{
    /// <summary>
    /// A many objects resolver that uses IoC
    /// </summary>
    /// <typeparam name="TResolver"></typeparam>
    /// <typeparam name="TResolved"></typeparam>
    public abstract class ContainerManyObjectsResolver<TResolver, TResolved> : ManyObjectsResolverBase<TResolver, TResolved>
        where TResolved : class
        where TResolver : ResolverBase
    {
        private readonly IServiceContainer _container;

        //TODO: Get rid of these - pretty sure all tests will still fail with these pass throughs, need to update
        // all tests that use resolvers to use a real container - then update most tests that are not integration tests to not use any resolvers!
        #region Constructors used for test - ONLY so that a container is not required and will just revert to using the normal ManyObjectsResolverBase
        [Obsolete("Used for tests only - should remove")]
        internal ContainerManyObjectsResolver(ILogger logger, IEnumerable<Type> types, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : base(logger, types, scope)
        {
        }
        [Obsolete("Used for tests only - should remove")]
        internal ContainerManyObjectsResolver(IServiceProvider serviceProvider, ILogger logger, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : base(serviceProvider, logger, scope)
        {
        }
        [Obsolete("Used for tests only - should remove")]
        internal ContainerManyObjectsResolver(IServiceProvider serviceProvider, ILogger logger, HttpContextBase httpContext)
            : base(serviceProvider, logger, httpContext)
        {
        }
        [Obsolete("Used for tests only - should remove")]
        internal ContainerManyObjectsResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> value, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : base(serviceProvider, logger, value, scope)
        {
        }
        #endregion

        /// <summary>
        /// Constructor for use with IoC
        /// </summary>
        /// <param name="container"></param>
        /// <param name="logger"></param>
        /// <param name="types"></param>
        /// <param name="scope"></param>
        internal ContainerManyObjectsResolver(IServiceContainer container, ILogger logger, IEnumerable<Type> types, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : base(logger, types, scope)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            _container = container;
            Resolution.Frozen += Resolution_Frozen;
        }

        /// <summary>
        /// When resolution is frozen add all the types to the container
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Resolution_Frozen(object sender, EventArgs e)
        {
            var prefix = GetType().FullName + "_";
            var i = 0;
            foreach (var type in InstanceTypes)
            {
                var name = $"{prefix}{i++:00000}";
                _container.Register(typeof(TResolved), type, name, GetLifetime(LifetimeScope));
            }
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
                //case ObjectLifetimeScope.Transient:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates the instances from IoC
        /// </summary>
        /// <returns>A list of objects of type <typeparamref name="TResolved"/>.</returns>
        protected override IEnumerable<TResolved> CreateInstances()
        {
            //NOTE: we ignore scope because objects are registered under this scope and not build based on the scope.

            var prefix = GetType().FullName + "_";

            // GetAllInstances could return more than what *this* resolver has registered,
            // and there is no guarantee instances will be in the right order - have to do
            // it differently

            var services = _container.AvailableServices
                .Where(x => x.ServiceName.StartsWith(prefix))
                .OrderBy(x => x.ServiceName);

            var values = services
                .Select(x => _container.GetInstance<TResolved>(x.ServiceName))
                .ToArray();

            return values;
        }
    }
}