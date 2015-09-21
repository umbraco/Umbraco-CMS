using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Resolves the IServerRegistrar object.
    /// </summary>
    public sealed class ServerRegistrarResolver : SingleObjectResolverBase<ServerRegistrarResolver, IServerRegistrar>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRegistrarResolver"/> class with a registrar.
        /// </summary>
        /// <param name="factory">An instance of a registrar.</param>
        /// <remarks>The resolver is created by the <c>CoreBootManager</c> and thus the constructor remains internal.</remarks>
        internal ServerRegistrarResolver(IServerRegistrar factory)
            : base(factory)
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