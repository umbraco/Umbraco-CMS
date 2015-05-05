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
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerMessengerResolver"/> class with a messenger.
        /// </summary>
        /// <param name="factory">An instance of a messenger.</param>
        /// <remarks>The resolver is created by the <c>CoreBootManager</c> and thus the constructor remains internal.</remarks>
        /// initialized before being accessed, otherwise an exception will be thrown when reading it.</remarks>
        public ServerMessengerResolver(IServerMessenger value) : base(value)
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