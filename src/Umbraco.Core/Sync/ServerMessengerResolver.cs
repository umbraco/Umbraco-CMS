using System;
using System.Linq.Expressions;
using Umbraco.Core.LightInject;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A resolver to return the currently registered IServerMessenger object
    /// </summary>
    public sealed class ServerMessengerResolver : ContainerSingleObjectResolver<ServerMessengerResolver, IServerMessenger>
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="SingleObjectResolverBase{TResolver, TResolved}"/> class with an instance of the resolved object.
        /// </summary>
        /// <param name="value">An instance of the resolved object.</param>
        /// <remarks>By default <c>CanBeNull</c> is false, so <c>value</c> has to be non-null, or <c>Value</c> has to be
        /// initialized before being accessed, otherwise an exception will be thrown when reading it.</remarks>
        public ServerMessengerResolver(IServerMessenger value) : base(value)
        {
        }

        internal ServerMessengerResolver(IServiceContainer container, Type implementationType)
            : base(container, implementationType)
        {
        }

        internal ServerMessengerResolver(IServiceContainer container, Expression<Func<IServiceFactory, IServerMessenger>> implementationType)
            : base(container, implementationType)
        {
        }

        /// <summary>
        /// Can be used at runtime to set a custom IServerMessenger at app startup
        /// </summary>
        /// <param name="serverMessenger"></param>
        public void SetServerMessenger(IServerMessenger serverMessenger)
        {
            Value = serverMessenger;
        }

        public IServerMessenger Messenger
        {
            get { return Value; }
        }
    }
}