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
        }
    }
}
