using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.ObjectResolution
{
    public abstract class LazyManyObjectsResolverBase<TBuilder, TCollection, TItem>
        where TBuilder : LazyCollectionBuilderBase<TBuilder, TCollection, TItem>
        where TCollection : IBuilderCollection<TItem>
    {
        protected LazyManyObjectsResolverBase(TBuilder builder)
        {
            Builder = builder;
        }

        protected TBuilder Builder { get; }

        public static bool HasCurrent => true;

        /// <summary>
        /// Removes types from the list of types, once it has been lazily evaluated, and before actual objects are instanciated.
        /// </summary>
        /// <param name="value">The type to remove.</param>
        public void RemoveType(Type value)
        {
            Builder.Exclude(value);
        }

        /// <summary>
        /// Lazily adds types from a function producing types.
        /// </summary>
        /// <param name="typeListProducer">The functions producing types, to add.</param>
        public void AddTypeListDelegate(Func<IEnumerable<Type>> typeListProducer)
        {
            Builder.Add(typeListProducer);
        }

        /// <summary>
        /// Lazily adds a type from an actual type.
        /// </summary>
        /// <param name="value">The actual type, to add.</param>
        /// <remarks>The type is converted to a lazy type.</remarks>
        public void AddType(Type value)
        {
            Builder.Add(value);
        }

        /// <summary>
        /// Clears all lazy types
        /// </summary>
        public void Clear()
        {
            Builder.Clear();
        }
    }
}
