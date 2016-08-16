using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;

namespace Umbraco.Core.DependencyInjection
{
    /// <summary>
    /// Provides a base class for injected collection builders.
    /// </summary>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public abstract class InjectCollectionBuilderBase<TCollection, TItem> : IInjectCollectionBuilder<TCollection, TItem>
        where TCollection : IInjectCollection<TItem>
    {
        private readonly IServiceContainer _container;
        private readonly List<Type> _types = new List<Type>();
        private readonly object _locker = new object();
        private bool _typesRegistered;

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectCollectionBuilderBase{TCollection, TItem}"/>
        /// class with a service container.
        /// </summary>
        /// <param name="container">A service container.</param>
        protected InjectCollectionBuilderBase(IServiceContainer container)
        {
            _container = container;
        }

        // everything could be implemented, add, insert, remove, replace, order, whatever...
        // and we could also have 'lazy' collections by supplying a types factory

        protected void Configure(Action action)
        {
            lock (_locker)
            {
                if (_typesRegistered) throw new InvalidOperationException();
                action();
            }
        }

        public void Add<T>()
            where T : TItem
        {
            Configure(() =>
            {
                var type = typeof(T);
                if (!_types.Contains(type))
                    _types.Add(type);
            });
        }

        protected virtual IEnumerable<Type> PrepareTypes(IEnumerable<Type> types)
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
                foreach (var type in PrepareTypes(_types))
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
        TCollection IInjectCollectionBuilder<TCollection, TItem>.GetCollection()
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