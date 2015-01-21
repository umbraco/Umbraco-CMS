using System;
using System.Linq.Expressions;
using Umbraco.Core.LightInject;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// The resolver to return the currently registered IServerRegistrar object
    /// </summary>
    public sealed class ServerRegistrarResolver : ContainerSingleObjectResolver<ServerRegistrarResolver, IServerRegistrar>
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="SingleObjectResolverBase{TResolver, TResolved}"/> class with an instance of the resolved object.
        /// </summary>
        /// <param name="value">An instance of the resolved object.</param>
        /// <remarks>By default <c>CanBeNull</c> is false, so <c>value</c> has to be non-null, or <c>Value</c> has to be
        /// initialized before being accessed, otherwise an exception will be thrown when reading it.</remarks>
        public ServerRegistrarResolver(IServerRegistrar value) : base(value)
        {
        }

        internal ServerRegistrarResolver(IServiceContainer container, Type implementationType)
            : base(container, implementationType)
        {
        }

        internal ServerRegistrarResolver(IServiceContainer container, Expression<Func<IServiceFactory, IServerRegistrar>> implementationType)
            : base(container, implementationType)
        {
        }

        /// <summary>
        /// Can be used at runtime to set a custom IServerRegistrar at app startup
        /// </summary>
        /// <param name="serverRegistrar"></param>
        public void SetServerRegistrar(IServerRegistrar serverRegistrar)
        {
            Value = serverRegistrar;
        }

        public IServerRegistrar Registrar
        {
            get { return Value; }
        }

    }
}