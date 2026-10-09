namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Defines routing constants.
    /// </summary>
    public static class Routing
    {
        /// <summary>
        ///     Represents the route of unroutable content.
        /// </summary>
        public const string Unroutable = "#";

        /// <summary>
        ///     Represents the route returned when a URL provider throws an exception while resolving content.
        /// </summary>
        public const string UrlProviderException = "#ex";
    }
}
