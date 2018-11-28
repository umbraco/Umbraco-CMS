using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides a base class for collection builders.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public abstract class CollectionBuilderBase<TBuilder, TCollection, TItem> : ICollectionBuilder<TCollection, TItem>
        where TBuilder: CollectionBuilderBase<TBuilder, TCollection, TItem>
        where TCollection : IBuilderCollection<TItem>
    {
        private readonly List<Type> _types = new List<Type>();
        private readonly object _locker = new object();
        private Type[] _registeredTypes;

        /// <summary>
        /// Gets the container.
        /// </summary>
        protected IRegister Container { get; private set; }

        /// <summary>
        /// Gets the internal list of types as an IEnumerable (immutable).
        /// </summary>
        public IEnumerable<Type> GetTypes() => _types;

        /// <summary>
        /// Initializes a new instance of the builder.
        /// </summary>
        /// <remarks>By default, this registers the collection automatically.</remarks>
        public virtual void Initialize(IRegister container)
        {
            if (Container != null)
                throw new InvalidOperationException("This builder has already been initialized.");

            Container = container;

            // register the collection
            Container.Register(factory => CreateCollection(factory), CollectionLifetime);
        }

        /// <summary>
        /// Gets the collection lifetime.
        /// </summary>
        protected virtual Lifetime CollectionLifetime => Lifetime.Singleton;

        /// <summary>
        /// Configures the internal list of types.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <remarks>Throws if the types have already been registered.</remarks>
        protected void Configure(Action<List<Type>> action)
        {
            lock (_locker)
            {
                if (_registeredTypes != null)
                    throw new InvalidOperationException("Cannot configure a collection builder after its types have been resolved.");
                action(_types);
            }
        }

        /// <summary>
        /// Gets the types.
        /// </summary>
        /// <param name="types">The internal list of types.</param>
        /// <returns>The list of types to register.</returns>
        /// <remarks>Used by implementations to add types to the internal list, sort the list, etc.</remarks>
        protected virtual IEnumerable<Type> GetRegisteringTypes(IEnumerable<Type> types)
        {
            return types;
        }

        private void RegisterTypes()
        {
            lock (_locker)
            {
                if (_registeredTypes != null) return;

                var types = GetRegisteringTypes(_types).ToArray();

                // ensure they are safe
                foreach (var type in types)
                    EnsureType(type, "register");

                // register them
                foreach (var type in types)
                    Container.Register(type);

                _registeredTypes = types;
            }
        }

        /// <summary>
        /// Creates the collection items.
        /// </summary>
        /// <returns>The collection items.</returns>
        protected virtual IEnumerable<TItem> CreateItems(IFactory factory)
        {
            RegisterTypes(); // will do it only once

            return _registeredTypes // respect order
                .Select(x => (TItem) factory.GetInstance(x))
                .ToArray(); // safe
        }

        /// <summary>
        /// Creates a collection.
        /// </summary>
        /// <returns>A collection.</returns>
        /// <remarks>Creates a new collection each time it is invoked.</remarks>
        public virtual TCollection CreateCollection(IFactory factory)
        {
            return factory.CreateInstance<TCollection>(CreateItems(factory));
        }

        protected Type EnsureType(Type type, string action)
        {
            if (typeof(TItem).IsAssignableFrom(type) == false)
                throw new InvalidOperationException($"Cannot {action} type {type.FullName} as it does not inherit from/implement {typeof(TItem).FullName}.");
            return type;
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains a type.
        /// </summary>
        /// <typeparam name="T">The type to look for.</typeparam>
        /// <returns>A value indicating whether the collection contains the type.</returns>
        /// <remarks>Some builder implementations may use this to expose a public Has{T}() method,
        /// when it makes sense. Probably does not make sense for lazy builders, for example.</remarks>
        public virtual bool Has<T>()
            where T : TItem
        {
            return _types.Contains(typeof (T));
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains a type.
        /// </summary>
        /// <param name="type">The type to look for.</param>
        /// <returns>A value indicating whether the collection contains the type.</returns>
        /// <remarks>Some builder implementations may use this to expose a public Has{T}() method,
        /// when it makes sense. Probably does not make sense for lazy builders, for example.</remarks>
        public virtual bool Has(Type type)
        {
            EnsureType(type, "find");
            return _types.Contains(type);
        }
    }
}
