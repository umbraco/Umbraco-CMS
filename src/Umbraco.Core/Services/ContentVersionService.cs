using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

// ReSharper disable once CheckNamespace
namespace Umbraco.Cms.Core.Services;

internal sealed class ContentVersionService : ContentVersionServiceBase<IContent>, IContentVersionService
{
    public ContentVersionService(
        ILogger<ContentVersionService> logger,
        IDocumentVersionRepository contentVersionRepository,
        IContentVersionCleanupPolicy contentVersionCleanupPolicy,
        ICoreScopeProvider scopeProvider,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        ILanguageRepository languageRepository,
        IEntityService entityService,
        IContentService contentService,
        IUserIdKeyResolver userIdKeyResolver)
        : base(
            logger,
            contentVersionRepository,
            contentVersionCleanupPolicy,
            scopeProvider,
            eventMessagesFactory,
            auditService,
            languageRepository,
            entityService,
            contentService,
            userIdKeyResolver)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

    protected override DeletingVersionsNotification<IContent> DeletingVersionsNotification(int id, EventMessages messages, int specificVersion)
        => new ContentDeletingVersionsNotification(id, messages, specificVersion);

    protected override DeletedVersionsNotification<IContent> DeletedVersionsNotification(int id, EventMessages messages, int specificVersion)
        => new ContentDeletedVersionsNotification(id, messages, specificVersion);
}
