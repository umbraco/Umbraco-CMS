using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Implements a lazy collection builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public abstract class LazyCollectionBuilderBase<TBuilder, TCollection, TItem> : CollectionBuilderBase<TBuilder, TCollection, TItem>
        where TBuilder : LazyCollectionBuilderBase<TBuilder, TCollection, TItem>
        where TCollection : IBuilderCollection<TItem>
    {
        private readonly List<Func<IEnumerable<Type>>> _producers1 = new List<Func<IEnumerable<Type>>>();
        private readonly List<Func<IServiceFactory, IEnumerable<Type>>> _producers2 = new List<Func<IServiceFactory, IEnumerable<Type>>>();
        private readonly List<Type> _excluded = new List<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCollectionBuilderBase{TBuilder,TCollection,TItem}"/> class.
        /// </summary>
        protected LazyCollectionBuilderBase(IServiceContainer container)
            : base(container)
        { }

        protected abstract TBuilder This { get; }

        /// <summary>
        /// Clears all types in the collection.
        /// </summary>
        /// <returns>The buidler.</returns>
        public TBuilder Clear()
        {
            Configure(types =>
            {
                types.Clear();
                _producers1.Clear();
                _producers2.Clear();
                _excluded.Clear();
            });
            return This;
        }

        /// <summary>
        /// Adds a type to the collection.
        /// </summary>
        /// <typeparam name="T">The type to add.</typeparam>
        /// <returns>The builder.</returns>
        public TBuilder Add<T>()
            where T : TItem
        {
            Configure(types =>
            {
                var type = typeof(T);
                if (types.Contains(type) == false) types.Add(type);
            });
            return This;
        }

        /// <summary>
        /// Adds a type to the collection.
        /// </summary>
        /// <param name="type">The type to add.</param>
        /// <returns>The builder.</returns>
        public TBuilder Add(Type type)
        {
            Configure(types =>
            {
                EnsureType(type, "register");
                if (types.Contains(type) == false) types.Add(type);
            });
            return This;
        }

        /// <summary>
        /// Adds a types producer to the collection.
        /// </summary>
        /// <param name="producer">The types producer.</param>
        /// <returns>The builder.</returns>
        public TBuilder Add(Func<IEnumerable<Type>> producer)
        {
            Configure(types =>
            {
                _producers1.Add(producer);
            });
            return This;
        }

        /// <summary>
        /// Adds a types producer to the collection.
        /// </summary>
        /// <param name="producer">The types producer.</param>
        /// <returns>The builder.</returns>
        public TBuilder Add(Func<IServiceFactory, IEnumerable<Type>> producer)
        {
            Configure(types =>
            {
                _producers2.Add(producer);
            });
            return This;
        }

        /// <summary>
        /// Excludes a type from the collection.
        /// </summary>
        /// <typeparam name="T">The type to exclude.</typeparam>
        /// <returns>The builder.</returns>
        public TBuilder Exclude<T>()
            where T : TItem
        {
            Configure(types =>
            {
                var type = typeof(T);
                if (_excluded.Contains(type) == false) _excluded.Add(type);
            });
            return This;
        }

        /// <summary>
        /// Excludes a type from the collection.
        /// </summary>
        /// <param name="type">The type to exclude.</param>
        /// <returns>The builder.</returns>
        public TBuilder Exclude(Type type)
        {
            Configure(types =>
            {
                EnsureType(type, "exclude");
                if (_excluded.Contains(type) == false) _excluded.Add(type);
            });
            return This;
        }

        protected override IEnumerable<Type> GetRegisteringTypes(IEnumerable<Type> types)
        {
            return types
                .Union(_producers1.SelectMany(x => x()))
                .Union(_producers2.SelectMany(x => x(Container)))
                .Distinct()
                .Select(x => EnsureType(x, "register"))
                .Except(_excluded);
        }
    }
}
