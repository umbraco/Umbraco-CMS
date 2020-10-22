using System;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Defines a service register for Umbraco.
    /// </summary>
    public interface IRegister
    {
        /// <summary>
        /// Registers a service as its own implementation.
        /// </summary>
        void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient);

        /// <summary>
        /// Registers a service with an implementation type.
        /// </summary>
        void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient);

        /// <summary>
        /// Registers a service with an implementation factory.
        /// </summary>
        void Register<TService>(Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class;

        /// <summary>
        /// Registers a service with an implementing instance.
        /// </summary>
        void Register(Type serviceType, object instance);
        
        /// <summary>
        /// Registers a base type for auto-registration.
        /// </summary>
        /// <remarks>
        /// <para>Auto-registration means that anytime the container is asked to create an instance
        /// of a type deriving from <paramref name="serviceBaseType"/>, it will first register that
        /// type automatically.</para>
        /// <para>This can be used for instance for views or controllers. Then, one just needs to
        /// register a common base class or interface, and the container knows how to create instances.</para>
        /// </remarks>
        void RegisterAuto(Type serviceBaseType);

        #region Control

        /// <summary>
        /// Configures the container for web support.
        /// </summary>
        /// <remarks>
        /// <para>Enables support for MVC, WebAPI, but *not* per-request scope. This is used early in the boot
        /// process, where anything "scoped" should not be linked to a web request.</para>
        /// </remarks>
        void ConfigureForWeb();  // TODO: Unsure if we need this anymore

        /// <summary>
        /// Creates the factory.
        /// </summary>
        IFactory CreateFactory();

        #endregion
    }
}
