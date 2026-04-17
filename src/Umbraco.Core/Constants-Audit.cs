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

        /// <summary>
        ///     The scope context enlistment key used by <see cref="Services.IAuditTriggerAccessor" /> to store
        ///     the ambient <see cref="Models.AuditTrigger" />.
        /// </summary>
        public const string TriggerEnlistmentKey = "Umbraco.AuditTrigger";

        /// <summary>
        ///     The maximum length for trigger source and trigger operation fields in audit logs.
        /// </summary>
        public const int TriggerFieldLength = 100;

        /// <summary>
        ///     The maximum length for the type alias field in audit logs.
        /// </summary>
        public const int TypeAliasLength = 100;

        /// <summary>
        ///     Well-known trigger source values identifying the software or package
        ///     that triggered an audited action.
        /// </summary>
        public static class TriggerSources
        {
            /// <summary>
            ///     The Umbraco CMS core.
            /// </summary>
            public const string Core = "Core";
        }

        /// <summary>
        ///     Well-known trigger operation values identifying what initiated an audited action.
        /// </summary>
        public static class TriggerOperations
        {
            /// <summary>
            ///     Content published by a scheduled publish background task.
            /// </summary>
            public const string ScheduledPublish = "ScheduledPublish";

            /// <summary>
            ///     Content rolled back to a previous version.
            /// </summary>
            public const string Rollback = "Rollback";

            /// <summary>
            ///     Content installed via a package.
            /// </summary>
            public const string PackageInstall = "PackageInstall";
        }
    }
}
