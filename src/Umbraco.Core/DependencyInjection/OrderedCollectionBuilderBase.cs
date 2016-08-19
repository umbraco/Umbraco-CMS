using System;
using System.Collections.Generic;
using LightInject;

namespace Umbraco.Core.DependencyInjection
{
    /// <summary>
    /// Implements an ordered collection builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public abstract class OrderedCollectionBuilderBase<TBuilder, TCollection, TItem> : CollectionBuilderBase<TBuilder, TCollection, TItem>
        where TBuilder : OrderedCollectionBuilderBase<TBuilder, TCollection, TItem>
        where TCollection : IBuilderCollection<TItem>
    {
        protected OrderedCollectionBuilderBase(IServiceContainer container)
            : base (container)
        { }

        protected abstract TBuilder This { get; }

        /// <summary>
        /// Appends a type to the collection.
        /// </summary>
        /// <typeparam name="T">The type to append.</typeparam>
        /// <returns>The builder.</returns>
        public TBuilder Append<T>()
            where T : TItem
        {
            Configure(types =>
            {
                var type = typeof (T);
                if (types.Contains(type)) types.Remove(type);
                types.Add(type);
            });
            return This;
        }

        /// <summary>
        ///  Appends types to the collections.
        /// </summary>
        /// <param name="types">The types to append.</param>
        /// <returns>The builder.</returns>
        public TBuilder Append(IEnumerable<Type> types)
        {
            Configure(list =>
            {
                foreach (var type in types)
                {
                    if (list.Contains(type)) list.Remove(type);
                    list.Add(type);
                }
            });
            return This;
        }

        /// <summary>
        /// Appends a type after another type.
        /// </summary>
        /// <typeparam name="TAfter">The other type.</typeparam>
        /// <typeparam name="T">The type to append.</typeparam>
        /// <returns>The builder.</returns>
        /// <remarks>Throws if both types are identical, or if the other type does not already belong to the collection.</remarks>
        public TBuilder AppendAfter<TAfter, T>()
            where TAfter : TItem
            where T : TItem
        {
            Configure(types =>
            {
                var typeAfter = typeof (TAfter);
                var type = typeof(T);
                if (typeAfter == type) throw new InvalidOperationException();

                var index = types.IndexOf(typeAfter);
                if (index < 0) throw new InvalidOperationException();

                if (types.Contains(type)) types.Remove(type);
                index = types.IndexOf(typeAfter); // in case removing type changed index
                types.Insert(index + 1, type);
            });
            return This;
        }

        /// <summary>
        /// Inserts a type into the collection.
        /// </summary>
        /// <typeparam name="T">The type to insert.</typeparam>
        /// <returns>The builder.</returns>
        public TBuilder Insert<T>()
            where T : TItem
        {
            Configure(types =>
            {
                var type = typeof (T);
                if (types.Contains(type)) types.Remove(type);
                types.Insert(0, type);
            });
            return This;
        }

        /// <summary>
        /// Inserts a type before another type.
        /// </summary>
        /// <typeparam name="TBefore">The other type.</typeparam>
        /// <typeparam name="T">The type to insert.</typeparam>
        /// <returns>The builder.</returns>
        /// <remarks>Throws if both types are identical, or if the other type does not already belong to the collection.</remarks>
        public TBuilder InsertBefore<TBefore, T>()
            where TBefore : TItem
            where T : TItem
        {
            Configure(types =>
            {
                var typeBefore = typeof(TBefore);
                var type = typeof(T);
                if (typeBefore == type) throw new InvalidOperationException();

                var index = types.IndexOf(typeBefore);
                if (index < 0) throw new InvalidOperationException();

                if (types.Contains(type)) types.Remove(type);
                index = types.IndexOf(typeBefore); // in case removing type changed index
                types.Insert(index, type);
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
                var type = typeof (T);
                if (types.Contains(type)) types.Remove(type);
            });
            return This;
        }

        /// <summary>
        /// Replaces a type in the collection.
        /// </summary>
        /// <typeparam name="TReplaced">The type to replace.</typeparam>
        /// <typeparam name="T">The type to insert.</typeparam>
        /// <returns>The builder.</returns>
        /// <remarks>Throws if the type to replace does not already belong to the collection.</remarks>
        public TBuilder Replace<TReplaced, T>()
            where TReplaced : TItem
            where T : TItem
        {
            Configure(types =>
            {
                var typeReplaced = typeof(TReplaced);
                var type = typeof(T);
                if (typeReplaced == type) return;

                var index = types.IndexOf(typeReplaced);
                if (index < 0) throw new InvalidOperationException();

                if (types.Contains(type)) types.Remove(type);
                index = types.IndexOf(typeReplaced); // in case removing type changed index
                types.Insert(index, type);
                types.Remove(typeReplaced);
            });
            return This;
        }
    }
}
