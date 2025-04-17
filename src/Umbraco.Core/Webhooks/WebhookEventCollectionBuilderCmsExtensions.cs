using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Core.Webhooks.Events;
using static Umbraco.Cms.Core.DependencyInjection.WebhookEventCollectionBuilderExtensions;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="WebhookEventCollectionBuilderCms" />.
/// </summary>
public static class WebhookEventCollectionBuilderCmsExtensions
{
    private static readonly Type[] _defaultTypes =
    [
        typeof(ContentDeletedWebhookEvent),
        typeof(ContentPublishedWebhookEvent),
        typeof(ContentUnpublishedWebhookEvent),
        typeof(MediaDeletedWebhookEvent),
        typeof(MediaSavedWebhookEvent),
    ];

    private static readonly Type[] _extendedDefaultTypes =
    [
        typeof(ContentDeletedWebhookEvent),
        typeof(ExtendedContentPublishedWebhookEvent),
        typeof(ContentUnpublishedWebhookEvent),
        typeof(MediaDeletedWebhookEvent),
        typeof(ExtendedMediaSavedWebhookEvent),
    ];

    private static readonly Type[] _legacyDefaultTypes =
    [
        typeof(LegacyContentDeletedWebhookEvent),
        typeof(LegacyContentPublishedWebhookEvent),
        typeof(LegacyContentUnpublishedWebhookEvent),
        typeof(LegacyMediaDeletedWebhookEvent),
        typeof(LegacyMediaSavedWebhookEvent),
    ];

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
    public static WebhookEventCollectionBuilderCms AddDefault(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Minimal:
                builder.Builder.Add(_defaultTypes);
                break;
            case WebhookPayloadType.Extended:
                builder.Builder.Add(_extendedDefaultTypes);
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder.Add(_legacyDefaultTypes);
                break;
        }

        return builder;
    }

    /// <summary>
    /// Removes the default webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms RemoveDefault(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Minimal:
                builder.Builder.Add(_defaultTypes);
                break;
            case WebhookPayloadType.Extended:
                builder.Builder.Add(_extendedDefaultTypes);
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder.Add(_legacyDefaultTypes);
                break;
        }

        foreach (Type type in _defaultTypes)
        {
            builder.Builder.Remove(type);
        }

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
    public static WebhookEventCollectionBuilderCms AddContent(this WebhookEventCollectionBuilderCms builder, bool onlyDefault = false, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
        => builder.AddContent(builder =>
        {
            builder.AddDefault(payloadType);

            if (onlyDefault is false)
            {
                builder
                    .AddBlueprint(payloadType)
                    .AddVersion(payloadType);
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
    public static WebhookEventCollectionBuilderCms AddContentType(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
        => builder.AddContentType(builder =>
        {
            builder
                .AddDocumentType(payloadType)
                .AddMediaType(payloadType)
                .AddMemberType(payloadType);
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
    public static WebhookEventCollectionBuilderCms AddDataType(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<DataTypeDeletedWebhookEvent>()
                    .Add<DataTypeMovedWebhookEvent>()
                    .Add<DataTypeSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyDataTypeDeletedWebhookEvent>()
                    .Add<LegacyDataTypeMovedWebhookEvent>()
                    .Add<LegacyDataTypeSavedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the dictionary webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddDictionary(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<DictionaryItemDeletedWebhookEvent>()
                    .Add<DictionaryItemSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyDictionaryItemDeletedWebhookEvent>()
                    .Add<LegacyDictionaryItemSavedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the domain webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddDomain(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<DomainDeletedWebhookEvent>()
                    .Add<DomainSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyDomainDeletedWebhookEvent>()
                    .Add<LegacyDomainSavedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds all available file (partial view, script, stylesheet and template) webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddFile(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
        => builder.AddFile(builder =>
        {
            builder
                .AddPartialView(payloadType)
                .AddScript(payloadType)
                .AddStylesheet(payloadType)
                .AddTemplate(payloadType);
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
    public static WebhookEventCollectionBuilderCms AddHealthCheck(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<HealthCheckCompletedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyHealthCheckCompletedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the language webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddLanguage(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<LanguageDeletedWebhookEvent>()
                    .Add<LanguageSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyLanguageDeletedWebhookEvent>()
                    .Add<LegacyLanguageSavedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the media webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddMedia(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
                builder.Builder
                    .Add<MediaDeletedWebhookEvent>()
                    .Add<ExtendedMediaSavedWebhookEvent>()
                    .Add<MediaEmptiedRecycleBinWebhookEvent>()
                    .Add<MediaMovedWebhookEvent>()
                    .Add<MediaMovedToRecycleBinWebhookEvent>();
                break;
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<MediaDeletedWebhookEvent>()
                    .Add<MediaSavedWebhookEvent>()
                    .Add<MediaEmptiedRecycleBinWebhookEvent>()
                    .Add<MediaMovedWebhookEvent>()
                    .Add<MediaMovedToRecycleBinWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyMediaDeletedWebhookEvent>()
                    .Add<LegacyMediaSavedWebhookEvent>()
                    .Add<LegacyMediaEmptiedRecycleBinWebhookEvent>()
                    .Add<LegacyMediaMovedWebhookEvent>()
                    .Add<LegacyMediaMovedToRecycleBinWebhookEvent>();

                break;
        }

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
    public static WebhookEventCollectionBuilderCms AddMember(this WebhookEventCollectionBuilderCms builder, bool onlyDefault = false, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
        => builder.AddMember(builder =>
        {
            builder.AddDefault(payloadType);

            if (onlyDefault is false)
            {
                builder
                    .AddRoles(payloadType)
                    .AddGroup(payloadType);
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
    public static WebhookEventCollectionBuilderCms AddPackage(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<ImportedPackageWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyImportedPackageWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the public access webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddPublicAccess(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<PublicAccessEntryDeletedWebhookEvent>()
                    .Add<PublicAccessEntrySavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyPublicAccessEntryDeletedWebhookEvent>()
                    .Add<LegacyPublicAccessEntrySavedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the relation webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddRelation(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<RelationDeletedWebhookEvent>()
                    .Add<RelationSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyRelationDeletedWebhookEvent>()
                    .Add<LegacyRelationSavedWebhookEvent>();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Adds the relation type webhook events.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The builder.
    /// </returns>
    public static WebhookEventCollectionBuilderCms AddRelationType(this WebhookEventCollectionBuilderCms builder, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
    {
        switch (payloadType)
        {
            case WebhookPayloadType.Extended:
            case WebhookPayloadType.Minimal:
                builder.Builder
                    .Add<RelationTypeDeletedWebhookEvent>()
                    .Add<RelationTypeSavedWebhookEvent>();
                break;
            case WebhookPayloadType.Legacy:
                builder.Builder
                    .Add<LegacyRelationTypeDeletedWebhookEvent>()
                    .Add<LegacyRelationTypeSavedWebhookEvent>();
                break;
        }

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
    public static WebhookEventCollectionBuilderCms AddUser(this WebhookEventCollectionBuilderCms builder, bool onlyDefault = false, WebhookPayloadType payloadType = WebhookPayloadType.Minimal)
        => builder.AddUser(builder =>
        {
            builder.AddDefault(payloadType);

            if (onlyDefault is false)
            {
                builder
                    .AddPassword(payloadType)
                    .AddLogin(payloadType)
                    .AddGroup(payloadType);
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
