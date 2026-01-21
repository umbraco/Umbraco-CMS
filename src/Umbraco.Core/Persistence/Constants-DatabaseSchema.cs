// ReSharper disable once CheckNamespace

namespace Umbraco.Cms.Core;

/// <summary>
///     Provides partial constants for the Umbraco application.
/// </summary>
public static partial class Constants
{
    /// <summary>
    ///     Provides constants related to the Umbraco database schema.
    /// </summary>
    public static class DatabaseSchema
    {
        // TODO: Why aren't all table names with the same prefix?
        /// <summary>
        ///     The prefix used for Umbraco database table names.
        /// </summary>
        public const string TableNamePrefix = "umbraco";

        /// <summary>
        ///     Provides constants for common database column names.
        /// </summary>
        public static class Columns
        {
            // Defines constants for common field names used throughout the database, to ensure
            // casing is aligned wherever used.

            /// <summary>
            ///     The primary key column name "id".
            /// </summary>
            public const string PrimaryKeyNameId = "id";

            /// <summary>
            ///     The primary key column name "pk".
            /// </summary>
            public const string PrimaryKeyNamePk = "pk";

            /// <summary>
            ///     The primary key column name "key".
            /// </summary>
            public const string PrimaryKeyNameKey = "key";

            /// <summary>
            ///     The node identifier column name.
            /// </summary>
            public const string NodeIdName = "nodeId";

            /// <summary>
            ///     The unique identifier column name.
            /// </summary>
            public const string UniqueIdName = "uniqueId";
        }

        /// <summary>
        ///     Provides constants for Umbraco database table names.
        /// </summary>
        /// <summary>
        ///     Provides constants for Umbraco database table names.
        /// </summary>
        public static class Tables
        {
            /// <summary>
            ///     The lock table name.
            /// </summary>
            public const string Lock = TableNamePrefix + "Lock";

            /// <summary>
            ///     The log table name.
            /// </summary>
            public const string Log = TableNamePrefix + "Log";

            /// <summary>
            ///     The node table name.
            /// </summary>
            public const string Node = TableNamePrefix + "Node";

            /// <summary>
            ///     The node data table name.
            /// </summary>
            public const string NodeData = /*TableNamePrefix*/ "cms" + "ContentNu";

            /// <summary>
            ///     The content type table name.
            /// </summary>
            public const string ContentType = /*TableNamePrefix*/ "cms" + "ContentType";

            /// <summary>
            ///     The content child type (allowed content types) table name.
            /// </summary>
            public const string ContentChildType = /*TableNamePrefix*/ "cms" + "ContentTypeAllowedContentType";

            /// <summary>
            ///     The document type table name.
            /// </summary>
            public const string DocumentType = /*TableNamePrefix*/ "cms" + "DocumentType";

            /// <summary>
            ///     The element type tree table name.
            /// </summary>
            [Obsolete("Please use ContentTypeTree instead. Scheduled for removal in Umbraco 18.")]
            public const string ElementTypeTree = /*TableNamePrefix*/ "cms" + "ContentType2ContentType";

            /// <summary>
            ///     The content type tree (composition) table name.
            /// </summary>
            public const string ContentTypeTree = /*TableNamePrefix*/ "cms" + "ContentType2ContentType";

            /// <summary>
            ///     The data type table name.
            /// </summary>
            public const string DataType = TableNamePrefix + "DataType";

            /// <summary>
            ///     The template table name.
            /// </summary>
            public const string Template = /*TableNamePrefix*/ "cms" + "Template";

            /// <summary>
            ///     The content table name.
            /// </summary>
            public const string Content = TableNamePrefix + "Content";

            /// <summary>
            ///     The content version table name.
            /// </summary>
            public const string ContentVersion = TableNamePrefix + "ContentVersion";

            /// <summary>
            ///     The content version culture variation table name.
            /// </summary>
            public const string ContentVersionCultureVariation = TableNamePrefix + "ContentVersionCultureVariation";

            /// <summary>
            ///     The content version cleanup policy table name.
            /// </summary>
            public const string ContentVersionCleanupPolicy = TableNamePrefix + "ContentVersionCleanupPolicy";

            /// <summary>
            ///     The document table name.
            /// </summary>
            public const string Document = TableNamePrefix + "Document";

