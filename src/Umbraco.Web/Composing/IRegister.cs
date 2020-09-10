﻿using System;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Defines a service register for Umbraco.
    /// </summary>
    /// <remarks>
    /// Moved from Umbraco Core to keep the .net 472 projects building.
    /// </remarks>
    [Obsolete("Use IServiceCollection instead")]
    public interface IRegister : IServiceCollection
    {
        /// <summary>
        /// Gets the concrete container.
        /// </summary>
        object Concrete { get; }

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
        void Register<TService>(Func<IServiceProvider, TService> serviceProvider, Lifetime lifetime = Lifetime.Transient)
            where TService : class;

        /// <summary>
        /// Registers a service with an implementing instance.
        /// </summary>
        void Register(Type serviceType, object instance);

        /// <summary>
        /// Registers a service for a target, as its own implementation.
        /// </summary>
        /// <remarks>
        /// There can only be one implementation or instanced registered for a service and target;
        /// what happens if many are registered is not specified.
        /// </remarks>
        void RegisterFor<TService, TTarget>(Lifetime lifetime = Lifetime.Transient)
            where TService : class;

        /// <summary>
        /// Registers a service for a target, with an implementation type.
        /// </summary>
        /// <remarks>
        /// There can only be one implementation or instanced registered for a service and target;
        /// what happens if many are registered is not specified.
        /// </remarks>
        void RegisterFor<TService, TTarget>(Type implementingType, Lifetime lifetime = Lifetime.Transient)
            where TService : class;

        /// <summary>
        /// Registers a service for a target, with an implementing instance.
        /// </summary>
        /// <remarks>
        /// There can only be one implementation or instanced registered for a service and target;
        /// what happens if many are registered is not specified.
        /// </remarks>
        void RegisterFor<TService, TTarget>(TService instance)
            where TService : class;

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
        /// Creates the factory.
        /// </summary>
        IFactory CreateFactory();

        #endregion
    }
}
