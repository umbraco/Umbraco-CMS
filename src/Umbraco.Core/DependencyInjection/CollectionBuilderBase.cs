using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;

namespace Umbraco.Core.DependencyInjection
{
    /// <summary>
    /// Provides a base class for collection builders.
    /// </summary>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public abstract class CollectionBuilderBase<TCollection, TItem> : ICollectionBuilder<TCollection, TItem>
        where TCollection : IBuilderCollection<TItem>
    {
        private readonly IServiceContainer _container;
        private readonly List<Type> _types = new List<Type>();
        private readonly object _locker = new object();
        private bool _typesRegistered;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionBuilderBase{TCollection,TItem}"/>
        /// class with a service container.
        /// </summary>
        /// <param name="container">A service container.</param>
        protected CollectionBuilderBase(IServiceContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Configures the internal list of types.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <remarks>Throws if the types have already been registered.</remarks>
        protected void Configure(Action<List<Type>> action)
        {
            lock (_locker)
            {
                if (_typesRegistered) throw new InvalidOperationException();
                action(_types);
            }
        }

        /// <summary>
        /// Gets the types.
        /// </summary>
        /// <param name="types">The internal list of types.</param>
        /// <returns>The list of types to register.</returns>
        protected virtual IEnumerable<Type> GetTypes(IEnumerable<Type> types)
        {
            return types;
        }

        private void RegisterTypes()
        {
            lock (_locker)
            {
                if (_typesRegistered) return;

                var prefix = GetType().FullName + "_";
                var i = 0;
                foreach (var type in GetTypes(_types))
                {
                    var name = $"{prefix}{i++:00000}";
                    _container.Register(typeof(TItem), type, name);
                }

                _typesRegistered = true;
            }
        }

        /// <summary>
        /// Gets a collection.
        /// </summary>
        /// <returns>A collection.</returns>
        /// <remarks>
        /// <para>Creates a new collection each time it is invoked, but only registers types once.</para>
        /// <para>Explicitely implemented in order to hide it to users.</para>
        /// </remarks>
        TCollection ICollectionBuilder<TCollection, TItem>.GetCollection()
        {
            RegisterTypes(); // will do it only once

            var prefix = GetType().FullName + "_";

            // fixme - shouldn't we save this somewhere?
            var services = _container.AvailableServices
                .Where(x => x.ServiceName.StartsWith(prefix))
                .OrderBy(x => x.ServiceName);

            var items = services
                .Select(x => _container.GetInstance<TItem>(x.ServiceName))
                .ToArray();

            return CreateCollection(items);
        }

        /// <summary>
        /// Creates the injected collection.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns>An injected collection containing the items.</returns>
        protected abstract TCollection CreateCollection(IEnumerable<TItem> items);
    }
}