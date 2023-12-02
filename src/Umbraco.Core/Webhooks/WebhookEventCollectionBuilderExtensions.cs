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
        => builder.AddCmsWebhooks(builder => builder
            .AddContentWebhooks()
            .AddDataTypeWebhooks()
            .AddDictionaryWebhooks()
            .AddDomainWebhooks()
            .AddLanguageWebhooks()
            .AddMediaWebhooks()
            .AddMediaTypeWebhooks()
            .AddMemberWebhooks()
            .AddMemberTypeWebhooks()
            .AddPackageWebhooks()
            .AddPublicAccessWebhooks()
            .AddRelationWebhooks()
            .AddScriptWebhooks()
            .AddStylesheetWebhooks()
            .AddTemplateWebhooks()
            .AddUserWebhooks());

    /// <summary>
    /// Adds CMS webhook events specified in the <see cref="CmsWebhookEventCollectionBuilder" /> action.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="cmsBuilder">The CMS builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddCmsWebhooks(this WebhookEventCollectionBuilder builder, Action<CmsWebhookEventCollectionBuilder> cmsBuilder)
    {
        cmsBuilder(new CmsWebhookEventCollectionBuilder(builder));

        return builder;
    }

    /// <summary>
    /// Adds the content webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddContentWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
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

        return builder;
    }

    /// <summary>
    /// Adds the data type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddDataTypeWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<DataTypeDeletedWebhookEvent>()
            .Append<DataTypeMovedWebhookEvent>()
            .Append<DataTypeSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the dictionary webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddDictionaryWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<DictionaryItemDeletedWebhookEvent>()
            .Append<DictionaryItemSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the domain webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddDomainWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<DomainDeletedWebhookEvent>()
            .Append<DomainSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the language webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddLanguageWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<LanguageDeletedWebhookEvent>()
            .Append<LanguageSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the media webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddMediaWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<MediaDeletedWebhookEvent>()
            .Append<MediaSavedWebhookEvent>()
            .Append<MediaEmptiedRecycleBinWebhookEvent>()
            .Append<MediaMovedWebhookEvent>()
            .Append<MediaMovedToRecycleBinWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the media type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddMediaTypeWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<MediaTypeChangedWebhookEvent>()
            .Append<MediaTypeDeletedWebhookEvent>()
            .Append<MediaTypeMovedWebhookEvent>()
            .Append<MediaTypeSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the member webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddMemberWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<AssignedMemberRolesWebhookEvent>()
            .Append<ExportedMemberWebhookEvent>()
            .Append<MemberDeletedWebhookEvent>()
            .Append<MemberGroupDeletedWebhookEvent>()
            .Append<MemberGroupSavedWebhookEvent>()
            .Append<MemberSavedWebhookEvent>()
            .Append<RemovedMemberRolesWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the member type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddMemberTypeWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<MemberTypeChangedWebhookEvent>()
            .Append<MemberTypeDeletedWebhookEvent>()
            .Append<MemberTypeMovedWebhookEvent>()
            .Append<MemberTypeSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the package webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddPackageWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<ImportedPackageWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the public access webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddPublicAccessWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<PublicAccessEntryDeletedWebhookEvent>()
            .Append<PublicAccessEntrySavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the relation webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddRelationWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<RelationDeletedWebhookEvent>()
            .Append<RelationSavedWebhookEvent>()
            .Append<RelationTypeDeletedWebhookEvent>()
            .Append<RelationTypeSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the script webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddScriptWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<ScriptDeletedWebhookEvent>()
            .Append<ScriptSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the stylesheet webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddStylesheetWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<StylesheetDeletedWebhookEvent>()
            .Append<StylesheetSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the template webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddTemplateWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<PartialViewDeletedWebhookEvent>()
            .Append<PartialViewSavedWebhookEvent>()
            .Append<TemplateDeletedWebhookEvent>()
            .Append<TemplateSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the user webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddUserWebhooks(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
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

        return builder;
    }
}

/// <summary>
/// Fluent <see cref="WebhookEventCollectionBuilder" /> for adding CMS specific webhook events.
/// </summary>
public sealed class CmsWebhookEventCollectionBuilder
{
    internal CmsWebhookEventCollectionBuilder(WebhookEventCollectionBuilder builder)
        => Builder = builder;

    internal WebhookEventCollectionBuilder Builder { get; }
}
