using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LightInject;

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
        private Func<IEnumerable<TItem>, TCollection> _collectionCtor;
        private ServiceRegistration[] _registrations;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionBuilderBase{TBuilder, TCollection,TItem}"/>
        /// class with a service container.
        /// </summary>
        /// <param name="container">A service container.</param>
        protected CollectionBuilderBase(IServiceContainer container)
        {
            Container = container;
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Initialize();
        }

        /// <summary>
        /// Gets the service container.
        /// </summary>
        protected IServiceContainer Container { get; }

        /// <summary>
        /// Gets the internal list of types as an IEnumerable (immutable).
        /// </summary>
        public IEnumerable<Type> GetTypes() => _types;

        /// <summary>
        /// Initializes a new instance of the builder.
        /// </summary>
        /// <remarks>This is called by the constructor and, by default, registers the
        /// collection automatically.</remarks>
        protected virtual void Initialize()
        {
            // compile the auto-collection constructor
            var argType = typeof(IEnumerable<TItem>);
            var ctorArgTypes = new[] { argType };
            var constructor = typeof(TCollection).GetConstructor(ctorArgTypes);
            if (constructor != null)
            {
                var exprArg = Expression.Parameter(argType, "items");
                var exprNew = Expression.New(constructor, exprArg);
                var expr = Expression.Lambda<Func<IEnumerable<TItem>, TCollection>>(exprNew, exprArg);
                _collectionCtor = expr.Compile();
            }
            // else _collectionCtor remains null, assuming CreateCollection has been overriden

            // we just don't want to support re-registering collections here
            var registration = Container.GetAvailableService<TCollection>();
            if (registration != null)
                throw new InvalidOperationException("Collection builders cannot be registered once the collection itself has been registered.");

            // register the collection
            Container.Register(_ => CreateCollection(), CollectionLifetime);
        }

        /// <summary>
        /// Gets the collection lifetime.
        /// </summary>
        /// <remarks>Return null for transient collections.</remarks>
        protected virtual ILifetime CollectionLifetime => new PerContainerLifetime();

        /// <summary>
        /// Configures the internal list of types.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <remarks>Throws if the types have already been registered.</remarks>
        protected void Configure(Action<List<Type>> action)
        {
            lock (_locker)
            {
                if (_registrations != null)
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
                if (_registrations != null) return;

                var types = GetRegisteringTypes(_types).ToArray();
                foreach (var type in types)
                    EnsureType(type, "register");

                var prefix = GetType().FullName + "_";
                var i = 0;
                foreach (var type in types)
                {
                    var name = $"{prefix}{i++:00000}";
                    Container.Register(typeof(TItem), type, name);
                }

                _registrations = Container.AvailableServices
                    .Where(x => x.ServiceName.StartsWith(prefix))
                    .OrderBy(x => x.ServiceName)
                    .ToArray();
            }
        }

        /// <summary>
        /// Creates the collection items.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The collection items.</returns>
        protected virtual IEnumerable<TItem> CreateItems(params object[] args)
        {
            RegisterTypes(); // will do it only once

            var type = typeof (TItem);
            return _registrations
                .Select(x => (TItem) Container.GetInstanceOrThrow(type, x.ServiceName, x.ImplementingType, args))
                .ToArray(); // safe
        }

        /// <summary>
        /// Creates a collection.
        /// </summary>
        /// <returns>A collection.</returns>
        /// <remarks>Creates a new collection each time it is invoked.</remarks>
        public virtual TCollection CreateCollection()
        {
            if (_collectionCtor == null) throw new InvalidOperationException("Collection auto-creation is not possible.");
            return _collectionCtor(CreateItems());
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
