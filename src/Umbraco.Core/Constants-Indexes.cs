namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains constants for Umbraco Examine search index names.
    /// </summary>
    public static class UmbracoIndexes
    {
        /// <summary>
        ///     The name of the internal content index (includes unpublished content).
        /// </summary>
        public const string InternalIndexName = "InternalIndex";

        /// <summary>
        ///     The name of the external content index (published content only).
        /// </summary>
        public const string ExternalIndexName = "ExternalIndex";

        /// <summary>
        ///     The name of the members index.
        /// </summary>
        public const string MembersIndexName = "MembersIndex";

        /// <summary>
        ///     The name of the Delivery API content index.
        /// </summary>
        public const string DeliveryApiContentIndexName = "DeliveryApiContentIndex";
    }
}
