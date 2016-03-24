﻿using System;
using System.Linq.Expressions;
using LightInject;

namespace Umbraco.Core.ObjectResolution
{

    /// <summary>
    /// A single object resolver that can be configured to use IoC to instantiate and wire up the object
    /// </summary>
    /// <typeparam name="TResolver"></typeparam>
    /// <typeparam name="TResolved"></typeparam>
    public abstract class ContainerSingleObjectResolver<TResolver, TResolved> : SingleObjectResolverBase<TResolver, TResolved>
        where TResolved : class
        where TResolver : ResolverBase
    {
        private readonly IServiceContainer _container;


        #region Constructors used for test - ONLY so that a container is not required and will just revert to using the normal SingleObjectResolverBase

        [Obsolete("Used for tests only - should remove")]
        internal ContainerSingleObjectResolver()
        {
        }

        [Obsolete("Used for tests only - should remove")]
        internal ContainerSingleObjectResolver(TResolved value)
            : base(value)
        {
        }

        [Obsolete("Used for tests only - should remove")]
        internal ContainerSingleObjectResolver(bool canBeNull)
            : base(canBeNull)
        {
        }

        [Obsolete("Used for tests only - should remove")]
        internal ContainerSingleObjectResolver(TResolved value, bool canBeNull)
            : base(value, canBeNull)
        {
        } 
        #endregion

        /// <summary>
        /// Initializes the resolver to use IoC
        /// </summary>
        /// <param name="container"></param>
        /// <param name="implementationType"></param>
        internal ContainerSingleObjectResolver(IServiceContainer container, Type implementationType)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (implementationType == null) throw new ArgumentNullException("implementationType");
            _container = container;
            _container.Register(typeof(TResolved), implementationType, new PerContainerLifetime());
        }

        /// <summary>
        /// Initialize the resolver to use IoC, when using this contructor the type must be set manually
        /// </summary>
        /// <param name="container"></param>
        internal ContainerSingleObjectResolver(IServiceContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        /// Initializes the resolver to use IoC
        /// </summary>
        /// <param name="container"></param>
        /// <param name="implementationType"></param>
        internal ContainerSingleObjectResolver(IServiceContainer container, Func<IServiceFactory, TResolved> implementationType)
        {
            _container = container;
            _container.Register<TResolved>(implementationType, new PerContainerLifetime());
        }

        /// <summary>
        /// Gets or sets the resolved object instance.
        /// </summary>
        /// <remarks></remarks>
        /// <exception cref="ArgumentNullException">value is set to null, but cannot be null (<c>CanBeNull</c> is <c>false</c>).</exception>
        /// <exception cref="InvalidOperationException">value is read and is null, but cannot be null (<c>CanBeNull</c> is <c>false</c>),
        /// or value is set (read) and resolution is (not) frozen.</exception>
        protected override TResolved Value
        {
            get
            {
                if (_container == null) return base.Value;
                return _container.GetInstance<TResolved>();
            }
            set
            {
                if (_container != null)
                {
                    _container.Override(
                        sr => sr.ServiceType == typeof (TResolved),
                        (factory, registration) =>
                        {
                            registration.Value = value;
                            registration.Lifetime = new PerContainerLifetime();
                            return registration;
                        });
                }
                base.Value = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the resolved object instance is null.
        /// </summary>
        public override bool HasValue
        {
            get
            {
                if (_container == null) return base.HasValue;   
                return (_container.TryGetInstance<TResolved>() == null) == false;
            }
        }
    }
}