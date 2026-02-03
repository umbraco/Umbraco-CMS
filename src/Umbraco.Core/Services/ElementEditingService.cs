using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

internal sealed class ElementEditingService
    : ContentEditingServiceBase<IElement, IContentType, IElementService, IContentTypeService>, IElementEditingService
{
    private readonly ILogger<ElementEditingService> _logger;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IElementContainerService _containerService;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly IIdKeyMap _idKeyMap;
    private readonly IAuditService _auditService;

    public ElementEditingService(
        IElementService elementService,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ElementEditingService> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        IElementValidationService validationService,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService,
        IElementContainerService containerService,
        ContentTypeFilterCollection contentTypeFilters,
        IEventMessagesFactory eventMessagesFactory,
        IIdKeyMap idKeyMap,
        IAuditService auditService)
        : base(
            elementService,
            contentTypeService,
            propertyEditorCollection,
            dataTypeService,
            logger,
            scopeProvider,
            userIdKeyResolver,
            validationService,
            optionsMonitor,
            relationService,
            contentTypeFilters)
    {
        _logger = logger;
        _userIdKeyResolver = userIdKeyResolver;
        _containerService = containerService;
        _eventMessagesFactory = eventMessagesFactory;
        _idKeyMap = idKeyMap;
        _auditService = auditService;
    }

    public Task<IElement?> GetAsync(Guid key)
    {
        IElement? element = ContentService.GetById(key);
        return Task.FromResult(element);
    }

    // TODO ELEMENTS: implement validation here
    public Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(ElementCreateModel createModel, Guid userKey)
        => Task.FromResult(Attempt<ContentValidationResult, ContentEditingOperationStatus>.Succeed(ContentEditingOperationStatus.Success, new ()));

    // TODO ELEMENTS: implement validation here
    public Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, ValidateElementUpdateModel updateModel, Guid userKey)
        => Task.FromResult(Attempt<ContentValidationResult, ContentEditingOperationStatus>.Succeed(ContentEditingOperationStatus.Success, new ()));

    public async Task<Attempt<ElementCreateResult, ContentEditingOperationStatus>> CreateAsync(ElementCreateModel createModel, Guid userKey)
    {
        if (await ValidateCulturesAsync(createModel) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.InvalidCulture, new ElementCreateResult());
        }

        Attempt<ElementCreateResult, ContentEditingOperationStatus> result = await MapCreate<ElementCreateResult>(createModel);
        if (result.Success == false)
        {
            return result;
        }

        // the create mapping might succeed, but this doesn't mean the model is valid at property level.
        // we'll return the actual property validation status if the entire operation succeeds.
        ContentEditingOperationStatus validationStatus = result.Status;
        ContentValidationResult validationResult = result.Result.ValidationResult;

        // TODO ELEMENTS: we need a fix for this; see ContentEditingService
        IElement element = result.Result.Content!;
        // IElement element = await EnsureOnlyAllowedFieldsAreUpdated(result.Result.Content!, userKey);

        ContentEditingOperationStatus saveStatus = await SaveAsync(element, userKey);
        return saveStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new ElementCreateResult { Content = element, ValidationResult = validationResult })
            : Attempt.FailWithStatus(saveStatus, new ElementCreateResult { Content = element });
    }

    public async Task<Attempt<ElementUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ElementUpdateModel updateModel, Guid userKey)
    {
        IElement? element = ContentService.GetById(key);
        if (element is null)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.NotFound, new ElementUpdateResult());
        }

        if (await ValidateCulturesAsync(updateModel) is false)
        {
            return Attempt.FailWithStatus(ContentEditingOperationStatus.InvalidCulture, new ElementUpdateResult { Content = element });
        }

        Attempt<ElementUpdateResult, ContentEditingOperationStatus> result = await MapUpdate<ElementUpdateResult>(element, updateModel);
        if (result.Success == false)
        {
            return Attempt.FailWithStatus(result.Status, result.Result);
        }

        // the update mapping might succeed, but this doesn't mean the model is valid at property level.
        // we'll return the actual property validation status if the entire operation succeeds.
        ContentEditingOperationStatus validationStatus = result.Status;
        ContentValidationResult validationResult = result.Result.ValidationResult;

        // TODO ELEMENTS: we need a fix for this; see ContentEditingService
        // element = await EnsureOnlyAllowedFieldsAreUpdated(element, userKey);

        ContentEditingOperationStatus saveStatus = await SaveAsync(element, userKey);
        return saveStatus == ContentEditingOperationStatus.Success
            ? Attempt.SucceedWithStatus(validationStatus, new ElementUpdateResult { Content = element, ValidationResult = validationResult })
            : Attempt.FailWithStatus(saveStatus, new ElementUpdateResult { Content = element });
    }

    public async Task<Attempt<IElement?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey, false);

    public async Task<Attempt<IElement?, ContentEditingOperationStatus>> DeleteFromRecycleBinAsync(Guid key, Guid userKey)
        => await HandleDeleteAsync(key, userKey, true);

    protected override IElement New(string? name, int parentId, IContentType contentType)
        => new Element(name, parentId, contentType);

    protected override async Task<(int? ParentId, ContentEditingOperationStatus OperationStatus)> TryGetAndValidateParentIdAsync(Guid? parentKey, IContentType contentType)
    {
        if (parentKey.HasValue is false)
        {
            return (Constants.System.Root, ContentEditingOperationStatus.Success);
        }

        EntityContainer? container = await _containerService.GetAsync(parentKey.Value);
        return container is not null
            ? (container.Id, ContentEditingOperationStatus.Success)
            : (null, ContentEditingOperationStatus.ParentNotFound);
    }

    public async Task<Attempt<ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? containerKey, Guid userKey)
    {
        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ElementTree);

        var parentId = Constants.System.Root;
        if (containerKey.HasValue && containerKey.Value != Guid.Empty)
        {
            EntityContainer? container = await _containerService.GetAsync(containerKey.Value);
            if (container is null)
            {
                return Attempt.Fail(ContentEditingOperationStatus.ParentNotFound);
            }

            if (container.Trashed)
            {
                // cannot move to a trashed container
                return Attempt.Fail(ContentEditingOperationStatus.InTrash);
            }

            parentId = container.Id;
        }

        Attempt<ContentEditingOperationStatus> moveResult = await MoveLockedAsync(
            scope,
            key,
            parentId,
            false,
            userKey,
            (element, eventMessages) =>
            {
                var moveEventInfo = new MoveEventInfo<IElement>(element, element.Path, parentId, containerKey);
                return new ElementMovingNotification(moveEventInfo, eventMessages);
            },
            (element, eventMessages) =>
            {
                var moveEventInfo = new MoveEventInfo<IElement>(element, element.Path, parentId, containerKey);
                return new ElementMovedNotification(moveEventInfo, eventMessages);
            });

        scope.Complete();

        return moveResult;
    }

    public async Task<Attempt<ContentEditingOperationStatus>> MoveToRecycleBinAsync(Guid key, Guid userKey)
    {
        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ElementTree);

        var originalPath = string.Empty;
        Attempt<ContentEditingOperationStatus> moveResult = await MoveLockedAsync(
            scope,
            key,
            Constants.System.RecycleBinElement,
            true,
            userKey,
            (element, eventMessages) =>
            {
                originalPath = element.Path;
                var moveEventInfo = new MoveToRecycleBinEventInfo<IElement>(element, originalPath);
                return new ElementMovingToRecycleBinNotification(moveEventInfo, eventMessages);
            },
            (element, eventMessages) =>
            {
                var moveEventInfo = new MoveToRecycleBinEventInfo<IElement>(element, originalPath);
                return new ElementMovedToRecycleBinNotification(moveEventInfo, eventMessages);
            });

        scope.Complete();

        return moveResult;
    }

    public async Task<Attempt<IElement?, ContentEditingOperationStatus>> CopyAsync(Guid key, Guid? parentKey, Guid userKey)
        => await HandleCopyAsync(key, parentKey, false, false, userKey);

    internal static async Task<bool> UnpublishTrashedElementOnRestore(IElement element, Guid userKey, IElementService elementService, IUserIdKeyResolver userIdKeyResolver, ILogger logger)
    {
        // this only applies to trashed, published elements
        if (element is not { Trashed: true, Published: true })
        {
            return true;
        }

        var userId = await userIdKeyResolver.GetAsync(userKey);
        PublishResult result = elementService.Unpublish(element, "*", userId);

        // we will accept if custom code cancels the unpublish operation here - all other error states should
        // result in a failed move.
        if (result.Success is false && result.Result is not PublishResultType.FailedUnpublishCancelledByEvent)
        {
            logger.LogError("An error occurred while attempting to unpublish an element being moved from the recycle bin. Status was: {unpublishStatus}", result.Result);
            return false;
        }

        return true;
    }

    private async Task<Attempt<ContentEditingOperationStatus>> MoveLockedAsync<TNotification>(
        ICoreScope scope,
        Guid key,
        int parentId,
        bool trash,
        Guid userKey,
        Func<IElement, EventMessages, TNotification> movingNotificationFactory,
        Func<IElement, EventMessages, IStatefulNotification> movedNotificationFactory)
        where TNotification : IStatefulNotification, ICancelableNotification
    {
        IElement? toMove = await GetAsync(key);
        if (toMove is null)
        {
            return Attempt.Fail(ContentEditingOperationStatus.NotFound);
        }

        // Capture original path before any modifications (needed for audit message when trashing)
        var originalPath = toMove.Path;

        if (toMove.ParentId == parentId)
        {
            return Attempt.Succeed(ContentEditingOperationStatus.Success);
        }

        EventMessages eventMessages = _eventMessagesFactory.Get();

        TNotification movingNotification = movingNotificationFactory(toMove,  eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(movingNotification))
        {
            return Attempt.Fail(ContentEditingOperationStatus.CancelledByNotification);
        }

        var unpublishSuccess = await UnpublishTrashedElementOnRestore(toMove, userKey, ContentService, _userIdKeyResolver, _logger);
        if (unpublishSuccess is false)
        {
            return Attempt.Fail(ContentEditingOperationStatus.Unknown);
        }

        // NOTE: as long as the parent ID is correct, the element repo takes care of updating the rest of the
        //       structural node data like path, level, sort orders etc.
        toMove.ParentId = parentId;

        // NOTE: this cast isn't pretty, but it's the best we can do now. the content and media services do something
        //       similar, and at the time of writing this, we are subject to the limitations imposed there.
        ((TreeEntityBase)toMove).Trashed = trash;

        ContentEditingOperationStatus saveResult = await SaveAsync(toMove, userKey);
        if (saveResult is not ContentEditingOperationStatus.Success)
        {
            return Attempt.Fail(saveResult);
        }

        await AuditMoveAsync(userKey, toMove.Id, trash, originalPath);

        IStatefulNotification movedNotification = movedNotificationFactory(toMove, eventMessages);
        scope.Notifications.Publish(movedNotification.WithStateFrom(movingNotification));

        return Attempt.Succeed(ContentEditingOperationStatus.Success);
    }

    private async Task AuditMoveAsync(Guid userKey, int elementId, bool trash, string originalPath)
    {
        string? message = null;
        if (trash)
        {
            IList<string> pathSegments = originalPath.ToDelimitedList();
            var originalParentId = pathSegments.Count > 2
                ? int.Parse(pathSegments[^2], CultureInfo.InvariantCulture)
                : Constants.System.Root;
            message = $"Moved to recycle bin from parent {originalParentId}";
        }

        await _auditService.AddAsync(AuditType.Move, userKey, elementId, UmbracoObjectTypes.Element.GetName(), message);
    }

    protected override IElement? Copy(IElement element, int newParentId, bool relateToOriginal, bool includeDescendants, int userId)
    {
        Guid? newParentKey;
        if (newParentId is Constants.System.Root)
        {
            newParentKey = Constants.System.RootKey;
        }
        else
        {
            Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(newParentId, UmbracoObjectTypes.ElementContainer);
            if (parentKeyAttempt.Success is false)
            {
                return null;
            }

            newParentKey = parentKeyAttempt.Result;
        }

        using ICoreScope scope = CoreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ElementTree);

        EventMessages eventMessages = _eventMessagesFactory.Get();

        IElement copy = element.DeepCloneWithResetIdentities();
        copy.ParentId = newParentId;

        var copyingNotification = new ElementCopyingNotification(element, copy, newParentId, newParentKey, eventMessages);
        if (scope.Notifications.PublishCancelable(copyingNotification))
        {
            scope.Complete();
            return null;
        }

        // update published state (copies cannot be published)
        copy.Published = false;

        // update creator and writer IDs
        copy.CreatorId = userId;
        copy.WriterId = userId;

        OperationResult saveResult = ContentService.Save(copy, userId);
        if (saveResult.Success is false)
        {
            return null;
        }

        scope.Notifications.Publish(
            new ElementCopiedNotification(element, copy, newParentId, newParentKey, relateToOriginal, eventMessages)
                .WithStateFrom(copyingNotification));

        scope.Complete();

        return copy;
    }

    protected override OperationResult? MoveToRecycleBin(IElement element, int userId)
        => throw new NotImplementedException("TODO ELEMENTS: implement recycle bin");

    protected override OperationResult? Delete(IElement element, int userId)
        => ContentService.Delete(element, userId);

    // NOTE: We have a custom implementation for Move because ContentEditingServiceBase has no concept of Containers.
    protected override OperationResult? Move(IElement element, int newParentId, int userId) => throw new NotImplementedException();

    private async Task<ContentEditingOperationStatus> SaveAsync(IElement content, Guid userKey)
    {
        try
        {
            var currentUserId = await GetUserIdAsync(userKey);
            OperationResult saveResult = ContentService.Save(content, currentUserId);
            return saveResult.Result switch
            {
                // these are the only result states currently expected from Save
                OperationResultType.Success => ContentEditingOperationStatus.Success,
                OperationResultType.FailedCancelledByEvent => ContentEditingOperationStatus.CancelledByNotification,

                // for any other state we'll return "unknown" so we know that we need to amend this
                _ => ContentEditingOperationStatus.Unknown
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Content save operation failed");
            return ContentEditingOperationStatus.Unknown;
        }
    }
}
