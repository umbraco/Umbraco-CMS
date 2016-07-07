using System;
using System.Linq.Expressions;
using LightInject;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Resolves the IServerMessenger object.
    /// </summary>
    public sealed class ServerMessengerResolver : ContainerSingleObjectResolver<ServerMessengerResolver, IServerMessenger>
    {
        internal ServerMessengerResolver(IServiceContainer container)
            : base(container)
        { }

        internal ServerMessengerResolver(IServiceContainer container, Func<IServiceFactory, IServerMessenger> implementationType)
            : base(container, implementationType)
        { }

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