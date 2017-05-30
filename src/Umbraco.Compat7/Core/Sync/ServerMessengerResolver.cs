using Umbraco.Core.Composing;
using CoreCurrent = Umbraco.Core.Composing.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Sync
{
    public class ServerMessengerResolver
    {
        private ServerMessengerResolver()
        { }

        public static bool HasCurrent => true;

        public static ServerMessengerResolver Current { get; }
            = new ServerMessengerResolver();

        public IServerMessenger Messenger => CoreCurrent.ServerMessenger;

        public void SetServerMessenger(IServerMessenger messenger)
        {
            CoreCurrent.Container.RegisterSingleton(_ => messenger);
        }
    }
}
