namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class HttpContext
    {
        /// <summary>
        ///     Defines keys for items stored in HttpContext.Items
        /// </summary>
        public static class Items
        {
            /// <summary>
            ///     Key for current requests body deserialized as JObject.
            /// </summary>
            public const string RequestBodyAsJObject = "RequestBodyAsJObject";

            /// <summary>
            ///     Key for the CSP nonce value stored per-request.
            /// </summary>
            public const string CspNonce = "Umbraco.CspNonce";
        }
    }
}
