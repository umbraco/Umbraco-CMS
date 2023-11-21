using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Webhooks.Events;
using Umbraco.Cms.Core.Webhooks.Events.Content;
using Umbraco.Cms.Core.Webhooks.Events.DataType;
using Umbraco.Cms.Core.Webhooks.Events.Dictionary;
using Umbraco.Cms.Core.Webhooks.Events.Domain;
using Umbraco.Cms.Core.Webhooks.Events.Language;
using Umbraco.Cms.Core.Webhooks.Events.Media;
using Umbraco.Cms.Core.Webhooks.Events.Member;
using Umbraco.Cms.Core.Webhooks.Events.MemberType;
using Umbraco.Cms.Core.Webhooks.Events.Package;
using Umbraco.Cms.Core.Webhooks.Events.PublicAccess;
using Umbraco.Cms.Core.Webhooks.Events.Relation;
using Umbraco.Cms.Core.Webhooks.Events.Script;
using Umbraco.Cms.Core.Webhooks.Events.Stylesheet;
using Umbraco.Cms.Core.Webhooks.Events.Template;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Webhooks;

public class WebhookEventCollectionBuilder : OrderedCollectionBuilderBase<WebhookEventCollectionBuilder,
    WebhookEventCollection, IWebhookEvent>
{
    protected override WebhookEventCollectionBuilder This => this;

    public override void RegisterWith(IServiceCollection services)
    {
        // register the collection
        services.Add(new ServiceDescriptor(typeof(WebhookEventCollection), CreateCollection,
            ServiceLifetime.Singleton));

        // register the types
        RegisterTypes(services);
        base.RegisterWith(services);
    }

    private void RegisterTypes(IServiceCollection services)
    {
        Type[] types = GetRegisteringTypes(GetTypes()).ToArray();

        // ensure they are safe
        foreach (Type type in types)
        {
            EnsureType(type, "register");
        }

        foreach (Type type in types)
        {
            Type? notificationType = GetNotificationType(type);

            if (notificationType is null)
            {
                continue;
            }

            var descriptor = new ServiceDescriptor(
                typeof(INotificationAsyncHandler<>).MakeGenericType(notificationType),
                type,
                ServiceLifetime.Transient);

            if (!services.Contains(descriptor))
            {
                services.Add(descriptor);
            }
        }
    }

    private Type? GetNotificationType(Type handlerType)
    {
        if (handlerType.IsOfGenericType(typeof(INotificationAsyncHandler<>)))
        {
            Type[] genericArguments = handlerType.BaseType!.GetGenericArguments();

            Type? notificationType =
                genericArguments.FirstOrDefault(arg => typeof(INotification).IsAssignableFrom(arg));

            if (notificationType is not null)
            {
                return notificationType;
            }
        }

        return null;
    }

    public WebhookEventCollectionBuilder AddAllTheWebhooks() =>
        this.AddContentWebhooks()
            .AddDataTypeWebhooks()
            .AddDictionaryWebhooks()
            .AddDictionaryWebhooks()
            .AddLanguageWebhooks()
            .AddMediaWebhooks()
            .AddMemberWebhooks()
            .AddMemberTypeWebhooks()
            .AddPackageWebhooks()
            .AddPublicAccessWebhooks()
            .AddRelationWebhooks()
            .AddScriptWebhooks()
            .AddStylesheetWebhooks()
            .AddTemplateWebhooks();

    public WebhookEventCollectionBuilder AddContentWebhooks()
    {
        Append<ContentCopiedWebhookEvent>();
        Append<ContentDeletedBlueprintWebhookEvent>();
        Append<ContentDeletedVersionsWebhookEvent>();
        Append<ContentDeletedWebhookEvent>();
        Append<ContentEmptiedRecycleBinWebhookEvent>();
        Append<ContentMovedToRecycleBinWebhookEvent>();
        Append<ContentMovedWebhookEvent>();
        Append<ContentPublishedWebhookEvent>();
        Append<ContentRolledBackWebhookEvent>();
        Append<ContentSavedBlueprintWebhookEvent>();
        Append<ContentSavedWebhookEvent>();
        Append<ContentSortedWebhookEvent>();
        Append<ContentUnpublishedWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddCoreWebhooks()
    {
        Append<ContentDeletedWebhookEvent>();
        Append<ContentPublishedWebhookEvent>();
        Append<ContentUnpublishedWebhookEvent>();
        Append<MediaDeletedWebhookEvent>();
        Append<MediaSavedWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddDataTypeWebhooks()
    {
        Append<DataTypeDeletedWebhookEvent>();
        Append<DataTypeMovedWebhookEvent>();
        Append<DataTypeSavedWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddDictionaryWebhooks()
    {
        Append<DictionaryItemDeletedWebhookEvent>();
        Append<DictionaryItemSavedWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddDomainWebhooks()
    {
        Append<DomainDeletedWebhookEvent>();
        Append<DomainSavedWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddLanguageWebhooks()
    {
        Append<LanguageDeletedWebhookEvent>();
        Append<LanguageSavedWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddMediaWebhooks()
    {
        // Even though these two are in the AddCoreWebhooks()
        // The job of the CollectionBuilder should be removing duplicates
        // Would allow someone to use .AddCoreWebhooks().AddMediaWebhooks()
        // Or if they explicitly they could skip over CoreWebHooks and just add this perhaps
        Append<MediaDeletedWebhookEvent>();
        Append<MediaSavedWebhookEvent>();

        Append<MediaEmptiedRecycleBinWebhookEvent>();
        Append<MediaMovedWebhookEvent>();
        Append<MediaMovedToRecycleBinWebhookEvent>();

        Append<MediaTypeChangedWebhookEvent>();
        Append<MediaTypeDeletedWebhookEvent>();
        Append<MediaTypeMovedWebhookEvent>();
        Append<MediaTypeSavedWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddMemberWebhooks()
    {
        Append<AssignedMemberRolesWebhookEvent>();
        Append<ExportedMemberWebhookEvent>();
        Append<MemberDeletedWebhookEvent>();
        Append<MemberGroupDeletedWebhookEvent>();
        Append<MemberGroupSavedWebhookEvent>();
        Append<MemberSavedWebhookEvent>();
        Append<RemovedMemberRolesWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddMemberTypeWebhooks()
    {
        Append<MemberTypeChangedWebhookEvent>();
        Append<MemberTypeDeletedWebhookEvent>();
        Append<MemberTypeMovedWebhookEvent>();
        Append<MemberTypeSavedWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddPackageWebhooks()
    {
        Append<ImportedPackageWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddPublicAccessWebhooks()
    {
        Append<PublicAccessEntryDeletedWebhookEvent>();
        Append<PublicAccessEntrySavedWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddRelationWebhooks()
    {
        Append<RelationDeletedWebhookEvent>();
        Append<RelationSavedWebhookEvent>();

        Append<RelationTypeDeletedWebhookEvent>();
        Append<RelationTypeSavedWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddScriptWebhooks()
    {
        Append<ScriptDeletedWebhookEvent>();
        Append<ScriptSavedWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddStylesheetWebhooks()
    {
        Append<StylesheetDeletedWebhookEvent>();
        Append<StylesheetSavedWebhookEvent>();
        return this;
    }

    public WebhookEventCollectionBuilder AddTemplateWebhooks()
    {
        Append<PartialViewDeletedWebhookEvent>();
        Append<PartialViewSavedWebhookEvent>();

        Append<TemplateDeletedWebhookEvent>();
        Append<TemplateSavedWebhookEvent>();
        return this;
    }
}
