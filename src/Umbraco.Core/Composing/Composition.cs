using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Composing
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
        public TypeLoader TypeLoader { get; }
        public IRuntimeState RuntimeState { get; }
        public IProfilingLogger Logger { get; }
        public IIOHelper IOHelper { get; }
        public AppCaches AppCaches { get; }
        public IServiceCollection Services { get; }

        private readonly Dictionary<Type, ICollectionBuilder> _builders = new Dictionary<Type, ICollectionBuilder>();

        public Composition(IServiceCollection services, TypeLoader typeLoader, IProfilingLogger logger, IRuntimeState runtimeState, IIOHelper ioHelper, AppCaches appCaches)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            TypeLoader = typeLoader ?? throw new ArgumentNullException(nameof(typeLoader));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            RuntimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
            IOHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
            AppCaches = appCaches ?? throw new ArgumentNullException(nameof(appCaches));
        }

        public void RegisterBuilders()
        {
            foreach (var builder in _builders.Values)
                builder.RegisterWith(Services);
            _builders.Clear(); // no point keep them around
        }

        /// <summary>
        /// Gets a collection builder (and registers the collection).
        /// </summary>
        /// <typeparam name="TBuilder">The type of the collection builder.</typeparam>
        /// <returns>The collection builder.</returns>
        public TBuilder WithCollectionBuilder<TBuilder>()
            where TBuilder: ICollectionBuilder, new()
        {
            var typeOfBuilder = typeof(TBuilder);

            if (_builders.TryGetValue(typeOfBuilder, out var o))
                return (TBuilder) o;

            var builder = new TBuilder();
            _builders[typeOfBuilder] = builder;
            return builder;
        }
    }
}
