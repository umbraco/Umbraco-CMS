using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Infrastructure.Composing;

using NoFactory = Umbraco.Infrastructure.Composing.ServiceProviderFactoryAdapter;

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
        private readonly Dictionary<Type, ICollectionBuilder> _builders = new Dictionary<Type, ICollectionBuilder>();
        private IRegister _register => ServiceCollectionRegistryAdapter.Wrap(Services);

        public Composition(IServiceCollection services, TypeLoader typeLoader, IProfilingLogger logger, IRuntimeState runtimeState, IIOHelper ioHelper, AppCaches appCaches)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            TypeLoader = typeLoader ?? throw new ArgumentNullException(nameof(typeLoader));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            RuntimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
            IOHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
            AppCaches = appCaches ?? throw new ArgumentNullException(nameof(appCaches));
        }

        #region Services

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public IProfilingLogger Logger { get; }

        public IIOHelper IOHelper { get; }
        public AppCaches AppCaches { get; }

        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the type loader.
        /// </summary>
        public TypeLoader TypeLoader { get; }

        /// <summary>
        /// Gets the runtime state.
        /// </summary>
        public IRuntimeState RuntimeState { get; }

        #endregion

        #region IRegister
        
        /// <inheritdoc />
        public void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient)
            => _register.Register(serviceType, lifetime);

        /// <inheritdoc />
        public void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient)
            => _register.Register(serviceType, implementingType, lifetime);

        /// <inheritdoc />
        public void Register<TService>(Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class
            => _register.Register(factory, lifetime);

        /// <inheritdoc />
        public void Register(Type serviceType, object instance)
            => _register.Register(serviceType, instance);


        /// <inheritdoc />
        public void RegisterBuilders()
        {
            foreach (var builder in _builders.Values)
                builder.RegisterWith(_register);
            _builders.Clear(); // no point keep them around
        }

        #endregion

        #region Unique
        
        /// <summary>
        /// Registers a unique service with an implementation type.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public void RegisterUnique(Type serviceType, Type implementingType)
            => Services.Replace(ServiceDescriptor.Singleton(serviceType, implementingType));

        /// <summary>
        /// Registers a unique service with an implementation factory.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public void RegisterUnique<TService>(Func<IFactory, TService> factory)
            where TService : class
            => Services.Replace(ServiceDescriptor.Singleton(sp => factory(NoFactory.Wrap(sp))));

        /// <summary>
        /// Registers a unique service with an implementing instance.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public void RegisterUnique(Type serviceType, object instance)
            => Services.Replace(ServiceDescriptor.Singleton(serviceType, instance));

        #endregion

        #region Collection Builders

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

        #endregion
    }

    public static class CompositionExtensins
    {
        //public static IFactory CreateFactory(this Composition composition)
        //{
        //    return NoFactory.Wrap(composition.Services.BuildServiceProvider());
        //}
    }
}
