using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Core.Handlers;

/// <summary>
///     Handles notifications for document type element switch validation and generates warnings
///     when the element flag change creates misalignment with ancestors or descendants.
/// </summary>
/// <remarks>
///     <para>
///         This handler monitors <see cref="ContentTypeSavingNotification"/> and <see cref="ContentTypeSavedNotification"/>
///         to detect when a document type's <see cref="IContentTypeBase.IsElement"/> property changes.
///     </para>
///     <para>
///         When a change is detected, it validates that the element flag is consistent with ancestor
///         and descendant content types, generating warnings if misalignment is found.
///     </para>
/// </remarks>
public class WarnDocumentTypeElementSwitchNotificationHandler :
    INotificationAsyncHandler<ContentTypeSavingNotification>,
    INotificationAsyncHandler<ContentTypeSavedNotification>
{
    private const string NotificationStateKey =
        "Umbraco.Cms.Core.Handlers.WarnDocumentTypeElementSwitchNotificationHandler";

    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly IContentTypeService _contentTypeService;
    private readonly IElementSwitchValidator _elementSwitchValidator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WarnDocumentTypeElementSwitchNotificationHandler"/> class.
    /// </summary>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="elementSwitchValidator">The validator for element switch operations.</param>
    public WarnDocumentTypeElementSwitchNotificationHandler(
        IEventMessagesFactory eventMessagesFactory,
        IContentTypeService contentTypeService,
        IElementSwitchValidator elementSwitchValidator)
    {
        _eventMessagesFactory = eventMessagesFactory;
        _contentTypeService = contentTypeService;
        _elementSwitchValidator = elementSwitchValidator;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     To figure out whether a warning should be generated, we need both the state before and after saving.
    /// </remarks>
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

    /// <inheritdoc />
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

    /// <summary>
    ///     Adds a warning event message if the content type's ancestors have misaligned element flags.
    /// </summary>
    /// <param name="contentType">The content type to validate.</param>
    /// <param name="eventMessages">The event messages collection to add warnings to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    ///     Adds a warning event message if the content type's descendants have misaligned element flags.
    /// </summary>
    /// <param name="contentType">The content type to validate.</param>
    /// <param name="eventMessages">The event messages collection to add warnings to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    ///     Holds state information about a document type's element flag before saving.
    /// </summary>
    private sealed class DocumentTypeElementSwitchInformation
    {
        /// <summary>
        ///     Gets or sets a value indicating whether the document type was an element type before saving.
        /// </summary>
        public bool WasElement { get; set; }
    }
}