            /// <summary>
            ///     The document culture variation table name.
            /// </summary>
            public const string DocumentCultureVariation = TableNamePrefix + "DocumentCultureVariation";

            /// <summary>
            ///     The document version table name.
            /// </summary>
            public const string DocumentVersion = TableNamePrefix + "DocumentVersion";

            /// <summary>
            ///     The document URL table name.
            /// </summary>
            public const string DocumentUrl = TableNamePrefix + "DocumentUrl";

            /// <summary>
            ///     The media version table name.
            /// </summary>
            public const string MediaVersion = TableNamePrefix + "MediaVersion";

            /// <summary>
            ///     The content schedule table name.
            /// </summary>
            public const string ContentSchedule = TableNamePrefix + "ContentSchedule";

            /// <summary>
            ///     The property type table name.
            /// </summary>
            public const string PropertyType = /*TableNamePrefix*/ "cms" + "PropertyType";

            /// <summary>
            ///     The property type group table name.
            /// </summary>
            public const string PropertyTypeGroup = /*TableNamePrefix*/ "cms" + "PropertyTypeGroup";

            /// <summary>
            ///     The property data table name.
            /// </summary>
            public const string PropertyData = TableNamePrefix + "PropertyData";

            /// <summary>
            ///     The relation type table name.
            /// </summary>
            public const string RelationType = TableNamePrefix + "RelationType";

            /// <summary>
            ///     The relation table name.
            /// </summary>
            public const string Relation = TableNamePrefix + "Relation";

            /// <summary>
            ///     The domain table name.
            /// </summary>
            public const string Domain = TableNamePrefix + "Domain";

            /// <summary>
            ///     The language table name.
            /// </summary>
            public const string Language = TableNamePrefix + "Language";

            /// <summary>
            ///     The dictionary entry table name.
            /// </summary>
            public const string DictionaryEntry = /*TableNamePrefix*/ "cms" + "Dictionary";

            /// <summary>
            ///     The dictionary value (language text) table name.
            /// </summary>
            public const string DictionaryValue = /*TableNamePrefix*/ "cms" + "LanguageText";

            /// <summary>
            ///     The user table name.
            /// </summary>
            public const string User = TableNamePrefix + "User";

            /// <summary>
            ///     The user group table name.
            /// </summary>
            public const string UserGroup = TableNamePrefix + "UserGroup";

            /// <summary>
            ///     The user start node table name.
            /// </summary>
            public const string UserStartNode = TableNamePrefix + "UserStartNode";

            /// <summary>
            ///     The user to user group mapping table name.
            /// </summary>
            public const string User2UserGroup = TableNamePrefix + "User2UserGroup";

            /// <summary>
            ///     The user to node notification table name.
            /// </summary>
            public const string User2NodeNotify = TableNamePrefix + "User2NodeNotify";

            /// <summary>
            ///     The user to client ID mapping table name.
            /// </summary>
            public const string User2ClientId = TableNamePrefix + "User2ClientId";

            /// <summary>
            ///     The user group to application mapping table name.
            /// </summary>
            public const string UserGroup2App = TableNamePrefix + "UserGroup2App";

            /// <summary>
            ///     The user data table name.
            /// </summary>
            public const string UserData = TableNamePrefix + "UserData";

            /// <summary>
            ///     The user group to node mapping table name.
            /// </summary>
            [Obsolete("Will be removed in Umbraco 18 as this table haven't existed since Umbraco 14.")]
            public const string UserGroup2Node = TableNamePrefix + "UserGroup2Node";

            /// <summary>
            ///     The user group to node permission mapping table name.
            /// </summary>
            [Obsolete("Will be removed in Umbraco 18 as this table haven't existed since Umbraco 14.")]
            public const string UserGroup2NodePermission = TableNamePrefix + "UserGroup2NodePermission";

            /// <summary>
            ///     The user group to permission mapping table name.
            /// </summary>
            public const string UserGroup2Permission = TableNamePrefix + "UserGroup2Permission";

            /// <summary>
            ///     The user group to granular permission mapping table name.
            /// </summary>
            public const string UserGroup2GranularPermission = TableNamePrefix + "UserGroup2GranularPermission";

            /// <summary>
            ///     The user group to language mapping table name.
            /// </summary>
            public const string UserGroup2Language = TableNamePrefix + "UserGroup2Language";

