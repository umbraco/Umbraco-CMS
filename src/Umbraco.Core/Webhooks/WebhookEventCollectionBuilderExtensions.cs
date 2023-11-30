using Umbraco.Cms.Core.Webhooks;
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

namespace Umbraco.Cms.Core.DependencyInjection;

public static class WebhookEventCollectionBuilderExtensions
{
    internal static WebhookEventCollectionBuilder AddDefaultWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<ContentDeletedWebhookEvent>()
            .Append<ContentPublishedWebhookEvent>()
            .Append<ContentUnpublishedWebhookEvent>()
            .Append<MediaDeletedWebhookEvent>()
            .Append<MediaSavedWebhookEvent>();

    /// <summary>
    /// Adds all available CMS webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddCmsWebhooks(this WebhookEventCollectionBuilder builder)
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

    /// <summary>
    /// Adds the content webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
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

    /// <summary>
    /// Adds the data type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddDataTypeWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<DataTypeDeletedWebhookEvent>()
            .Append<DataTypeMovedWebhookEvent>()
            .Append<DataTypeSavedWebhookEvent>();

    /// <summary>
    /// Adds the dictionary webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddDictionaryWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<DictionaryItemDeletedWebhookEvent>()
            .Append<DictionaryItemSavedWebhookEvent>();

    /// <summary>
    /// Adds the domain webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddDomainWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<DomainDeletedWebhookEvent>()
            .Append<DomainSavedWebhookEvent>();

    /// <summary>
    /// Adds the language webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddLanguageWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
        .Append<LanguageDeletedWebhookEvent>()
        .Append<LanguageSavedWebhookEvent>();

    /// <summary>
    /// Adds the media webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddMediaWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<MediaDeletedWebhookEvent>()
            .Append<MediaSavedWebhookEvent>()
            .Append<MediaEmptiedRecycleBinWebhookEvent>()
            .Append<MediaMovedWebhookEvent>()
            .Append<MediaMovedToRecycleBinWebhookEvent>()
            .Append<MediaTypeChangedWebhookEvent>()
            .Append<MediaTypeDeletedWebhookEvent>()
            .Append<MediaTypeMovedWebhookEvent>()
            .Append<MediaTypeSavedWebhookEvent>();

    /// <summary>
    /// Adds the member webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddMemberWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<AssignedMemberRolesWebhookEvent>()
            .Append<ExportedMemberWebhookEvent>()
            .Append<MemberDeletedWebhookEvent>()
            .Append<MemberGroupDeletedWebhookEvent>()
            .Append<MemberGroupSavedWebhookEvent>()
            .Append<MemberSavedWebhookEvent>()
            .Append<RemovedMemberRolesWebhookEvent>();

    /// <summary>
    /// Adds the member type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddMemberTypeWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<MemberTypeChangedWebhookEvent>()
            .Append<MemberTypeDeletedWebhookEvent>()
            .Append<MemberTypeMovedWebhookEvent>()
            .Append<MemberTypeSavedWebhookEvent>();

    /// <summary>
    /// Adds the package webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddPackageWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<ImportedPackageWebhookEvent>();

    /// <summary>
    /// Adds the public access webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddPublicAccessWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<PublicAccessEntryDeletedWebhookEvent>()
            .Append<PublicAccessEntrySavedWebhookEvent>();

    /// <summary>
    /// Adds the relation webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddRelationWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<RelationDeletedWebhookEvent>()
            .Append<RelationSavedWebhookEvent>()
            .Append<RelationTypeDeletedWebhookEvent>()
            .Append<RelationTypeSavedWebhookEvent>();

    /// <summary>
    /// Adds the script webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddScriptWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<ScriptDeletedWebhookEvent>()
            .Append<ScriptSavedWebhookEvent>();

    /// <summary>
    /// Adds the stylesheet webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddStylesheetWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<StylesheetDeletedWebhookEvent>()
            .Append<StylesheetSavedWebhookEvent>();

    /// <summary>
    /// Adds the template webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddTemplateWebhooks(this WebhookEventCollectionBuilder builder)
        => builder
            .Append<PartialViewDeletedWebhookEvent>()
            .Append<PartialViewSavedWebhookEvent>()
            .Append<TemplateDeletedWebhookEvent>()
            .Append<TemplateSavedWebhookEvent>();

    /// <summary>
    /// Adds the user webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
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
