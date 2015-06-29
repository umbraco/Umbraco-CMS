using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// The resolver to return the currently registered IServerRegistrar object
    /// </summary>
    public sealed class ServerRegistrarResolver : SingleObjectResolverBase<ServerRegistrarResolver, IServerRegistrar>
    {

        internal ServerRegistrarResolver(IServerRegistrar factory)
            : base(factory)
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