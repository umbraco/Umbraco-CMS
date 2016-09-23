using Umbraco.Core.DependencyInjection;
using CoreCurrent = Umbraco.Core.DependencyInjection.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Sync
{
    public class ServerRegistrarResolver
    {
        private ServerRegistrarResolver()
        { }

        public static bool HasCurrent => true;

        public static ServerRegistrarResolver Current { get; }
            = new ServerRegistrarResolver();

        public IServerRegistrar Registrar => CoreCurrent.ServerRegistrar;

        public void SetServerRegistrar(IServerRegistrar registrar)
        {
            CoreCurrent.Container.RegisterSingleton(_ => registrar);
        }
    }
}
