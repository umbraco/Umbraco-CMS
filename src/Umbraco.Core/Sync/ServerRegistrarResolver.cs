using System;
using System.Linq.Expressions;
using LightInject;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Resolves the IServerRegistrar object.
    /// </summary>
    public sealed class ServerRegistrarResolver : ContainerSingleObjectResolver<ServerRegistrarResolver, IServerRegistrar>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRegistrarResolver"/> class with a registrar.
        /// </summary>
        /// <param name="value">An instance of a registrar.</param>
        /// <remarks>The resolver is created by the <c>CoreBootManager</c> and thus the constructor remains internal.</remarks>
        public ServerRegistrarResolver(IServerRegistrar value) 
            : base(value)
        { }

        internal ServerRegistrarResolver(IServiceContainer container)
            : base(container)
        { }

        internal ServerRegistrarResolver(IServiceContainer container, Func<IServiceFactory, IServerRegistrar> implementationType)
            : base(container, implementationType)
        { }

        /// <summary>
        /// Sets the registrar.
        /// </summary>
        /// <param name="serverRegistrar">The registrar.</param>
        /// <remarks>For developers, at application startup.</remarks>
        public void SetServerRegistrar(IServerRegistrar serverRegistrar)
        {
            Value = serverRegistrar;
        }

        /// <summary>
        /// Gets the registrar.
        /// </summary>
        public IServerRegistrar Registrar
        {
            get { return Value; }
        }

    }
}