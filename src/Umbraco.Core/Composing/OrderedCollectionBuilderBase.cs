using System;
using System.Collections.Generic;
using LightInject;

namespace Umbraco.Core.Composing
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
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedCollectionBuilderBase{TBuilder,TCollection,TItem}"/> class.
        /// </summary>
        /// <param name="container"></param>
        protected OrderedCollectionBuilderBase(IServiceContainer container)
            : base (container)
        { }

        protected abstract TBuilder This { get; }

        /// <summary>
        /// Clears all types in the collection.
        /// </summary>
        /// <returns>The buidler.</returns>
        public TBuilder Clear()
        {
            Configure(types => types.Clear());
            return This;
        }

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
        /// Appends a type to the collection.
        /// </summary>
        /// <param name="type">The type to append.</param>
        /// <returns>The builder.</returns>
        public TBuilder Append(Type type)
        {
            Configure(types =>
            {
                EnsureType(type, "register");
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
                    // would be detected by CollectionBuilderBase when registering, anyways, but let's fail fast
                    EnsureType(type, "register");
                    if (list.Contains(type)) list.Remove(type);
                    list.Add(type);
                }
            });
            return This;
        }

        /// <summary>
        ///  Appends types to the collections.
        /// </summary>
        /// <param name="types">The types to append.</param>
        /// <returns>The builder.</returns>
        public TBuilder Append(Func<IServiceFactory, IEnumerable<Type>> types)
        {
            Configure(list =>
            {
                foreach (var type in types(Container))
                {
                    // would be detected by CollectionBuilderBase when registering, anyways, but let's fail fast
                    EnsureType(type, "register");
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
        /// <param name="index">The optional index.</param>
        /// <returns>The builder.</returns>
        /// <remarks>Throws if the index is out of range.</remarks>
        public TBuilder Insert<T>(int index = 0)
            where T : TItem
        {
            Configure(types =>
            {
                var type = typeof (T);
                if (types.Contains(type)) types.Remove(type);
                types.Insert(index, type);
            });
            return This;
        }

        /// <summary>
        /// Inserts a type into the collection.
        /// </summary>
        /// <param name="type">The type to insert.</param>
        /// <returns>The builder.</returns>
        /// <remarks>Throws if the index is out of range.</remarks>
        public TBuilder Insert(Type type)
        {
            return Insert(0, type);
        }

        /// <summary>
        /// Inserts a type into the collection.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="type">The type to insert.</param>
        /// <returns>The builder.</returns>
        /// <remarks>Throws if the index is out of range.</remarks>
        public TBuilder Insert(int index, Type type)
        {
            Configure(types =>
            {
                EnsureType(type, "register");
                if (types.Contains(type)) types.Remove(type);
                types.Insert(index, type);
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
        /// Inserts a type before another type.
        /// </summary>
        /// <param name="typeBefore">The other type.</param>
        /// <param name="type">The type to insert.</param>
        /// <returns>The builder.</returns>
        /// <remarks>Throws if both types are identical, or if the other type does not already belong to the collection.</remarks>
        public TBuilder InsertBefore(Type typeBefore, Type type)
        {
            Configure(types =>
            {
                EnsureType(typeBefore, "find");
                EnsureType(type, "register");

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

        /// <summary>
        /// Replaces a type in the collection.
        /// </summary>
        /// <param name="typeReplaced">The type to replace.</param>
        /// <param name="type">The type to insert.</param>
        /// <returns>The builder.</returns>
        /// <remarks>Throws if the type to replace does not already belong to the collection.</remarks>
        public TBuilder Replace(Type typeReplaced, Type type)
        {
            Configure(types =>
            {
                EnsureType(typeReplaced, "find");
                EnsureType(type, "register");

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
