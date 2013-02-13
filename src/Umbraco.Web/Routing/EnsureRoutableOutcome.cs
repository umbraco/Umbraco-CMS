namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Reasons a request was not routable on the front-end
    /// </summary>
    internal enum EnsureRoutableOutcome
    {
        IsRoutable = 0,
        NotDocumentRequest = 10,
        NotReady = 11,
        NotConfigured = 12,
        NoContent = 13
    }
}