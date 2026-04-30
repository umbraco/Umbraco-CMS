namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains constants for telemetry data keys used in system analytics and reporting.
    /// </summary>
    public static class Telemetry
    {
        /// <summary>
        ///     The telemetry key for the count of root content nodes.
        /// </summary>
        public const string RootCount = "RootCount";

        /// <summary>
        ///     The telemetry key for the count of domains configured.
        /// </summary>
        public const string DomainCount = "DomainCount";

        /// <summary>
        ///     The telemetry key for the count of Examine search indexes.
        /// </summary>
        public const string ExamineIndexCount = "ExamineIndexCount";

        /// <summary>
        ///     The telemetry key for the count of languages configured.
        /// </summary>
        public const string LanguageCount = "LanguageCount";

        /// <summary>
        ///     The telemetry key for the count of media items.
        /// </summary>
        public const string MediaCount = "MediaCount";

        /// <summary>
        ///     The telemetry key for the count of members.
        /// </summary>
        public const string MemberCount = "MemberCount";

        /// <summary>
        ///     The telemetry key for the count of templates.
        /// </summary>
        public const string TemplateCount = "TemplateCount";

        /// <summary>
        ///     The telemetry key for the count of content items.
        /// </summary>
        public const string ContentCount = "ContentCount";

        /// <summary>
        ///     The telemetry key for the count of document types.
        /// </summary>
        public const string DocumentTypeCount = "DocumentTypeCount";

        /// <summary>
        ///     The telemetry key for property information.
        /// </summary>
        public const string Properties = "Properties";

        /// <summary>
        ///     The telemetry key for the count of users.
        /// </summary>
        public const string UserCount = "UserCount";

        /// <summary>
        ///     The telemetry key for the count of user groups.
        /// </summary>
        public const string UserGroupCount = "UserGroupCount";

        /// <summary>
        ///     The telemetry key for the server operating system.
        /// </summary>
        public const string ServerOs = "ServerOs";

        /// <summary>
        ///     The telemetry key for the server framework version.
        /// </summary>
        public const string ServerFramework = "ServerFramework";

        /// <summary>
        ///     The telemetry key for the operating system language.
        /// </summary>
        public const string OsLanguage = "OsLanguage";

        /// <summary>
        ///     The telemetry key for the web server type.
        /// </summary>
        public const string WebServer = "WebServer";

        /// <summary>
        ///     The telemetry key for the Models Builder mode.
        /// </summary>
        public const string ModelsBuilderMode = "ModelBuilderMode";

        /// <summary>
        ///     The telemetry key for the ASP.NET Core environment name.
        /// </summary>
        public const string AspEnvironment = "AspEnvironment";

        /// <summary>
        ///     The telemetry key indicating whether debug mode is enabled.
        /// </summary>
        public const string IsDebug = "IsDebug";

        /// <summary>
        ///     The telemetry key for the database provider type.
        /// </summary>
        public const string DatabaseProvider = "DatabaseProvider";

        /// <summary>
        ///     The telemetry key for the current server role in a load-balanced environment.
        /// </summary>
        public const string CurrentServerRole = "CurrentServerRole";

        /// <summary>
        ///     The telemetry key for the runtime mode.
        /// </summary>
        public const string RuntimeMode = "RuntimeMode";

        /// <summary>
        ///     The telemetry key for the count of backoffice external login providers.
        /// </summary>
        public const string BackofficeExternalLoginProviderCount = "BackofficeExternalLoginProviderCount";

        /// <summary>
        ///     The telemetry key indicating whether the Delivery API is enabled.
        /// </summary>
        public const string DeliverApiEnabled = "DeliverApiEnabled";

        /// <summary>
        ///     The telemetry key for Delivery API public access configuration.
        /// </summary>
        public const string DeliveryApiPublicAccess = "DeliveryApiPublicAccess";

        /// <summary>
        ///     The prefix used for webhook-related telemetry keys.
        /// </summary>
        public const string WebhookPrefix = "WebhookCount_";

        /// <summary>
        ///     The telemetry key for the total count of webhooks.
        /// </summary>
        public const string WebhookTotal = WebhookPrefix + "Total";

        /// <summary>
        ///     The telemetry key for the count of webhooks with custom headers.
        /// </summary>
        public const string WebhookCustomHeaders = WebhookPrefix + "CustomHeaders";

        /// <summary>
        ///     The telemetry key for the count of webhooks with custom events.
        /// </summary>
        public const string WebhookCustomEvent = WebhookPrefix + "CustomEvent";

        /// <summary>
        ///     The telemetry key for the count of rich text editors.
        /// </summary>
        public const string RichTextEditorCount = "RichTextEditorCount";

        /// <summary>
        ///     The telemetry key for the count of rich text blocks.
        /// </summary>
        public const string RichTextBlockCount = "RichTextBlockCount";

        /// <summary>
        ///     The telemetry key for the total count of properties across all content types.
        /// </summary>
        public const string TotalPropertyCount = "TotalPropertyCount";

        /// <summary>
        ///     The telemetry key for the highest property count on a single content type.
        /// </summary>
        public const string HighestPropertyCount = "HighestPropertyCount";

        /// <summary>
        ///     The telemetry key for the total count of compositions used.
        /// </summary>
        public const string TotalCompositions = "TotalCompositions";
    }
}
