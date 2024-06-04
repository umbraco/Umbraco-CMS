using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Handlers;

public class WarnDocumentTypeElementSwitchNotificationHandler :
    INotificationAsyncHandler<ContentTypeSavingNotification>,
    INotificationAsyncHandler<ContentTypeSavedNotification>
{
    private const string NotificationStateKey =
        "Umbraco.Cms.Core.Handlers.WarnDocumentTypeElementSwitchNotificationHandler";

    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly IContentTypeService _contentTypeService;

    public WarnDocumentTypeElementSwitchNotificationHandler(
        IEventMessagesFactory eventMessagesFactory,
        IContentTypeService contentTypeService)
    {
        _eventMessagesFactory = eventMessagesFactory;
        _contentTypeService = contentTypeService;
    }

    // To figure out whether a warning should be generated, we need both the state before and after saving
    public async Task HandleAsync(ContentTypeSavingNotification notification, CancellationToken cancellationToken)
    {
        IEnumerable<Guid> updatedKeys = notification.SavedEntities
            .Where(e => e.HasIdentity)
            .Select(e => e.Key);

        IEnumerable<IContentType> persistedItems = _contentTypeService.GetAll(updatedKeys);

        var stateInformation = persistedItems
            .ToDictionary(
                contentType => contentType.Key,
                contentType => new DocumentTypeElementSwitchInformation { WasElement = contentType.IsElement });
        notification.State[NotificationStateKey] = stateInformation;
    }

    public async Task HandleAsync(ContentTypeSavedNotification notification, CancellationToken cancellationToken)
    {
        if (notification.State[NotificationStateKey] is not Dictionary<Guid, DocumentTypeElementSwitchInformation>
            stateInformation)
        {
            return;
        }

        EventMessages eventMessages = _eventMessagesFactory.Get();

        foreach (IContentType savedDocumentType in notification.SavedEntities)
        {
            if (stateInformation.ContainsKey(savedDocumentType.Key) is false)
            {
                continue;
            }

            DocumentTypeElementSwitchInformation state = stateInformation[savedDocumentType.Key];
            if (state.WasElement == savedDocumentType.IsElement)
            {
                // no change
                continue;
            }

            WarnIfAncestorsAreMisaligned(savedDocumentType, eventMessages);
            WarnIfDescendantsAreMisaligned(savedDocumentType, eventMessages);
        }
    }

    private void WarnIfAncestorsAreMisaligned(IContentType contentType, EventMessages eventMessages)
    {
        var ancestorIds = contentType.AncestorIds();
        if (ancestorIds.Length == 0)
        {
            return;
        }

        var ancestors = _contentTypeService
            .GetAll(ancestorIds).ToArray();
        var misMatchingAncestors = ancestors
            .Where(ancestor => ancestor.IsElement != contentType.IsElement).ToArray();

        if (misMatchingAncestors.Any())
        {
            // todo update this message when the format has been agreed upon on with the client
            eventMessages.Add(new EventMessage(
                "DocumentType saved",
                "One or more ancestors have a mismatching element flag",
                EventMessageType.Warning));
        }
    }

    private void WarnIfDescendantsAreMisaligned(IContentType contentType, EventMessages eventMessages)
    {
        IEnumerable<IContentType> descendants = _contentTypeService.GetDescendants(contentType.Id, false);

        if (descendants.Any(descendant => descendant.IsElement != contentType.IsElement))
        {
            // todo update this message when the format has been agreed upon on with the client
            eventMessages.Add(new EventMessage(
                "DocumentType saved",
                "One or more descendants have a mismatching element flag",
                EventMessageType.Warning));
        }
    }

    private class DocumentTypeElementSwitchInformation
    {
        public bool WasElement { get; set; }
    }
}
