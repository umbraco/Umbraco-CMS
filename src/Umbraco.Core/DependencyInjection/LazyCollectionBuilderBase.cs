using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;

namespace Umbraco.Core.DependencyInjection
{
    /// <summary>
    /// Implements a lazy collection builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public abstract class LazyCollectionBuilderBase<TBuilder, TCollection, TItem> : CollectionBuilderBase<TCollection, TItem>
        where TBuilder : LazyCollectionBuilderBase<TBuilder, TCollection, TItem>
        where TCollection : IBuilderCollection<TItem>
    {
        private readonly List<Func<IEnumerable<Type>>> _producers = new List<Func<IEnumerable<Type>>>();
        private readonly List<Type> _excluded = new List<Type>();

        protected LazyCollectionBuilderBase(IServiceContainer container)
            : base(container)
        { }

        protected abstract TBuilder This { get; }

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
        /// Removes a type from the collection.
        /// </summary>
        /// <typeparam name="T">The type to remove.</typeparam>
        /// <returns>The builder.</returns>
        public TBuilder Remove<T>()
            where T : TItem
        {
            Configure(types =>
            {
                var type = typeof(T);
                if (types.Contains(type)) types.Remove(type);
            });
            return This;
        }

        /// <summary>
        /// Adds a types producer to the collection.
        /// </summary>
        /// <param name="producer">The types producer.</param>
        /// <returns>The builder.</returns>
        public TBuilder AddProducer(Func<IEnumerable<Type>> producer)
        {
            Configure(types =>
            {
                _producers.Add(producer);
            });
            return This;
        }

        /// <summary>
        /// Excludes a type from the collection.
        /// </summary>
        /// <typeparam name="T">The type to exclude.</typeparam>
        /// <returns>The builder.</returns>
        public TBuilder Exclude<T>()
        {
            Configure(types =>
            {
                var type = typeof(T);
                if (_excluded.Contains(type) == false) _excluded.Add(type);
            });
            return This;
        }

        protected override IEnumerable<Type> GetTypes(IEnumerable<Type> types)
        {
            return types.Union(_producers.SelectMany(x => x())).Distinct().Except(_excluded);
        }
    }
}