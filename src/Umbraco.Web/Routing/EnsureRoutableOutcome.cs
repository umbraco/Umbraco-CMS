namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Represents the outcome of trying to route an incoming request.
    /// </summary>
    internal enum EnsureRoutableOutcome
    {
        /// <summary>
        /// Request routes to a document.
        /// </summary>
        /// <remarks>
        /// <para>Umbraco was ready and configured, and has content.</para>
        /// <para>The request looks like it can be a route to a document. This does not
        /// mean that there *is* a matching document, ie the request might end up returning
        /// 404.</para>
        /// </remarks>
        IsRoutable = 0,

        /// <summary>
        /// Request does not route to a document.
        /// </summary>
        /// <remarks>
        /// <para>Umbraco was ready and configured, and has content.</para>
        /// <para>The request does not look like it can be a route to a document. Can be
        /// anything else eg back-office, surface controller...</para>
        /// </remarks>
        NotDocumentRequest = 10,

        /// <summary>
        /// Umbraco was not ready.
        /// </summary>
        NotReady = 11,

        /// <summary>
        /// Umbraco was not configured.
        /// </summary>
        NotConfigured = 12,

        /// <summary>
        /// There was no content at all.
        /// </summary>
        NoContent = 13
    }
}