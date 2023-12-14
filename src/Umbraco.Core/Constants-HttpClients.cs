namespace Umbraco.Cms.Core;

/// <summary>
///     Defines constants.
/// </summary>
public static partial class Constants
{
    /// <summary>
    ///     Defines constants for named http clients.
    /// </summary>
    public static class HttpClients
    {
        /// <summary>
        ///     Name for http client which ignores certificate errors.
        /// </summary>
        public const string IgnoreCertificateErrors = "Umbraco:HttpClients:IgnoreCertificateErrors";

        /// <summary>
        ///     Name for http client which sends webhook requests.
        /// </summary>
        public const string WebhookFiring = "Umbraco:HttpClients:WebhookFiring";

        public static class Headers
        {
            /// <summary>
            ///     User agent name for the product name.
            /// </summary>
            public const string UserAgentProductName = "Umbraco-Cms";
        }
    }
}
