using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Represents a composition.
    /// </summary>
    /// <remarks>Although a composition exposes the application's service container, people should use the
    /// extension methods (such as <c>PropertyEditors()</c> or <c>SetPublishedContentModelFactory()</c>) and
    /// avoid accessing the container. This is because everything needs to be properly registered and with
    /// the proper lifecycle. These methods will take care of it. Directly registering into the container
    /// may cause issues.</remarks>
    public class Composition
    {
        private readonly Dictionary<Type, ICollectionBuilder> _builders = new Dictionary<Type, ICollectionBuilder>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Composition"/> class.
        /// </summary>
        /// <param name="container">A container.</param>
        /// <param name="typeLoader">The type loader.</param>
        /// <param name="level">The runtime level.</param>
        public Composition(IContainer container, TypeLoader typeLoader, RuntimeLevel level)
        {
            Container = container;
            TypeLoader = typeLoader;
            RuntimeLevel = level;
        }

        // used for tests
        internal Composition(IContainer container, RuntimeLevel level)
        {
            Container = container;
            RuntimeLevel = level;
        }

        /// <summary>
        /// Gets the container.
        /// </summary>
        /// <remarks>Use with care!</remarks>
        public IContainer Container { get; }

        /// <summary>
        /// Gets the type loader.
        /// </summary>
        public TypeLoader TypeLoader { get; }

        /// <summary>
        /// Gets the runtime level.
        /// </summary>
        public RuntimeLevel RuntimeLevel { get; }

        /// <summary>
        /// Gets a collection builder (and registers the collection).
        /// </summary>
        /// <typeparam name="TBuilder">The type of the collection builder.</typeparam>
        /// <returns>The collection builder.</returns>
        public TBuilder GetCollectionBuilder<TBuilder>()
            where TBuilder: ICollectionBuilder, new()
        {
            var typeOfBuilder = typeof(TBuilder);

            if (_builders.TryGetValue(typeOfBuilder, out var o))
                return (TBuilder) o;

            var builder = new TBuilder();
            builder.Initialize(Container);

            _builders[typeOfBuilder] = builder;

            return builder;
        }
    }
}
