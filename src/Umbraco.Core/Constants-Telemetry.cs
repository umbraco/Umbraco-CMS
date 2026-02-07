namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains constants for telemetry data keys used in system analytics and reporting.
    /// </summary>
    public static class Telemetry
    {
        // TODO (V18): Convert these static fields to const to comply with SA1401 (fields should be private)

        /// <summary>
        ///     The telemetry key for the count of root content nodes.
        /// </summary>
        public static string RootCount = "RootCount";

        /// <summary>
        ///     The telemetry key for the count of domains configured.
        /// </summary>
        public static string DomainCount = "DomainCount";

        /// <summary>
        ///     The telemetry key for the count of Examine search indexes.
        /// </summary>
        public static string ExamineIndexCount = "ExamineIndexCount";

        /// <summary>
        ///     The telemetry key for the count of languages configured.
        /// </summary>
        public static string LanguageCount = "LanguageCount";

        /// <summary>
        ///     The telemetry key for the count of media items.
        /// </summary>
        public static string MediaCount = "MediaCount";

        /// <summary>
        ///     The telemetry key for the count of members.
        /// </summary>
        public static string MemberCount = "MemberCount";

        /// <summary>
        ///     The telemetry key for the count of templates.
        /// </summary>
        public static string TemplateCount = "TemplateCount";

        /// <summary>
        ///     The telemetry key for the count of content items.
        /// </summary>
        public static string ContentCount = "ContentCount";

        /// <summary>
        ///     The telemetry key for the count of document types.
        /// </summary>
        public static string DocumentTypeCount = "DocumentTypeCount";

        /// <summary>
        ///     The telemetry key for property information.
        /// </summary>
        public static string Properties = "Properties";

        /// <summary>
        ///     The telemetry key for the count of users.
        /// </summary>
        public static string UserCount = "UserCount";

        /// <summary>
        ///     The telemetry key for the count of user groups.
        /// </summary>
        public static string UserGroupCount = "UserGroupCount";

        /// <summary>
        ///     The telemetry key for the server operating system.
        /// </summary>
        public static string ServerOs = "ServerOs";

        /// <summary>
        ///     The telemetry key for the server framework version.
        /// </summary>
        public static string ServerFramework = "ServerFramework";

        /// <summary>
        ///     The telemetry key for the operating system language.
        /// </summary>
        public static string OsLanguage = "OsLanguage";

        /// <summary>
        ///     The telemetry key for the web server type.
        /// </summary>
        public static string WebServer = "WebServer";

        /// <summary>
        ///     The telemetry key for the Models Builder mode.
        /// </summary>
        public static string ModelsBuilderMode = "ModelBuilderMode";

        /// <summary>
        ///     The telemetry key for the ASP.NET Core environment name.
        /// </summary>
        public static string AspEnvironment = "AspEnvironment";

        /// <summary>
        ///     The telemetry key indicating whether debug mode is enabled.
        /// </summary>
        public static string IsDebug = "IsDebug";

        /// <summary>
        ///     The telemetry key for the database provider type.
        /// </summary>
        public static string DatabaseProvider = "DatabaseProvider";

        /// <summary>
        ///     The telemetry key for the current server role in a load-balanced environment.
        /// </summary>
        public static string CurrentServerRole = "CurrentServerRole";

        /// <summary>
        ///     The telemetry key for the runtime mode.
        /// </summary>
        public static string RuntimeMode = "RuntimeMode";

        /// <summary>
        ///     The telemetry key for the count of backoffice external login providers.
        /// </summary>
        public static string BackofficeExternalLoginProviderCount = "BackofficeExternalLoginProviderCount";

        /// <summary>
        ///     The telemetry key indicating whether the Delivery API is enabled.
        /// </summary>
        public static string DeliverApiEnabled = "DeliverApiEnabled";

        /// <summary>
        ///     The telemetry key for Delivery API public access configuration.
        /// </summary>
        public static string DeliveryApiPublicAccess = "DeliveryApiPublicAccess";

        /// <summary>
        ///     The prefix used for webhook-related telemetry keys.
        /// </summary>
        public static string WebhookPrefix = "WebhookCount_";

        /// <summary>
        ///     The telemetry key for the total count of webhooks.
        /// </summary>
        public static string WebhookTotal = $"{WebhookPrefix}Total";

        /// <summary>
        ///     The telemetry key for the count of webhooks with custom headers.
        /// </summary>
        public static string WebhookCustomHeaders = $"{WebhookPrefix}CustomHeaders";

        /// <summary>
        ///     The telemetry key for the count of webhooks with custom events.
        /// </summary>
        public static string WebhookCustomEvent = $"{WebhookPrefix}CustomEvent";

        /// <summary>
        ///     The telemetry key for the count of rich text editors.
        /// </summary>
        public static string RichTextEditorCount = "RichTextEditorCount";

        /// <summary>
        ///     The telemetry key for the count of rich text blocks.
        /// </summary>
        public static string RichTextBlockCount = "RichTextBlockCount";

        /// <summary>
        ///     The telemetry key for the total count of properties across all content types.
        /// </summary>
        public static string TotalPropertyCount = "TotalPropertyCount";

        /// <summary>
        ///     The telemetry key for the highest property count on a single content type.
        /// </summary>
        public static string HighestPropertyCount = "HighestPropertyCount";

        /// <summary>
        ///     The telemetry key for the total count of compositions used.
        /// </summary>
        public static string TotalCompositions = "TotalCompositions";
    }
}
