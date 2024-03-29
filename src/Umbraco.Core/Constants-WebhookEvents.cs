namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class WebhookEvents
    {
        public static class HeaderNames
        {
            /// <summary>
            /// Gets the <c>Umb-Webhook-Event</c> HTTP header name.
            /// </summary>
            public const string EventName = "Umb-Webhook-Event";

            /// <summary>
            /// Gets the <c>Umb-Webhook-RetryCount</c> HTTP header name.
            /// </summary>
            public const string RetryCount = "Umb-Webhook-RetryCount";
        }

        public static class Aliases
        {
            /// <summary>
            /// Webhook event alias for content versions deleted.
            /// </summary>
            public const string ContentDeletedVersions = "Umbraco.ContentDeletedVersions";

            /// <summary>
            /// Webhook event alias for content blueprint saved.
            /// </summary>
            public const string ContentSavedBlueprint = "Umbraco.ContentSavedBlueprint";

            /// <summary>
            /// Webhook event alias for content blueprint deleted.
            /// </summary>
            public const string ContentDeletedBlueprint = "Umbraco.ContentDeletedBlueprint";

            /// <summary>
            /// Webhook event alias for content moved into the recycle bin.
            /// </summary>
            public const string ContentMovedToRecycleBin = "Umbraco.ContentMovedToRecycleBin";

            /// <summary>
            /// Webhook event alias for content sorted.
            /// </summary>
            public const string ContentSorted = "Umbraco.ContentSorted";

            /// <summary>
            /// Webhook event alias for content moved.
            /// </summary>
            public const string ContentMoved = "Umbraco.ContentMoved";

            /// <summary>
            /// Webhook event alias for content copied.
            /// </summary>
            public const string ContentCopied = "Umbraco.ContentCopied";

            /// <summary>
            /// Webhook event alias for content emptied recycle bin.
            /// </summary>
            public const string ContentEmptiedRecycleBin = "Umbraco.ContentEmptiedRecycleBin";

            /// <summary>
            /// Webhook event alias for content rolled back.
            /// </summary>
            public const string ContentRolledBack = "Umbraco.ContentRolledBack";

            /// <summary>
            /// Webhook event alias for content saved.
            /// </summary>
            public const string ContentSaved = "Umbraco.ContentSaved";

            /// <summary>
            /// Webhook event alias for content publish.
            /// </summary>
            public const string ContentPublish = "Umbraco.ContentPublish";

            /// <summary>
            /// Webhook event alias for content delete.
            /// </summary>
            public const string ContentDelete = "Umbraco.ContentDelete";

            /// <summary>
            /// Webhook event alias for content unpublish.
            /// </summary>
            public const string ContentUnpublish = "Umbraco.ContentUnpublish";

            /// <summary>
            /// Webhook event alias for media delete.
            /// </summary>
            public const string MediaDelete = "Umbraco.MediaDelete";

            /// <summary>
            /// Webhook event alias for media save.
            /// </summary>
            public const string MediaSave = "Umbraco.MediaSave";

            /// <summary>
            ///     Webhook event alias for document type changed.
            /// </summary>
            public const string DocumentTypeChanged = "documentTypeChanged";

            /// <summary>
            ///     Webhook event alias for document type deleted.
            /// </summary>
            public const string DocumentTypeDeleted = "documentTypeDeleted";

            /// <summary>
            ///     Webhook event alias for document type moved.
            /// </summary>
            public const string DocumentTypeMoved = "documentTypeMoved";

            /// <summary>
            ///     Webhook event alias for document type saved.
            /// </summary>
            public const string DocumentTypeSaved = "documentTypeSaved";

            /// <summary>
            ///     Webhook event alias for media type changed.
            /// </summary>
            public const string MediaTypeChanged = "mediaTypeChanged";

            /// <summary>
            ///     Webhook event alias for media type deleted.
            /// </summary>
            public const string MediaTypeDeleted = "mediaTypeDeleted";

            /// <summary>
            ///     Webhook event alias for media type moved.
            /// </summary>
            public const string MediaTypeMoved = "mediaTypeMoved";

            /// <summary>
            ///     Webhook event alias for media type saved.
            /// </summary>
            public const string MediaTypeSaved = "mediaTypeSaved";

            /// <summary>
            ///     Webhook event alias for member type changed.
            /// </summary>
            public const string MemberTypeChanged = "memberTypeChanged";

            /// <summary>
            ///     Webhook event alias for member type deleted.
            /// </summary>
            public const string MemberTypeDeleted = "memberTypeDeleted";

            /// <summary>
            ///     Webhook event alias for member type moved.
            /// </summary>
            public const string MemberTypeMoved = "memberTypeMoved";

            /// <summary>
            ///     Webhook event alias for member type saved.
            /// </summary>
            public const string MemberTypeSaved = "memberTypeSaved";

            /// <summary>
            ///     Webhook event alias for data type deleted.
            /// </summary>
            public const string DataTypeDeleted = "dataTypeDeleted";

            /// <summary>
            ///     Webhook event alias for data type moved.
            /// </summary>
            public const string DataTypeMoved = "dataTypeMoved";

            /// <summary>
            ///     Webhook event alias for data type saved.
            /// </summary>
            public const string DataTypeSaved = "dataTypeSaved";

            /// <summary>
            ///     Webhook event alias for dictionary item deleted.
            /// </summary>
            public const string DictionaryItemDeleted = "dictionaryItemDeleted";

            /// <summary>
            ///     Webhook event alias for dictionary item saved.
            /// </summary>
            public const string DictionaryItemSaved = "dictionaryItemSaved";

            /// <summary>
            ///     Webhook event alias for domain deleted.
            /// </summary>
            public const string DomainDeleted = "domainDeleted";

            /// <summary>
            ///     Webhook event alias for domain saved.
            /// </summary>
            public const string DomainSaved = "domainSaved";

            /// <summary>
            ///     Webhook event alias for partial view deleted.
            /// </summary>
            public const string PartialViewDeleted = "partialViewDeleted";

            /// <summary>
            ///     Webhook event alias for partial view saved.
            /// </summary>
            public const string PartialViewSaved = "partialViewSaved";

            /// <summary>
            ///     Webhook event alias for script deleted.
            /// </summary>
            public const string ScriptDeleted = "scriptDeleted";

            /// <summary>
            ///     Webhook event alias for script saved.
            /// </summary>
            public const string ScriptSaved = "scriptSaved";

            /// <summary>
            ///     Webhook event alias for stylesheet deleted.
            /// </summary>
            public const string StylesheetDeleted = "stylesheetDeleted";

            /// <summary>
            ///     Webhook event alias for stylesheet saved.
            /// </summary>
            public const string StylesheetSaved = "stylesheetSaved";

            /// <summary>
            ///     Webhook event alias for template deleted.
            /// </summary>
            public const string TemplateDeleted = "templateDeleted";

            /// <summary>
            ///     Webhook event alias for template saved.
            /// </summary>
            public const string TemplateSaved = "templateSaved";

            /// <summary>
            ///     Webhook event alias for health check completed.
            /// </summary>
            public const string HealthCheckCompleted = "healthCheckCompleted";

            /// <summary>
            ///     Webhook event alias for language deleted.
            /// </summary>
            public const string LanguageDeleted = "languageDeleted";

            /// <summary>
            ///     Webhook event alias for language saved.
            /// </summary>
            public const string LanguageSaved = "languageSaved";

            /// <summary>
            ///     Webhook event alias for media emptied recycle bin.
            /// </summary>
            public const string MediaEmptiedRecycleBin = "mediaEmptiedRecycleBin";

            /// <summary>
            ///     Webhook event alias for media moved to recycle bin.
            /// </summary>
            public const string MediaMovedToRecycleBin = "mediaMovedToRecycleBin";

            /// <summary>
            ///     Webhook event alias for media moved.
            /// </summary>
            public const string MediaMoved = "mediaMoved";

            /// <summary>
            ///     Webhook event alias for assigned member roles.
            /// </summary>
            public const string AssignedMemberRoles = "assignedMemberRoles";

            /// <summary>
            ///     Webhook event alias for exported member.
            /// </summary>
            public const string ExportedMember = "exportedMember";

            /// <summary>
            ///     Webhook event alias for member deleted.
            /// </summary>
            public const string MemberDeleted = "memberDeleted";

            /// <summary>
            ///     Webhook event alias for member group deleted.
            /// </summary>
            public const string MemberGroupDeleted = "memberGroupDeleted";

            /// <summary>
            ///     Webhook event alias for member group saved.
            /// </summary>
            public const string MemberGroupSaved = "memberGroupSaved";

            /// <summary>
            ///     Webhook event alias for member saved.
            /// </summary>
            public const string MemberSaved = "memberSaved";

            /// <summary>
            ///     Webhook event alias for removed member roles.
            /// </summary>
            public const string RemovedMemberRoles = "removedMemberRoles";

            /// <summary>
            ///     Webhook event alias for package imported.
            /// </summary>
            public const string PackageImported = "packageImported";

            /// <summary>
            ///     Webhook event alias for public access entry deleted.
            /// </summary>
            public const string PublicAccessEntryDeleted = "publicAccessEntryDeleted";

            /// <summary>
            ///     Webhook event alias for public access entry saved.
            /// </summary>
            public const string PublicAccessEntrySaved = "publicAccessEntrySaved";

            /// <summary>
            ///     Webhook event alias for relation deleted.
            /// </summary>
            public const string RelationDeleted = "relationDeleted";

            /// <summary>
            ///     Webhook event alias for relation saved.
            /// </summary>
            public const string RelationSaved = "relationSaved";

            /// <summary>
            ///     Webhook event alias for relation type deleted.
            /// </summary>
            public const string RelationTypeDeleted = "relationTypeDeleted";

            /// <summary>
            ///     Webhook event alias for relation type saved.
            /// </summary>
            public const string RelationTypeSaved = "relationTypeSaved";

            /// <summary>
            ///    Webhook event alias for assigned user group permissions;
            /// </summary>
            public const string AssignedUserGroupPermissions = "assignedUserGroupPermissions";

            /// <summary>
            ///     Webhook event alias for user deleted.
            /// </summary>
            public const string UserDeleted = "userDeleted";

            /// <summary>
            ///     Webhook event alias for user forgot password requested.
            /// </summary>
            public const string UserForgotPasswordRequested = "userForgotPasswordRequested";

            /// <summary>
            ///     Webhook event alias for user group deleted.
            /// </summary>
            public const string UserGroupDeleted = "UserGroupDeleted";

            /// <summary>
            ///     Webhook event alias for user group saved.
            /// </summary>
            public const string UserGroupSaved = "userGroupSaved";

            /// <summary>
            ///     Webhook event alias for user locked.
            /// </summary>
            public const string UserLocked = "userLocked";

            /// <summary>
            ///     Webhook event alias for user login failed.
            /// </summary>
            public const string UserLoginFailed = "userLoginFailed";

            /// <summary>
            ///     Webhook event alias for user login requires verification.
            /// </summary>
            public const string UserLoginRequiresVerification = "userLoginRequiresVerification";

            /// <summary>
            ///     Webhook event alias for user login success.
            /// </summary>
            public const string UserLoginSuccess = "userLoginSuccess";

            /// <summary>
            ///     Webhook event alias for user logout success.
            /// </summary>
            public const string UserLogoutSuccess = "userLogoutSuccess";

            /// <summary>
            ///     Webhook event alias for user password changed.
            /// </summary>
            public const string UserPasswordChanged = "userPasswordChanged";

            /// <summary>
            ///     Webhook event alias for user password reset.
            /// </summary>
            public const string UserPasswordReset = "userPasswordReset";

            /// <summary>
            ///     Webhook event alias for user saved.
            /// </summary>
            public const string UserSaved = "userSaved";

            /// <summary>
            ///     Webhook event alias for user two factor requested.
            /// </summary>
            public const string UserTwoFactorRequested = "userTwoFactorRequested";

            /// <summary>
            ///     Webhook event alias for user unlocked.
            /// </summary>
            public const string UserUnlocked = "userUnlocked";
        }

        public static class Types
        {
            /// <summary>
            /// Webhook event type for content.
            /// </summary>
            public const string Content = "Content";

            /// <summary>
            /// Webhook event type for content media.
            /// </summary>
            public const string Media = "Media";

            /// <summary>
            /// Webhook event type for content member.
            /// </summary>
            public const string Member = "Member";

            /// <summary>
            /// Webhook event type for others, this is the default category if you have not chosen one.
            /// </summary>
            public const string Other = "Other";
        }
    }
}