            /// <summary>
            ///     The external login table name.
            /// </summary>
            public const string ExternalLogin = TableNamePrefix + "ExternalLogin";

            /// <summary>
            ///     The two-factor login table name.
            /// </summary>
            public const string TwoFactorLogin = TableNamePrefix + "TwoFactorLogin";

            /// <summary>
            ///     The external login token table name.
            /// </summary>
            public const string ExternalLoginToken = TableNamePrefix + "ExternalLoginToken";

            /// <summary>
            ///     The member table name.
            /// </summary>
            public const string Member = /*TableNamePrefix*/ "cms" + "Member";

            /// <summary>
            ///     The member property type table name.
            /// </summary>
            public const string MemberPropertyType = /*TableNamePrefix*/ "cms" + "MemberType";

            /// <summary>
            ///     The member to member group mapping table name.
            /// </summary>
            public const string Member2MemberGroup = /*TableNamePrefix*/ "cms" + "Member2MemberGroup";

            /// <summary>
            ///     The public access table name.
            /// </summary>
            public const string Access = TableNamePrefix + "Access";

            /// <summary>
            ///     The access rule table name.
            /// </summary>
            public const string AccessRule = TableNamePrefix + "AccessRule";

            /// <summary>
            ///     The redirect URL table name.
            /// </summary>
            public const string RedirectUrl = TableNamePrefix + "RedirectUrl";

            /// <summary>
            ///     The cache instruction table name.
            /// </summary>
            public const string CacheInstruction = TableNamePrefix + "CacheInstruction";

            /// <summary>
            ///     The server registration table name.
            /// </summary>
            public const string Server = TableNamePrefix + "Server";

            /// <summary>
            ///     The tag table name.
            /// </summary>
            public const string Tag = /*TableNamePrefix*/ "cms" + "Tags";

            /// <summary>
            ///     The tag relationship table name.
            /// </summary>
            public const string TagRelationship = /*TableNamePrefix*/ "cms" + "TagRelationship";

            /// <summary>
            ///     The key-value table name.
            /// </summary>
            public const string KeyValue = TableNamePrefix + "KeyValue";

            /// <summary>
            ///     The audit entry table name.
            /// </summary>
            public const string AuditEntry = TableNamePrefix + "Audit";

            /// <summary>
            ///     The consent table name.
            /// </summary>
            public const string Consent = TableNamePrefix + "Consent";

            /// <summary>
            ///     The user login table name.
            /// </summary>
            public const string UserLogin = TableNamePrefix + "UserLogin";

            /// <summary>
            ///     The log viewer query table name.
            /// </summary>
            public const string LogViewerQuery = TableNamePrefix + "LogViewerQuery";

            /// <summary>
            ///     The created package schema table name.
            /// </summary>
            public const string CreatedPackageSchema = TableNamePrefix + "CreatedPackageSchema";

            /// <summary>
            ///     The webhook table name.
            /// </summary>
            public const string Webhook = TableNamePrefix + "Webhook";

            /// <summary>
            ///     The webhook to content type keys mapping table name.
            /// </summary>
            public const string Webhook2ContentTypeKeys = Webhook + "2ContentTypeKeys";

            /// <summary>
            ///     The webhook to events mapping table name.
            /// </summary>
            public const string Webhook2Events = Webhook + "2Events";

            /// <summary>
            ///     The webhook to headers mapping table name.
            /// </summary>
            public const string Webhook2Headers = Webhook + "2Headers";

            /// <summary>
            ///     The webhook log table name.
            /// </summary>
            public const string WebhookLog = Webhook + "Log";

            /// <summary>
            ///     The webhook request table name.
            /// </summary>
            public const string WebhookRequest = Webhook + "Request";

            /// <summary>
            ///     The repository cache version table name.
            /// </summary>
            public const string RepositoryCacheVersion = TableNamePrefix + "RepositoryCacheVersion";

            /// <summary>
            ///     The long-running operation table name.
            /// </summary>
            public const string LongRunningOperation = TableNamePrefix + "LongRunningOperation";

            /// <summary>
            ///     The last synced table name.
            /// </summary>
            public const string LastSynced = TableNamePrefix + "LastSynced";

            /// <summary>
            ///     The distributed job table name.
            /// </summary>
            public const string DistributedJob = TableNamePrefix + "DistributedJob";
        }
    }
}
