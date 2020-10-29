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
        void Register<TService>(Func<IServiceProvider, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class;

        /// <summary>
        /// Registers a service with an implementing instance.
        /// </summary>
        void Register(Type serviceType, object instance);
    }
}
