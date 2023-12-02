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
    /// <summary>
    /// Adds all available CMS webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="onlyDefault">If set to <c>true</c> only adds the default webhook events instead of all available.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddCms(this WebhookEventCollectionBuilder builder, bool onlyDefault = false)
        => builder.AddCms(builder =>
        {
            if (onlyDefault)
            {
                builder.AddDefault();
            }
            else
            {
                builder
                    .AddContent()
                    .AddDataType()
                    .AddDictionary()
                    .AddDomain()
                    .AddLanguage()
                    .AddMedia()
                    .AddMediaType()
                    .AddMember()
                    .AddMemberType()
                    .AddPackage()
                    .AddPublicAccess()
                    .AddRelation()
                    .AddScript()
                    .AddStylesheet()
                    .AddTemplate()
                    .AddUser();
            }
        });

    /// <summary>
    /// Adds CMS webhook events specified in the <see cref="CmsWebhookEventCollectionBuilder" /> action.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="cmsBuilder">The CMS builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilder AddCms(this WebhookEventCollectionBuilder builder, Action<CmsWebhookEventCollectionBuilder> cmsBuilder)
    {
        cmsBuilder(new CmsWebhookEventCollectionBuilder(builder));

        return builder;
    }

    /// <summary>
    /// Adds the default CMS webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    /// <remarks>
    /// This is a special subset of webhook events that is added by default.
    /// </remarks>
    public static CmsWebhookEventCollectionBuilder AddDefault(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<ContentDeletedWebhookEvent>()
            .Append<ContentPublishedWebhookEvent>()
            .Append<ContentUnpublishedWebhookEvent>()
            .Append<MediaDeletedWebhookEvent>()
            .Append<MediaSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the content webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddContent(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddDataType(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddDictionary(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddDomain(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddLanguage(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddMedia(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddMediaType(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddMember(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddMemberType(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddPackage(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddPublicAccess(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddRelation(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddScript(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddStylesheet(this CmsWebhookEventCollectionBuilder builder)
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
    public static CmsWebhookEventCollectionBuilder AddTemplate(this CmsWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<PartialViewDeletedWebhookEvent>()
            .Append<PartialViewSavedWebhookEvent>()
            .Append<TemplateDeletedWebhookEvent>()
            .Append<TemplateSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds all available user webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="onlyDefault">If set to <c>true</c> only adds the default webhook events instead of all available.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddUser(this CmsWebhookEventCollectionBuilder builder, bool onlyDefault = false)
        => builder.AddUser(builder =>
        {
            builder.AddDefault();

            if (onlyDefault is false)
            {
                builder
                    .AddUserGroup()
                    .AddLogin()
                    .AddPassword();
            }
        });

    /// <summary>
    /// Adds CMS user webhook events specified in the <see cref="CmsUserWebhookEventCollectionBuilder" /> action.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="userCmsBuilder">The CMS user builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsWebhookEventCollectionBuilder AddUser(this CmsWebhookEventCollectionBuilder builder, Action<CmsUserWebhookEventCollectionBuilder> userCmsBuilder)
    {
        userCmsBuilder(new CmsUserWebhookEventCollectionBuilder(builder.Builder));

        return builder;
    }

    /// <summary>
    /// Adds the user events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsUserWebhookEventCollectionBuilder AddDefault(this CmsUserWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<UserDeletedWebhookEvent>()
            .Append<UserSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the user group events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsUserWebhookEventCollectionBuilder AddUserGroup(this CmsUserWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<AssignedUserGroupPermissionsWebhookEvent>()
            .Append<UserGroupDeletedWebhookEvent>()
            .Append<UserGroupSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the login events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsUserWebhookEventCollectionBuilder AddLogin(this CmsUserWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<UserLockedWebhookEvent>()
            .Append<UserLoginFailedWebhookEvent>()
            .Append<UserLoginRequiresVerificationWebhookEvent>()
            .Append<UserLoginSuccessWebhookEvent>()
            .Append<UserLogoutSuccessWebhookEvent>()
            .Append<UserTwoFactorRequestedWebhookEvent>()
            .Append<UserUnlockedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the password events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static CmsUserWebhookEventCollectionBuilder AddPassword(this CmsUserWebhookEventCollectionBuilder builder)
    {
        builder.Builder
            .Append<UserForgotPasswordRequestedWebhookEvent>()
            .Append<UserForgottenPasswordRequestedWebhookEvent>()
            .Append<UserPasswordChangedWebhookEvent>()
            .Append<UserPasswordResetWebhookEvent>();

        return builder;
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

    /// <summary>
    /// Fluent <see cref="WebhookEventCollectionBuilder" /> for adding CMS user specific webhook events.
    /// </summary>
    public sealed class CmsUserWebhookEventCollectionBuilder
    {
        internal CmsUserWebhookEventCollectionBuilder(WebhookEventCollectionBuilder builder)
            => Builder = builder;

        internal WebhookEventCollectionBuilder Builder { get; }
    }
}
