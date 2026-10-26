namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains HTTP header name constants used by Umbraco.
    /// </summary>
    public static class Headers
    {
        /// <summary>
        ///     Location header name.
        /// </summary>
        public const string Location = "Location";

        /// <summary>
        ///     Generated resource identifier header name.
        /// </summary>
        public const string GeneratedResource = "Umb-Generated-Resource";

        /// <summary>
        ///     Response notifications header name.
        /// </summary>
        public const string Notifications = "Umb-Notifications";

        /// <summary>
        ///     Schema lockdown denial header name. Present only on a response schema lockdown denied, naming the
        ///     entity type and the operation it was denied for.
        /// </summary>
        public const string SchemaLockdown = "Umb-Schema-Lockdown";
    }
}
