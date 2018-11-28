using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

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
    public class Composition : IRegister
    {
        private readonly Dictionary<Type, ICollectionBuilder> _builders = new Dictionary<Type, ICollectionBuilder>();
        private readonly IRegister _register;

        /// <summary>
        /// Initializes a new instance of the <see cref="Composition"/> class.
        /// </summary>
        /// <param name="register">A register.</param>
        /// <param name="typeLoader">A type loader.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="level">The runtime level.</param>
        public Composition(IRegister register, TypeLoader typeLoader, IProfilingLogger logger, RuntimeLevel level)
        {
            _register = register;
            TypeLoader = typeLoader;
            Logger = logger;
            RuntimeLevel = level;
        }

        #region Services

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public IProfilingLogger Logger { get; }

        /// <summary>
        /// Gets the type loader.
        /// </summary>
        public TypeLoader TypeLoader { get; }

        /// <summary>
        /// Gets the runtime level.
        /// </summary>
        public RuntimeLevel RuntimeLevel { get; }

        #endregion

        #region IRegister

        /// <inheritdoc />
        public object ConcreteContainer => _register.ConcreteContainer;

        /// <inheritdoc />
        public void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient)
            => _register.Register(serviceType, lifetime);

        /// <inheritdoc />
        public void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient)
            => _register.Register(serviceType, implementingType, lifetime);

        /// <inheritdoc />
        public void Register<TService>(Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
            => _register.Register(factory, lifetime);

        /// <inheritdoc />
        public void RegisterInstance(Type serviceType, object instance)
            => _register.RegisterInstance(serviceType, instance);

        /// <inheritdoc />
        public void RegisterAuto(Type serviceBaseType)
            => _register.RegisterAuto(serviceBaseType);

        /// <inheritdoc />
        public IContainer ConfigureForWeb()
            => _register.ConfigureForWeb();

        /// <inheritdoc />
        public IContainer EnablePerWebRequestScope()
            => _register.EnablePerWebRequestScope();

        #endregion

        #region Collection Builders

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
            builder.Initialize(_register);

            _builders[typeOfBuilder] = builder;

            return builder;
        }

        #endregion
    }
}
