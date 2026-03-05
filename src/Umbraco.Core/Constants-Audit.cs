namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains constants for audit logging.
    /// </summary>
    public static class Audit
    {
        /// <summary>
        ///     The maximum length for IP address fields in audit logs.
        /// </summary>
        public const int IpLength = 64;

        /// <summary>
        ///     The maximum length for event type fields in audit logs.
        /// </summary>
        public const int EventTypeLength = 256;

        /// <summary>
        ///     The maximum length for details fields in audit logs.
        /// </summary>
        public const int DetailsLength = 1024;
    }
}
