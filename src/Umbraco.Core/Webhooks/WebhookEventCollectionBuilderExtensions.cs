using Umbraco.Cms.Core.Webhooks.Events.Content;
using Umbraco.Cms.Core.Webhooks.Events.DataType;
using Umbraco.Cms.Core.Webhooks.Events.Dictionary;
using Umbraco.Cms.Core.Webhooks.Events.Domain;
using Umbraco.Cms.Core.Webhooks.Events.Language;
using Umbraco.Cms.Core.Webhooks.Events.Media;
using Umbraco.Cms.Core.Webhooks.Events.MediaType;
using Umbraco.Cms.Core.Webhooks.Events.Member;
using Umbraco.Cms.Core.Webhooks.Events.MemberType;
using Umbraco.Cms.Core.Webhooks.Events.Package;
using Umbraco.Cms.Core.Webhooks.Events.PublicAccess;
using Umbraco.Cms.Core.Webhooks.Events.Relation;
using Umbraco.Cms.Core.Webhooks.Events.RelationType;
using Umbraco.Cms.Core.Webhooks.Events.Script;
using Umbraco.Cms.Core.Webhooks.Events.Stylesheet;
using Umbraco.Cms.Core.Webhooks.Events.Template;
using Umbraco.Cms.Core.Webhooks.Events.User;

namespace Umbraco.Cms.Core.Webhooks;

public static class WebhookEventCollectionBuilderExtensions
{
    public static WebhookEventCollectionBuilder AddAllAvailableWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .AddContentWebhooks()
            .AddDataTypeWebhooks()
            .AddDictionaryWebhooks()
            .AddDomainWebhooks()
            .AddLanguageWebhooks()
            .AddMediaWebhooks()
            .AddMemberWebhooks()
            .AddMemberTypeWebhooks()
            .AddPackageWebhooks()
            .AddPublicAccessWebhooks()
            .AddRelationWebhooks()
            .AddScriptWebhooks()
            .AddStylesheetWebhooks()
            .AddTemplateWebhooks()
            .AddUserWebhooks();

    public static WebhookEventCollectionBuilder AddContentWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<ContentCopiedWebhookEvent>()
            .Append<ContentDeletedBlueprintWebhookEvent>()
            .Append<ContentDeletedVersionsWebhookEvent>()
            .Append<ContentDeletedWebhookEvent>()
            .Append<ContentEmptiedRecycleBinWebhookEvent>()
            .Append<ContentMovedToRecycleBinWebhookEvent>()
            .Append<ContentMovedWebhookEvent>()
            .Append<ContentPublishedWebhookEvent>()
            .Append<ContentRolledBackWebhookEvent>()
            .Append<ContentSavedBlueprintWebhookEvent>()
            .Append<ContentSavedWebhookEvent>()
            .Append<ContentSortedWebhookEvent>()
            .Append<ContentUnpublishedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddCoreWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<ContentDeletedWebhookEvent>()
            .Append<ContentPublishedWebhookEvent>()
            .Append<ContentUnpublishedWebhookEvent>()
            .Append<MediaDeletedWebhookEvent>()
            .Append<MediaSavedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddDataTypeWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<DataTypeDeletedWebhookEvent>()
            .Append<DataTypeMovedWebhookEvent>()
            .Append<DataTypeSavedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddDictionaryWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<DictionaryItemDeletedWebhookEvent>()
            .Append<DictionaryItemSavedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddDomainWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<DomainDeletedWebhookEvent>()
            .Append<DomainSavedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddLanguageWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
        .Append<LanguageDeletedWebhookEvent>()
        .Append<LanguageSavedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddMediaWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            // Even though these two are in the AddCoreWebhooks()
            // The job of the CollectionBuilder should be removing duplicates
            // Would allow someone to use .AddCoreWebhooks().AddMediaWebhooks()
            // Or if they explicitly they could skip over CoreWebHooks and just add this perhaps
            .Append<MediaDeletedWebhookEvent>()
            .Append<MediaSavedWebhookEvent>()
            .Append<MediaEmptiedRecycleBinWebhookEvent>()
            .Append<MediaMovedWebhookEvent>()
            .Append<MediaMovedToRecycleBinWebhookEvent>()
            .Append<MediaTypeChangedWebhookEvent>()
            .Append<MediaTypeDeletedWebhookEvent>()
            .Append<MediaTypeMovedWebhookEvent>()
            .Append<MediaTypeSavedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddMemberWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<AssignedMemberRolesWebhookEvent>()
            .Append<ExportedMemberWebhookEvent>()
            .Append<MemberDeletedWebhookEvent>()
            .Append<MemberGroupDeletedWebhookEvent>()
            .Append<MemberGroupSavedWebhookEvent>()
            .Append<MemberSavedWebhookEvent>()
            .Append<RemovedMemberRolesWebhookEvent>();

    public static WebhookEventCollectionBuilder AddMemberTypeWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<MemberTypeChangedWebhookEvent>()
            .Append<MemberTypeDeletedWebhookEvent>()
            .Append<MemberTypeMovedWebhookEvent>()
            .Append<MemberTypeSavedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddPackageWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<ImportedPackageWebhookEvent>();

    public static WebhookEventCollectionBuilder AddPublicAccessWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<PublicAccessEntryDeletedWebhookEvent>()
            .Append<PublicAccessEntrySavedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddRelationWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<RelationDeletedWebhookEvent>()
            .Append<RelationSavedWebhookEvent>()
            .Append<RelationTypeDeletedWebhookEvent>()
            .Append<RelationTypeSavedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddScriptWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<ScriptDeletedWebhookEvent>()
            .Append<ScriptSavedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddStylesheetWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<StylesheetDeletedWebhookEvent>()
            .Append<StylesheetSavedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddTemplateWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<PartialViewDeletedWebhookEvent>()
            .Append<PartialViewSavedWebhookEvent>()
            .Append<TemplateDeletedWebhookEvent>()
            .Append<TemplateSavedWebhookEvent>();

    public static WebhookEventCollectionBuilder AddUserWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<AssignedUserGroupPermissionsWebhookEvent>()
            .Append<UserDeletedWebhookEvent>()
            .Append<UserForgotPasswordRequestedWebhookEvent>()
            .Append<UserForgottenPasswordRequestedWebhookEvent>()
            .Append<UserGroupDeletedWebhookEvent>()
            .Append<UserGroupSavedWebhookEvent>()
            .Append<UserLockedWebhookEvent>()
            .Append<UserLoginFailedWebhookEvent>()
            .Append<UserLoginRequiresVerificationWebhookEvent>()
            .Append<UserLoginSuccessWebhookEvent>()
            .Append<UserLogoutSuccessWebhookEvent>()
            .Append<UserPasswordChangedWebhookEvent>()
            .Append<UserPasswordResetWebhookEvent>()
            .Append<UserSavedWebhookEvent>()
            .Append<UserTwoFactorRequestedWebhookEvent>()
            .Append<UserUnlockedWebhookEvent>();
    }
