﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
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
        private readonly Dictionary<Type, ICollectionBuilder> _builders = new Dictionary<Type, ICollectionBuilder>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Composition"/> class.
        /// </summary>
        /// <param name="services">A register.</param>
        /// <param name="typeLoader">A type loader.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="runtimeState">The runtime state.</param>
        /// <param name="configs">Optional configs.</param>
        /// <param name="ioHelper">An IOHelper</param>
        /// <param name="appCaches"></param>
        public Composition(IServiceCollection services, TypeLoader typeLoader, IProfilingLogger logger, IRuntimeState runtimeState, Configs configs, IIOHelper ioHelper, AppCaches appCaches)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            TypeLoader = typeLoader ?? throw new ArgumentNullException(nameof(typeLoader));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            RuntimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
            Configs = configs ?? throw new ArgumentNullException(nameof(configs));
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

        /// <summary>
        /// Gets the type loader.
        /// </summary>
        public TypeLoader TypeLoader { get; }

        /// <summary>
        /// Gets the runtime state.
        /// </summary>
        public IRuntimeState RuntimeState { get; }

        /// <summary>
        /// Gets the configurations.
        /// </summary>
        public Configs Configs { get; }

        #endregion

        #region IRegister
        public IServiceCollection Services { get; }

        [Obsolete("Please use Services instead")]
        public object Concrete => Services;

         // TODO: MDSI Not used in core, Nuke or keep for backwards compatability for user composers?
        public void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient)
            => Services.Register(serviceType, lifetime);

         // TODO: MDSI Not used in core, Nuke or keep for backwards compatability for user composers?
        public void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient)
            => Services.Register(serviceType, implementingType, lifetime);

        public void Register<TService>(Lifetime lifetime = Lifetime.Transient)
            where TService : class
            => Services.Register<TService>(lifetime);

        public void Register<TService, TTarget>(Lifetime lifetime = Lifetime.Transient)
            where TService : class
            => Services.Register<TService, TTarget>(lifetime);

        public void Register<TService>(Func<IServiceProvider, TService> serviceProvider, Lifetime lifetime = Lifetime.Transient)
            where TService : class
            => Services.Register(serviceProvider, lifetime);

         // TODO: MDSI Not used in core, Nuke or keep for backwards compatability for user composers?
        public void Register(Type serviceType, object instance)
            => Services.Register(serviceType, instance);

         // TODO: MDSI Not used in core, Nuke or keep for backwards compatability for user composers?
        public void RegisterFor<TService, TTarget>(Lifetime lifetime = Lifetime.Transient)
            where TService : class
            => Services.RegisterFor<TService, TTarget>(lifetime);

         // TODO: MDSI Not used in core, Nuke or keep for backwards compatability for user composers?
        public void RegisterFor<TService, TTarget>(Type implementingType, Lifetime lifetime = Lifetime.Transient)
            where TService : class
            => Services.RegisterFor<TService>(implementingType, lifetime);

         // TODO: MDSI Not used in core, Nuke or keep for backwards compatability for user composers?
        public void RegisterFor<TService>(Func<IServiceProvider, TService> serviceProvider, Lifetime lifetime = Lifetime.Transient)
            where TService : class
            => Services.RegisterFor(serviceProvider, lifetime);

         // TODO: MDSI Not used in core, Nuke or keep for backwards compatability for user composers?
        public void RegisterFor<TService, TTarget>(TService instance)
            where TService : class
            => Services.RegisterFor<TService>(instance);

        public void RegisterBuildersAndConfigs()
        {
            foreach (var builder in _builders.Values)
                builder.RegisterWith(Services);
            _builders.Clear(); // no point keep them around

            Configs.RegisterWith(Services);
        }

        #endregion

        #region Unique

        /// <summary>
        /// Registers a unique service with an implementation type.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public void RegisterUnique(Type serviceType, Type implementingType)
            => Services.RegisterFor(serviceType, implementingType, Lifetime.Singleton);

        /// <summary>
        /// Registers a unique service with an implementation factory.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public void RegisterUnique<TService>(Func<IServiceProvider, TService> serviceProvider)
            where TService : class
            => Services.RegisterFor(serviceProvider, Lifetime.Singleton);

        /// <summary>
        /// Registers a unique service with an implementing instance.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public void RegisterUnique(Type serviceType, object instance)
            => Services.RegisterFor(serviceType, instance);

        /// <summary>
        /// Registers a unique service for a target, with an implementation factory.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public void RegisterUniqueFor<TService, TTarget>(Func<IServiceProvider, TService> serviceProvider)
            where TService : class
            => Services.RegisterFor(serviceProvider, Lifetime.Singleton);

        /// <summary>
        /// Registers a unique service for a target, with an implementing instance.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        // TODO: MDSI Not used in core, Nuke or keep for backwards compat for user composers?
        public void RegisterUniqueFor<TService, TTarget>(TService instance)
            where TService : class
            => Services.RegisterFor(typeof(TService), instance);

        #endregion

        #region Collection Builders

        /// <summary>
        /// Gets a collection builder (and registers the collection).
        /// </summary>
        /// <typeparam name="TBuilder">The type of the collection builder.</typeparam>
        /// <returns>The collection builder.</returns>
        public TBuilder WithCollectionBuilder<TBuilder>()
            where TBuilder : ICollectionBuilder, new()
        {
            var typeOfBuilder = typeof(TBuilder);

            if (_builders.TryGetValue(typeOfBuilder, out var o))
                return (TBuilder)o;

            var builder = new TBuilder();
            _builders[typeOfBuilder] = builder;
            return builder;
        }

        #endregion
    }
}
