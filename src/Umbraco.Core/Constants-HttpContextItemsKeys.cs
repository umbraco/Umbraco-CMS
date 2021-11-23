namespace Umbraco.Cms.Core
{
    public static partial class Constants
    {
        /// <summary>
        /// Defines keys for items stored in HttpContext.Items
        /// </summary>
        public static class HttpContextItems
        {
            /// <summary>
            /// Key for current requests body deserialized as JObject.
            /// </summary>
            public const string RequestBodyAsJObject = "RequestBodyAsJObject";
        }
    }
}
