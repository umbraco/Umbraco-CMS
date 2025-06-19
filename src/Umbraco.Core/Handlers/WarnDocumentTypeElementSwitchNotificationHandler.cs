using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Core.Handlers;

public class WarnDocumentTypeElementSwitchNotificationHandler :
    INotificationAsyncHandler<ContentTypeSavingNotification>,
    INotificationAsyncHandler<ContentTypeSavedNotification>
{
    private const string NotificationStateKey =
        "Umbraco.Cms.Core.Handlers.WarnDocumentTypeElementSwitchNotificationHandler";

    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly IContentTypeService _contentTypeService;
    private readonly IElementSwitchValidator _elementSwitchValidator;

    public WarnDocumentTypeElementSwitchNotificationHandler(
        IEventMessagesFactory eventMessagesFactory,
        IContentTypeService contentTypeService,
        IElementSwitchValidator elementSwitchValidator)
    {
        _eventMessagesFactory = eventMessagesFactory;
        _contentTypeService = contentTypeService;
        _elementSwitchValidator = elementSwitchValidator;
    }

    // To figure out whether a warning should be generated, we need both the state before and after saving
    public async Task HandleAsync(ContentTypeSavingNotification notification, CancellationToken cancellationToken)
    {
        IEnumerable<Guid> updatedKeys = notification.SavedEntities
            .Where(e => e.HasIdentity)
            .Select(e => e.Key);

        IEnumerable<IContentType> persistedItems = _contentTypeService.GetMany(updatedKeys);

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
            if (stateInformation.TryGetValue(savedDocumentType.Key, out DocumentTypeElementSwitchInformation? state) is false)
            {
                continue;
            }

            if (state.WasElement == savedDocumentType.IsElement)
            {
                // no change
                continue;
            }

            await WarnIfAncestorsAreMisaligned(savedDocumentType, eventMessages);
            await WarnIfDescendantsAreMisaligned(savedDocumentType, eventMessages);
        }
    }

    private async Task WarnIfAncestorsAreMisaligned(IContentType contentType, EventMessages eventMessages)
    {
        if (await _elementSwitchValidator.AncestorsAreAlignedAsync(contentType) == false)
        {
            // todo update this message when the format has been agreed upon on with the client
            eventMessages.Add(new EventMessage(
                "DocumentType saved",
                "One or more ancestors have a mismatching element flag",
                EventMessageType.Warning));
        }
    }

    private async Task WarnIfDescendantsAreMisaligned(IContentType contentType, EventMessages eventMessages)
    {
        if (await _elementSwitchValidator.DescendantsAreAlignedAsync(contentType) == false)
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
