using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Core.Webhooks.Events;
using static Umbraco.Cms.Core.DependencyInjection.WebhookEventCollectionBuilderExtensions;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="WebhookEventCollectionBuilderCms" />.
/// </summary>
public static class WebhookEventCollectionBuilderCmsExtensions
{
    /// <summary>
    /// Adds the default webhook events.
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
            .Add<ContentDeletedWebhookEvent>()
            .Add<ContentPublishedWebhookEvent>()
            .Add<ContentUnpublishedWebhookEvent>()
            .Add<MediaDeletedWebhookEvent>()
            .Add<MediaSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds all available content (including blueprint and version) webhook events.
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
    /// Adds content webhook events specified in the <paramref name="contentBuilder" /> action.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="contentBuilder">The content builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddContent(this WebhookEventCollectionBuilderCms builder, Action<WebhookEventCollectionBuilderCmsContent> contentBuilder)
    {
        contentBuilder(new WebhookEventCollectionBuilderCmsContent(builder.Builder));

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
    /// Adds content type webhook events specified in the <paramref name="contentTypeBuilder" /> action.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="contentTypeBuilder">The content type builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddContentType(this WebhookEventCollectionBuilderCms builder, Action<WebhookEventCollectionBuilderCmsContentType> contentTypeBuilder)
    {
        contentTypeBuilder(new WebhookEventCollectionBuilderCmsContentType(builder.Builder));

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
            .Add<DataTypeDeletedWebhookEvent>()
            .Add<DataTypeMovedWebhookEvent>()
            .Add<DataTypeSavedWebhookEvent>();

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
            .Add<DictionaryItemDeletedWebhookEvent>()
            .Add<DictionaryItemSavedWebhookEvent>();

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
            .Add<DomainDeletedWebhookEvent>()
            .Add<DomainSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds all available file (partial view, script, stylesheet and template) webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddFile(this WebhookEventCollectionBuilderCms builder)
        => builder.AddFile(builder =>
        {
            builder
                .AddPartialView()
                .AddScript()
                .AddStylesheet()
                .AddTemplate();
        });

    /// <summary>
    /// Adds file webhook events specified in the <paramref name="fileBuilder" /> action.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="fileBuilder">The file builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddFile(this WebhookEventCollectionBuilderCms builder, Action<WebhookEventCollectionBuilderCmsFile> fileBuilder)
    {
        fileBuilder(new WebhookEventCollectionBuilderCmsFile(builder.Builder));

        return builder;
    }

    /// <summary>
    /// Adds the health check webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddHealthCheck(this WebhookEventCollectionBuilderCms builder)
    {
        builder.Builder
            .Add<HealthCheckCompletedWebhookEvent>();

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
            .Add<LanguageDeletedWebhookEvent>()
            .Add<LanguageSavedWebhookEvent>();

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
            .Add<MediaDeletedWebhookEvent>()
            .Add<MediaSavedWebhookEvent>()
            .Add<MediaEmptiedRecycleBinWebhookEvent>()
            .Add<MediaMovedWebhookEvent>()
            .Add<MediaMovedToRecycleBinWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds all available member (including member role and member group) webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="onlyDefault">If set to <c>true</c> only adds the default webhook events instead of all available.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddMember(this WebhookEventCollectionBuilderCms builder, bool onlyDefault = false)
        => builder.AddMember(builder =>
        {
            builder.AddDefault();

            if (onlyDefault is false)
            {
                builder
                    .AddRoles()
                    .AddGroup();
            }
        });

    /// <summary>
    /// Adds member webhook events specified in the <paramref name="memberBuilder" /> action.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="memberBuilder">The member builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddMember(this WebhookEventCollectionBuilderCms builder, Action<WebhookEventCollectionBuilderCmsMember> memberBuilder)
    {
        memberBuilder(new WebhookEventCollectionBuilderCmsMember(builder.Builder));

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
            .Add<ImportedPackageWebhookEvent>();

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
            .Add<PublicAccessEntryDeletedWebhookEvent>()
            .Add<PublicAccessEntrySavedWebhookEvent>();

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
            .Add<RelationDeletedWebhookEvent>()
            .Add<RelationSavedWebhookEvent>();

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
            .Add<RelationTypeDeletedWebhookEvent>()
            .Add<RelationTypeSavedWebhookEvent>();

        return builder;
    }

    /// <summary>
    /// Adds all available user (including password, login and user group) webhook events.
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
                    .AddGroup();
            }
        });

    /// <summary>
    /// Adds user webhook events specified in the <paramref name="userBuilder" /> action.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="userBuilder">The user builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddUser(this WebhookEventCollectionBuilderCms builder, Action<WebhookEventCollectionBuilderCmsUser> userBuilder)
    {
        userBuilder(new WebhookEventCollectionBuilderCmsUser(builder.Builder));

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
    /// Fluent <see cref="WebhookEventCollectionBuilder" /> for adding CMS member specific webhook events.
    /// </summary>
    public sealed class WebhookEventCollectionBuilderCmsMember
    {
        internal WebhookEventCollectionBuilderCmsMember(WebhookEventCollectionBuilder builder)
            => Builder = builder;

        internal WebhookEventCollectionBuilder Builder { get; }
    }

    /// <summary>
    /// Fluent <see cref="WebhookEventCollectionBuilder" /> for adding CMS file specific webhook events.
    /// </summary>
    public sealed class WebhookEventCollectionBuilderCmsFile
    {
        internal WebhookEventCollectionBuilderCmsFile(WebhookEventCollectionBuilder builder)
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
