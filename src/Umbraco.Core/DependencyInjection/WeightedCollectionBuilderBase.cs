using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;

namespace Umbraco.Core.DependencyInjection
{
    /// <summary>
    /// Implements a weighted collection builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public abstract class WeightedCollectionBuilderBase<TBuilder, TCollection, TItem> : CollectionBuilderBase<TBuilder, TCollection, TItem>
        where TBuilder : WeightedCollectionBuilderBase<TBuilder, TCollection, TItem>
        where TCollection : IBuilderCollection<TItem>
    {
        protected WeightedCollectionBuilderBase(IServiceContainer container)
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
        /// Adds types to the collection.
        /// </summary>
        /// <param name="types">The types to add.</param>
        /// <returns>The builder.</returns>
        public TBuilder Add(IEnumerable<Type> types)
        {
            Configure(list =>
            {
                foreach (var type in types)
                {
                    // would be detected by CollectionBuilderBase when registering, anyways, but let's fail fast
                    if (typeof(TItem).IsAssignableFrom(type) == false)
                        throw new InvalidOperationException($"Cannot register type {type.FullName} as it does not inherit from/implement {typeof(TItem).FullName}.");
                    if (list.Contains(type) == false) list.Add(type);
                }
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

        protected override IEnumerable<Type> GetTypes(IEnumerable<Type> types)
        {
            var list = types.ToList();
            list.Sort((t1, t2) => GetWeight(t1).CompareTo(GetWeight(t2)));
            return list;
        }

        protected virtual int DefaultWeight { get; set; } = 10;

        protected virtual int GetWeight(Type type)
        {
            var attr = type.GetCustomAttributes(typeof(WeightAttribute), false).OfType<WeightAttribute>().SingleOrDefault();
            return attr?.Weight ?? DefaultWeight;
        }
    }
}