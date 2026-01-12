namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class Telemetry
    {
        public const string RootCount = "RootCount";
        public const string DomainCount = "DomainCount";
        public const string ExamineIndexCount = "ExamineIndexCount";
        public const string LanguageCount = "LanguageCount";
        public const string MediaCount = "MediaCount";
        public const string MemberCount = "MemberCount";
        public const string TemplateCount = "TemplateCount";
        public const string ContentCount = "ContentCount";
        public const string DocumentTypeCount = "DocumentTypeCount";
        public const string Properties = "Properties";
        public const string UserCount = "UserCount";
        public const string UserGroupCount = "UserGroupCount";
        public const string ServerOs = "ServerOs";
        public const string ServerFramework = "ServerFramework";
        public const string OsLanguage = "OsLanguage";
        public const string WebServer = "WebServer";
        public const string ModelsBuilderMode = "ModelBuilderMode";
        public const string AspEnvironment = "AspEnvironment";
        public const string IsDebug = "IsDebug";
        public const string DatabaseProvider = "DatabaseProvider";
        public const string CurrentServerRole = "CurrentServerRole";
        public const string RuntimeMode = "RuntimeMode";
        public const string BackofficeExternalLoginProviderCount = "BackofficeExternalLoginProviderCount";
        public const string DeliverApiEnabled = "DeliverApiEnabled";
        public const string DeliveryApiPublicAccess = "DeliveryApiPublicAccess";
        public const string WebhookPrefix = "WebhookCount_";
        public static readonly string WebhookTotal = $"{WebhookPrefix}Total";
        public static readonly string WebhookCustomHeaders = $"{WebhookPrefix}CustomHeaders";
        public static readonly string WebhookCustomEvent = $"{WebhookPrefix}CustomEvent";
        public const string RichTextEditorCount = "RichTextEditorCount";
        public const string RichTextBlockCount = "RichTextBlockCount";
        public const string TotalPropertyCount = "TotalPropertyCount";
        public const string HighestPropertyCount = "HighestPropertyCount";
        public const string TotalCompositions = "TotalCompositions";
    }
}
