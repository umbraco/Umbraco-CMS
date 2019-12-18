using System;
using System.Reflection;

namespace Umbraco.ModelsBuilder.Api
{
    /// <summary>
    /// Manages API version handshake between client and server.
    /// </summary>
    public class ApiVersion
    {
        #region Configure

        // indicate the minimum version of the client API that is supported by this server's API.
        //   eg our Version = 4.8 but we support connections from VSIX down to version 3.2
        //   => as a server, we accept connections from client down to version ...
        private static readonly Version MinClientVersionSupportedByServerConst = new Version(3, 0, 0, 0);

        // indicate the minimum version of the server that can support the client API
        //   eg our Version = 4.8 and we know we're compatible with website server down to version 3.2
        //   => as a client, we tell the server down to version ... that it should accept us
        private static readonly Version MinServerVersionSupportingClientConst = new Version(3, 0, 0, 0);

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersion"/> class.
        /// </summary>
        /// <param name="executingVersion">The currently executing version.</param>
        /// <param name="minClientVersionSupportedByServer">The min client version supported by the server.</param>
        /// <param name="minServerVersionSupportingClient">An opt min server version supporting the client.</param>
        internal ApiVersion(Version executingVersion, Version minClientVersionSupportedByServer, Version minServerVersionSupportingClient = null)
        {
            if (executingVersion == null) throw new ArgumentNullException(nameof(executingVersion));
            if (minClientVersionSupportedByServer == null) throw new ArgumentNullException(nameof(minClientVersionSupportedByServer));

            Version = executingVersion;
            MinClientVersionSupportedByServer = minClientVersionSupportedByServer;
            MinServerVersionSupportingClient = minServerVersionSupportingClient;
        }

        /// <summary>
        /// Gets the currently executing API version.
        /// </summary>
        public static ApiVersion Current { get; }
            = new ApiVersion(Assembly.GetExecutingAssembly().GetName().Version,
                MinClientVersionSupportedByServerConst, MinServerVersionSupportingClientConst);

        /// <summary>
        /// Gets the executing version of the API.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Gets the min client version supported by the server.
        /// </summary>
        public Version MinClientVersionSupportedByServer { get; }

        /// <summary>
        /// Gets the min server version supporting the client.
        /// </summary>
        public Version MinServerVersionSupportingClient { get; }

        /// <summary>
        /// Gets a value indicating whether the API server is compatible with a client.
        /// </summary>
        /// <param name="clientVersion">The client version.</param>
        /// <param name="minServerVersionSupportingClient">An opt min server version supporting the client.</param>
        /// <remarks>
        /// <para>A client is compatible with a server if the client version is greater-or-equal _minClientVersionSupportedByServer
        /// (ie client can be older than server, up to a point) AND the client version is lower-or-equal the server version
        /// (ie client cannot be more recent than server) UNLESS the server .</para>
        /// </remarks>
        public bool IsCompatibleWith(Version clientVersion, Version minServerVersionSupportingClient = null)
        {
            // client cannot be older than server's min supported version
            if (clientVersion < MinClientVersionSupportedByServer)
                return false;

            // if we know about this client (client is older than server), it is supported
            if (clientVersion <= Version) // if we know about this client (client older than server)
                return true;

            // if we don't know about this client (client is newer than server),
            // give server a chance to tell client it is, indeed, ok to support it
            return minServerVersionSupportingClient != null && minServerVersionSupportingClient <= Version;
        }
    }
}
