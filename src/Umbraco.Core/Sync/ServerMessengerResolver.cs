using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A resolver to return the currently registered IServerMessenger object
    /// </summary>
    public sealed class ServerMessengerResolver : SingleObjectResolverBase<ServerMessengerResolver, IServerMessenger>
    {
        internal ServerMessengerResolver(IServerMessenger factory)
            : base(factory)
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