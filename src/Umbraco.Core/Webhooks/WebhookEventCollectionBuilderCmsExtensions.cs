using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Core.Webhooks.Events.Content;
using Umbraco.Cms.Core.Webhooks.Events.DataType;
using Umbraco.Cms.Core.Webhooks.Events.Dictionary;
using Umbraco.Cms.Core.Webhooks.Events.Domain;
using Umbraco.Cms.Core.Webhooks.Events.Language;
using Umbraco.Cms.Core.Webhooks.Events.Media;
using Umbraco.Cms.Core.Webhooks.Events.Member;
using Umbraco.Cms.Core.Webhooks.Events.MemberGroup;
using Umbraco.Cms.Core.Webhooks.Events.Package;
using Umbraco.Cms.Core.Webhooks.Events.PublicAccess;
using Umbraco.Cms.Core.Webhooks.Events.Relation;
using Umbraco.Cms.Core.Webhooks.Events.RelationType;
using Umbraco.Cms.Core.Webhooks.Events.Script;
using Umbraco.Cms.Core.Webhooks.Events.Stylesheet;
using Umbraco.Cms.Core.Webhooks.Events.Template;
using static Umbraco.Cms.Core.DependencyInjection.WebhookEventCollectionBuilderExtensions;

namespace Umbraco.Cms.Core.DependencyInjection;

public static class WebhookEventCollectionBuilderCmsExtensions
{
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
    public static WebhookEventCollectionBuilderCms AddDefault(this WebhookEventCollectionBuilderCms builder)
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
    /// <param name="onlyDefault">If set to <c>true</c> only adds the default webhook events instead of all available.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddContent(this WebhookEventCollectionBuilderCms builder, bool onlyDefault = false)
        => builder.AddContent(builder =>
        {
            builder.AddDefault();

            if (onlyDefault is false)
            {
                builder
                    .AddBlueprint()
                    .AddVersion();
            }
        });

    /// <summary>
    /// Adds CMS content webhook events specified in the <see cref="WebhookEventCollectionBuilderCmsContent" /> action.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="cmsContentBuilder">The CMS content builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddContent(this WebhookEventCollectionBuilderCms builder, Action<WebhookEventCollectionBuilderCmsContent> cmsContentBuilder)
    {
        cmsContentBuilder(new WebhookEventCollectionBuilderCmsContent(builder.Builder));

        return builder;
    }

    /// <summary>
    /// Adds all available content type (document, media and member type) webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddContentType(this WebhookEventCollectionBuilderCms builder)
        => builder.AddContentType(builder =>
        {
            builder
                .AddDocumentType()
                .AddMediaType()
                .AddMemberType();
        });

    /// <summary>
    /// Adds CMS content type (document, media and member type) webhook events specified in the <see cref="WebhookEventCollectionBuilderCmsContentType" /> action.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="cmsContentTypeBuilder">The CMS content type builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddContentType(this WebhookEventCollectionBuilderCms builder, Action<WebhookEventCollectionBuilderCmsContentType> cmsContentTypeBuilder)
    {
        cmsContentTypeBuilder(new WebhookEventCollectionBuilderCmsContentType(builder.Builder));

        return builder;
    }

    /// <summary>
    /// Adds the data type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddDataType(this WebhookEventCollectionBuilderCms builder)
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
    public static WebhookEventCollectionBuilderCms AddDictionary(this WebhookEventCollectionBuilderCms builder)
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
    public static WebhookEventCollectionBuilderCms AddDomain(this WebhookEventCollectionBuilderCms builder)
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
    public static WebhookEventCollectionBuilderCms AddLanguage(this WebhookEventCollectionBuilderCms builder)
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
    public static WebhookEventCollectionBuilderCms AddMedia(this WebhookEventCollectionBuilderCms builder)
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
    /// Adds the member webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddMember(this WebhookEventCollectionBuilderCms builder)
    {
        builder.Builder
            .Append<AssignedMemberRolesWebhookEvent>()
            .Append<ExportedMemberWebhookEvent>()
            .Append<MemberDeletedWebhookEvent>()
            .Append<MemberSavedWebhookEvent>()
            .Append<RemovedMemberRolesWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the member group webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddMemberGroup(this WebhookEventCollectionBuilderCms builder)
    {
        builder.Builder
            .Append<MemberGroupDeletedWebhookEvent>()
            .Append<MemberGroupSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the package webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddPackage(this WebhookEventCollectionBuilderCms builder)
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
    public static WebhookEventCollectionBuilderCms AddPublicAccess(this WebhookEventCollectionBuilderCms builder)
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
    public static WebhookEventCollectionBuilderCms AddRelation(this WebhookEventCollectionBuilderCms builder)
    {
        builder.Builder
            .Append<RelationDeletedWebhookEvent>()
            .Append<RelationSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds the relation type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddRelationType(this WebhookEventCollectionBuilderCms builder)
    {
        builder.Builder
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
    public static WebhookEventCollectionBuilderCms AddScript(this WebhookEventCollectionBuilderCms builder)
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
    public static WebhookEventCollectionBuilderCms AddStylesheet(this WebhookEventCollectionBuilderCms builder)
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
    public static WebhookEventCollectionBuilderCms AddTemplate(this WebhookEventCollectionBuilderCms builder)
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
    public static WebhookEventCollectionBuilderCms AddUser(this WebhookEventCollectionBuilderCms builder, bool onlyDefault = false)
        => builder.AddUser(builder =>
        {
            builder.AddDefault();

            if (onlyDefault is false)
            {
                builder
                    .AddPassword()
                    .AddLogin()
                    .AddUserGroup();
            }
        });

    /// <summary>
    /// Adds CMS user webhook events specified in the <see cref="WebhookEventCollectionBuilderCmsUser" /> action.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="cmsUserBuilder">The CMS user builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddUser(this WebhookEventCollectionBuilderCms builder, Action<WebhookEventCollectionBuilderCmsUser> cmsUserBuilder)
    {
        cmsUserBuilder(new WebhookEventCollectionBuilderCmsUser(builder.Builder));

        return builder;
    }

    /// <summary>
    /// Fluent <see cref="WebhookEventCollectionBuilder" /> for adding CMS content specific webhook events.
    /// </summary>
    public sealed class WebhookEventCollectionBuilderCmsContent
    {
        internal WebhookEventCollectionBuilderCmsContent(WebhookEventCollectionBuilder builder)
            => Builder = builder;

        internal WebhookEventCollectionBuilder Builder { get; }
    }

    /// <summary>
    /// Fluent <see cref="WebhookEventCollectionBuilder" /> for adding CMS content type specific webhook events.
    /// </summary>
    public sealed class WebhookEventCollectionBuilderCmsContentType
    {
        internal WebhookEventCollectionBuilderCmsContentType(WebhookEventCollectionBuilder builder)
            => Builder = builder;

        internal WebhookEventCollectionBuilder Builder { get; }
    }

    /// <summary>
    /// Fluent <see cref="WebhookEventCollectionBuilder" /> for adding CMS user specific webhook events.
    /// </summary>
    public sealed class WebhookEventCollectionBuilderCmsUser
    {
        internal WebhookEventCollectionBuilderCmsUser(WebhookEventCollectionBuilder builder)
            => Builder = builder;

        internal WebhookEventCollectionBuilder Builder { get; }
    }
}
