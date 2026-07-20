namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains constants for server-sent events (SSE) used for real-time notifications.
    /// </summary>
    public static class ServerEvents
    {
        /// <summary>
        ///     Contains event source identifiers for different entity types.
        /// </summary>
        public static class EventSource
        {
            /// <summary>
            ///     The event source identifier for document events.
            /// </summary>
            public const string Document = "Umbraco:CMS:Document";

            /// <summary>
            ///     The event source identifier for document blueprint events.
            /// </summary>
            public const string DocumentBlueprint = "Umbraco:CMS:DocumentBlueprint";

            /// <summary>
            ///     The event source identifier for document type events.
            /// </summary>
            public const string DocumentType = "Umbraco:CMS:DocumentType";

            /// <summary>
            ///     The event source identifier for media events.
            /// </summary>
            public const string Media = "Umbraco:CMS:Media";

            /// <summary>
            ///     The event source identifier for media type events.
            /// </summary>
            public const string MediaType = "Umbraco:CMS:MediaType";

            /// <summary>
            ///     The event source identifier for member events.
            /// </summary>
            public const string Member = "Umbraco:CMS:Member";

            /// <summary>
            ///     The event source identifier for member type events.
            /// </summary>
            public const string MemberType = "Umbraco:CMS:MemberType";

            /// <summary>
            ///     The event source identifier for member group events.
            /// </summary>
            public const string MemberGroup = "Umbraco:CMS:MemberGroup";

            /// <summary>
            ///     The event source identifier for data type events.
            /// </summary>
            public const string DataType = "Umbraco:CMS:DataType";

            /// <summary>
            ///     The event source identifier for language events.
            /// </summary>
            public const string Language = "Umbraco:CMS:Language";

            /// <summary>
            ///     The event source identifier for script events.
            /// </summary>
            public const string Script = "Umbraco:CMS:Script";

            /// <summary>
            ///     The event source identifier for stylesheet events.
            /// </summary>
            public const string Stylesheet = "Umbraco:CMS:Stylesheet";

            /// <summary>
            ///     The event source identifier for template events.
            /// </summary>
            public const string Template = "Umbraco:CMS:Template";

            /// <summary>
            ///     The event source identifier for dictionary item events.
            /// </summary>
            public const string DictionaryItem = "Umbraco:CMS:DictionaryItem";

            /// <summary>
            ///     The event source identifier for domain events.
            /// </summary>
            public const string Domain = "Umbraco:CMS:Domain";

            /// <summary>
            ///     The event source identifier for partial view events.
            /// </summary>
            public const string PartialView = "Umbraco:CMS:PartialView";

            /// <summary>
            ///     The event source identifier for public access entry events.
            /// </summary>
            public const string PublicAccessEntry = "Umbraco:CMS:PublicAccessEntry";

            /// <summary>
            ///     The event source identifier for relation events.
            /// </summary>
            public const string Relation = "Umbraco:CMS:Relation";

            /// <summary>
            ///     The event source identifier for relation type events.
            /// </summary>
            public const string RelationType = "Umbraco:CMS:RelationType";

            /// <summary>
            ///     The event source identifier for user group events.
            /// </summary>
            public const string UserGroup = "Umbraco:CMS:UserGroup";

            /// <summary>
            ///     The event source identifier for user events.
            /// </summary>
            public const string User = "Umbraco:CMS:User";

            /// <summary>
            ///     The event source identifier for current user events.
            /// </summary>
            public const string CurrentUser = "Umbraco:CMS:CurrentUser";

            /// <summary>
            ///     The event source identifier for webhook events.
            /// </summary>
            public const string Webhook = "Umbraco:CMS:Webhook";
        }

        /// <summary>
        ///     Contains event type identifiers for entity lifecycle events.
        /// </summary>
        public static class EventType
        {
            // TODO (V18): Convert these statics to consts to comply with SA1401.

            /// <summary>
            ///     The event type for entity creation events.
            /// </summary>
            public static string Created = "Created";

            /// <summary>
            ///     The event type for entity update events.
            /// </summary>
            public static string Updated = "Updated";

            /// <summary>
            ///     The event type for entity deletion events.
            /// </summary>
            public static string Deleted = "Deleted";

            /// <summary>
            ///     The event type for entity trashed events.
            /// </summary>
            public static string Trashed = "Trashed";
        }
    }
}
