using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

internal sealed class ElementVersionService : ContentVersionServiceBase<IElement>, IElementVersionService
{
    public ElementVersionService(
        ILogger<ElementVersionService> logger,
        IElementVersionRepository elementVersionRepository,
        IContentVersionCleanupPolicy contentVersionCleanupPolicy,
        ICoreScopeProvider scopeProvider,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        ILanguageRepository languageRepository,
        IEntityService entityService,
        IElementService contentService,
        IUserIdKeyResolver userIdKeyResolver,
        IOptionsMonitor<ContentSettings> contentSettings)
        : base(
            logger,
            elementVersionRepository,
            contentVersionCleanupPolicy,
            scopeProvider,
            eventMessagesFactory,
            auditService,
            languageRepository,
            entityService,
            contentService,
            userIdKeyResolver,
            contentSettings)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Element;

    protected override DeletingVersionsNotification<IElement> DeletingVersionsNotification(int id, EventMessages messages, int specificVersion)
        => new ElementDeletingVersionsNotification(id, messages, specificVersion);

    protected override DeletedVersionsNotification<IElement> DeletedVersionsNotification(int id, EventMessages messages, int specificVersion)
        => new ElementDeletedVersionsNotification(id, messages, specificVersion);
}
