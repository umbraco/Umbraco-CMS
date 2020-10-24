using System;
using System.Collections.Generic;
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
    public class Composition : IRegister
    {
        private readonly Dictionary<Type, ICollectionBuilder> _builders = new Dictionary<Type, ICollectionBuilder>();
        private readonly Dictionary<string, Action<IRegister>> _uniques = new Dictionary<string, Action<IRegister>>();
        private readonly IRegister _register;

        /// <summary>
        /// Initializes a new instance of the <see cref="Composition"/> class.
        /// </summary>
        /// <param name="register">A register.</param>
        /// <param name="typeLoader">A type loader.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="runtimeState">The runtime state.</param>
        /// <param name="configs">Optional configs.</param>
        /// <param name="ioHelper">An IOHelper</param>
        /// <param name="appCaches"></param>
        public Composition(IRegister register, TypeLoader typeLoader, IProfilingLogger logger, IRuntimeState runtimeState, IIOHelper ioHelper, AppCaches appCaches)
        {
            _register = register ?? throw new ArgumentNullException(nameof(register));
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
        public IFactory CreateFactory()
        {
            foreach (var onCreating in OnCreatingFactory.Values)
                onCreating();

            foreach (var unique in _uniques.Values)
                unique(_register);
            _uniques.Clear(); // no point keep them around

            foreach (var builder in _builders.Values)
                builder.RegisterWith(_register);
            _builders.Clear(); // no point keep them around

            return _register.CreateFactory();
        }

        /// <summary>
        /// Gets a dictionary of action to execute when creating the factory.
        /// </summary>
        public Dictionary<string, Action> OnCreatingFactory { get; } = new Dictionary<string, Action>();

        #endregion

        #region Unique

        private string GetUniqueName<TService>()
            => GetUniqueName(typeof(TService));

        private string GetUniqueName(Type serviceType)
            => serviceType.FullName;

        private string GetUniqueName<TService, TTarget>()
            => GetUniqueName(typeof(TService), typeof(TTarget));

        private string GetUniqueName(Type serviceType, Type targetType)
            => serviceType.FullName + "::" + targetType.FullName;

        /// <summary>
        /// Registers a unique service as its own implementation.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public void RegisterUnique(Type serviceType)
            => _uniques[GetUniqueName(serviceType)] = register => register.Register(serviceType, Lifetime.Singleton);

        /// <summary>
        /// Registers a unique service with an implementation type.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public void RegisterUnique(Type serviceType, Type implementingType)
            => _uniques[GetUniqueName(serviceType)] = register => register.Register(serviceType, implementingType, Lifetime.Singleton);

        /// <summary>
        /// Registers a unique service with an implementation factory.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public void RegisterUnique<TService>(Func<IFactory, TService> factory)
            where TService : class
            => _uniques[GetUniqueName<TService>()] = register => register.Register<TService>(factory, Lifetime.Singleton);

        /// <summary>
        /// Registers a unique service with an implementing instance.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public void RegisterUnique(Type serviceType, object instance)
            => _uniques[GetUniqueName(serviceType)] = register => register.Register(serviceType, instance);

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
}
