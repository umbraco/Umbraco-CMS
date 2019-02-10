﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Implements a weighted collection builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public abstract class WeightedCollectionBuilderBase<TBuilder, TCollection, TItem> : CollectionBuilderBase<TBuilder, TCollection, TItem>
        where TBuilder : WeightedCollectionBuilderBase<TBuilder, TCollection, TItem>
        where TCollection : class, IBuilderCollection<TItem>
    {
        protected abstract TBuilder This { get; }

        /// <summary>
        /// Clears all types in the collection.
        /// </summary>
        /// <returns>The builder.</returns>
        public TBuilder Clear()
        {
            Configure(types => types.Clear());
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
                    EnsureType(type, "register");
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

        /// <summary>
        /// Removes a type from the collection.
        /// </summary>
        /// <param name="type">The type to remove.</param>
        /// <returns>The builder.</returns>
        public TBuilder Remove(Type type)
        {
            Configure(types =>
            {
                EnsureType(type, "remove");
                if (types.Contains(type)) types.Remove(type);
            });
            return This;
        }

        protected override IEnumerable<Type> GetRegisteringTypes(IEnumerable<Type> types)
        {
            var list = types.ToList();
            list.Sort((t1, t2) => GetWeight(t1).CompareTo(GetWeight(t2)));
            return list;
        }

        public virtual int DefaultWeight { get; set; } = 100;

        protected virtual int GetWeight(Type type)
        {
            var attr = type.GetCustomAttributes(typeof(WeightAttribute), false).OfType<WeightAttribute>().SingleOrDefault();
            return attr?.Weight ?? DefaultWeight;
        }
    }
}
