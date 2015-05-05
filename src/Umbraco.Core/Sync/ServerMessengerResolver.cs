using System;
using System.Linq.Expressions;
using Umbraco.Core.LightInject;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Resolves the IServerMessenger object.
    /// </summary>
    public sealed class ServerMessengerResolver : ContainerSingleObjectResolver<ServerMessengerResolver, IServerMessenger>
    {
        internal ServerMessengerResolver(IServiceContainer container, Type implementationType)
            : base(container, implementationType)
        {
        }

        internal ServerMessengerResolver(IServiceContainer container, Expression<Func<IServiceFactory, IServerMessenger>> implementationType)
            : base(container, implementationType)
        {
        }

        /// <summary>
        /// Sets the messenger.
        /// </summary>
        /// <param name="serverMessenger">The messenger.</param>
        public void SetServerMessenger(IServerMessenger serverMessenger)
        {
            Value = serverMessenger;
        }

        /// <summary>
        /// Gets the messenger.
        /// </summary>
        public IServerMessenger Messenger
        {
            get { return Value; }
        }
    }
}