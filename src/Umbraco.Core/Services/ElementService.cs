using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages elements.
/// </summary>
/// <remarks>
///     There is no asynchronous element repository, so the asynchronous contract is satisfied by bridging onto the
///     synchronous engine inherited from <see cref="PublishableContentServiceBase{TContent}" />.
/// </remarks>
public class ElementService : PublishableContentServiceBase<IElement>, IElementService
{
    private readonly IElementRepository _elementRepository;
    private readonly ILogger<ElementService> _logger;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public ElementService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        IContentTypeRepository contentTypeRepository,
        IElementRepository elementRepository,
        ILanguageRepository languageRepository,
        Lazy<IPropertyValidationService> propertyValidationService,
        ICultureImpactFactory cultureImpactFactory,
        IUserIdKeyResolver userIdKeyResolver,
        PropertyEditorCollection propertyEditorCollection,
        IIdKeyMap idKeyMap,
        IShortStringHelper shortStringHelper)
        : base(
            provider,
            loggerFactory,
            eventMessagesFactory,
            auditService,
            contentTypeRepository,
            elementRepository,
            languageRepository,
            propertyValidationService,
            cultureImpactFactory,
            userIdKeyResolver,
            propertyEditorCollection,
            idKeyMap)
    {
        _elementRepository = elementRepository;
        _shortStringHelper = shortStringHelper;
        _logger = loggerFactory.CreateLogger<ElementService>();
        _userIdKeyResolver = userIdKeyResolver;
    }

    #region Others

    /// <inheritdoc />
    public Task<IElement?> GetByIdAsync(Guid key, CancellationToken cancellationToken) => Task.FromResult(GetById(key));

    /// <inheritdoc />
    public Task<IEnumerable<IElement>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken) => Task.FromResult(GetByIds(ids));

    /// <inheritdoc />
    public Task<Attempt<ContentScheduleOperationStatus>> PersistContentScheduleAsync(IPublishableContentBase content, ContentScheduleCollection contentSchedule, CancellationToken cancellationToken)
    {
        PersistContentSchedule(content, contentSchedule);
        return Task.FromResult(Attempt.Succeed(ContentScheduleOperationStatus.Success));
    }

    /// <inheritdoc />
    public async Task<PublishResult> PublishAsync(IElement content, string[] cultures, Guid userKey, CancellationToken cancellationToken)
    {
        int userId = await _userIdKeyResolver.GetAsync(userKey);
        return Publish(content, cultures, userId);
    }

    /// <inheritdoc />
    public async Task<PublishResult> UnpublishAsync(IElement content, string? culture, Guid userKey, CancellationToken cancellationToken)
    {
        int userId = await _userIdKeyResolver.GetAsync(userKey);
        return Unpublish(content, culture, userId);
    }

    /// <inheritdoc />
    public async Task<PublishResult> SaveAndPublishAsync(IElement content, string[] culturesToPublish, Guid userKey, CancellationToken cancellationToken)
    {
        int userId = await _userIdKeyResolver.GetAsync(userKey);
        return SaveAndPublish(content, culturesToPublish, userId);
    }

    /// <inheritdoc />
    public Task<IEnumerable<PublishResult>> PerformScheduledPublishAsync(DateTime date, CancellationToken cancellationToken)
        => Task.FromResult(PerformScheduledPublish(date));

    /// <inheritdoc />
    public async Task<Attempt<ContentRollbackOperationStatus>> RollbackAsync(Guid key, int versionId, string culture, Guid userKey, CancellationToken cancellationToken)
    {
        Attempt<int> idAttempt = await IdKeyMap.GetIdForKeyAsync(key, ContentObjectType);
        if (idAttempt.Success == false)
        {
            return Attempt.Fail(ContentRollbackOperationStatus.ContentNotFound);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);
        OperationResult result = Rollback(idAttempt.Result, versionId, culture, userId);

        return result.Result switch
        {
            OperationResultType.Success => Attempt.Succeed(ContentRollbackOperationStatus.Success),
            OperationResultType.FailedCancelledByEvent => Attempt.Fail(ContentRollbackOperationStatus.CancelledByNotification),
            OperationResultType.FailedCannot => Attempt.Fail(ContentRollbackOperationStatus.ContentNotFound),
            _ => Attempt.Fail(ContentRollbackOperationStatus.SaveFailed),
        };
    }

    /// <inheritdoc />
    public Task<IDictionary<Guid, IEnumerable<ContentSchedule>>> GetContentSchedulesByKeysAsync(Guid[] keys, CancellationToken cancellationToken) =>
        Task.FromResult(GetContentSchedulesByKeys(keys));

    /// <inheritdoc />
    public Task<ContentScheduleCollection> GetContentScheduleByContentIdAsync(Guid contentId, CancellationToken cancellationToken) =>
        Task.FromResult(GetContentScheduleByContentId(contentId));

    /// <inheritdoc />
    /// <remarks>
    ///     A synchronous save reports failure only when a notification handler cancels it - its validation
    ///     failures throw - so every non-success result maps to <see cref="ContentSaveOperationStatus.CancelledByNotification" />.
    /// </remarks>
    public async Task<Attempt<ContentSaveOperationStatus>> SaveAsync(IElement content, Guid userKey, ContentScheduleCollection? contentSchedule, CancellationToken cancellationToken)
    {
        int userId = await _userIdKeyResolver.GetAsync(userKey);
        OperationResult result = Save(content, userId, contentSchedule);
        return result.Success
            ? Attempt.Succeed(ContentSaveOperationStatus.Success)
            : Attempt.Fail(ContentSaveOperationStatus.CancelledByNotification);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentDeleteOperationStatus>> DeleteAsync(IElement content, Guid userKey, CancellationToken cancellationToken)
    {
        int userId = await _userIdKeyResolver.GetAsync(userKey);
        OperationResult result = Delete(content, userId);
        return result.Success
            ? Attempt.Succeed(ContentDeleteOperationStatus.Success)
            : Attempt.Fail(ContentDeleteOperationStatus.CancelledByNotification);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     A synchronous save reports failure only when a notification handler cancels it - its validation
    ///     failures throw - so every non-success result maps to <see cref="ContentSaveOperationStatus.CancelledByNotification" />.
    /// </remarks>
    public async Task<Attempt<ContentSaveOperationStatus>> SaveAsync(IEnumerable<IElement> contents, Guid userKey, CancellationToken cancellationToken)
    {
        int userId = await _userIdKeyResolver.GetAsync(userKey);
        OperationResult result = Save(contents, userId);
        return result.Success
            ? Attempt.Succeed(ContentSaveOperationStatus.Success)
            : Attempt.Fail(ContentSaveOperationStatus.CancelledByNotification);
    }

    /// <inheritdoc />
    public override ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options)
        => CheckDataIntegrity(
            options,
            scope =>
            {
                // The event args needs a content item so we'll make a fake one with enough properties to not cause a null ref
                var root = new Element("root", -1, new ContentType(_shortStringHelper, -1)) { Id = -1, Key = Guid.Empty };
                scope.Notifications.Publish(new ElementTreeChangeNotification(root, TreeChangeTypes.RefreshAll, EventMessagesFactory.Get()));
            });

    /// <inheritdoc />
    public Task<ContentDataIntegrityReport> CheckDataIntegrityAsync(ContentDataIntegrityReportOptions options, CancellationToken cancellationToken)
        => Task.FromResult(CheckDataIntegrity(options));

    #endregion

    #region Content Types

    /// <inheritdoc/>
    public override OperationResult DeleteOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId)
    {
        var changes = new List<TreeChange<IElement>>();
        var contentTypeIdsA = contentTypeIds.ToArray();
        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(WriteLockIds);

        IQuery<IElement> query = Query<IElement>().WhereIn(x => x.ContentTypeId, contentTypeIdsA);
        IElement[] elements = _elementRepository.Get(query).ToArray();

        if (elements.Length is 0)
        {
            scope.Complete();
            return OperationResult.Succeed(eventMessages);
        }

        if (scope.Notifications.PublishCancelable(new ElementDeletingNotification(elements, eventMessages)))
        {
            scope.Complete();
            return OperationResult.Cancel(eventMessages);
        }

        foreach (IElement element in elements)
        {
            // if it's not trashed yet, and published, we should unpublish
            // but... Unpublishing event makes no sense (not going to cancel?) and no need to save
            // just raise the event
            if (element.Trashed == false && element.Published)
            {
                scope.Notifications.Publish(new ElementUnpublishedNotification(
                    element,
                    eventMessages,
                    BuildCultureMap(element, element.ContentType.VariesByCulture() ? element.PublishedCultures : ["*"])));
            }

            // delete content
            // triggers the deleted event
            DeleteLocked(scope, element, eventMessages);
            changes.Add(new TreeChange<IElement>(element, TreeChangeTypes.Remove));
        }

        scope.Notifications.Publish(new ElementTreeChangeNotification(changes, eventMessages));

        Audit(AuditType.Delete, userId, Constants.System.Root, $"Delete element of type {string.Join(",", contentTypeIdsA)}");

        scope.Complete();
        return OperationResult.Succeed(eventMessages);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentDeleteOfTypesOperationStatus>> DeleteOfTypesAsync(IEnumerable<Guid> contentTypeKeys, Guid userKey, CancellationToken cancellationToken)
    {
        var contentTypeIds = new List<int>();
        foreach (Guid contentTypeKey in contentTypeKeys)
        {
            Attempt<int> idAttempt = await IdKeyMap.GetIdForKeyAsync(contentTypeKey, UmbracoObjectTypes.DocumentType);
            if (idAttempt.Success == false)
            {
                return Attempt.Fail(ContentDeleteOfTypesOperationStatus.NotFound);
            }

            contentTypeIds.Add(idAttempt.Result);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);
        OperationResult result = DeleteOfTypes(contentTypeIds, userId);
        return result.Success
            ? Attempt.Succeed(ContentDeleteOfTypesOperationStatus.Success)
            : Attempt.Fail(ContentDeleteOfTypesOperationStatus.CancelledByNotification);
    }

    #endregion

    #region Abstract implementations

    protected override UmbracoObjectTypes ContentObjectType => UmbracoObjectTypes.Element;

    protected override int[] ReadLockIds => WriteLockIds;

    protected override int[] WriteLockIds => new[] { Constants.Locks.ElementTree };

    protected override bool SupportsBranchPublishing => false;

    protected override ILogger<ElementService> Logger => _logger;

    protected override void DeleteLocked(ICoreScope scope, IElement content, EventMessages evtMsgs)
    {
        _elementRepository.Delete(content);
        scope.Notifications.Publish(new ElementDeletedNotification(content, evtMsgs));
    }

    protected override SavingNotification<IElement> SavingNotification(IElement content, EventMessages eventMessages)
        => new ElementSavingNotification(content, eventMessages);

    protected override SavedNotification<IElement> SavedNotification(IElement content, EventMessages eventMessages)
        => new ElementSavedNotification(content, eventMessages);

    protected override SavedNotification<IElement> SavedNotification(IElement content, EventMessages eventMessages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? savedCultures)
        => new ElementSavedNotification(content, eventMessages, savedCultures);

    protected override SavingNotification<IElement> SavingNotification(IEnumerable<IElement> content, EventMessages eventMessages)
        => new ElementSavingNotification(content, eventMessages);

    protected override SavedNotification<IElement> SavedNotification(IEnumerable<IElement> content, EventMessages eventMessages)
        => new ElementSavedNotification(content, eventMessages);

    protected override SavedNotification<IElement> SavedNotification(IEnumerable<IElement> content, EventMessages eventMessages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? savedCultures)
        => new ElementSavedNotification(content, eventMessages, savedCultures);

    protected override TreeChangeNotification<IElement> TreeChangeNotification(IElement content, TreeChangeTypes changeTypes, EventMessages eventMessages)
        => new ElementTreeChangeNotification(content, changeTypes, eventMessages);

    protected override TreeChangeNotification<IElement> TreeChangeNotification(IElement content, TreeChangeTypes changeTypes, IEnumerable<string>? publishedCultures, IEnumerable<string>? unpublishedCultures, EventMessages eventMessages)
        => new ElementTreeChangeNotification(content, changeTypes, publishedCultures, unpublishedCultures, eventMessages);

    protected override TreeChangeNotification<IElement> TreeChangeNotification(IEnumerable<IElement> content, TreeChangeTypes changeTypes, EventMessages eventMessages)
        => new ElementTreeChangeNotification(content, changeTypes, eventMessages);

    protected override DeletingNotification<IElement> DeletingNotification(IElement content, EventMessages eventMessages)
        => new ElementDeletingNotification(content, eventMessages);

    protected override CancelableEnumerableObjectNotification<IElement> PublishingNotification(IElement content, EventMessages eventMessages)
        => new ElementPublishingNotification(content, eventMessages);

    protected override IStatefulNotification PublishedNotification(IElement content, EventMessages eventMessages)
        => new ElementPublishedNotification(content, eventMessages);

    protected override IStatefulNotification PublishedNotification(IElement content, EventMessages eventMessages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? publishedCultures, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? unpublishedCultures)
        => new ElementPublishedNotification(content, eventMessages, publishedCultures, unpublishedCultures);

    protected override IStatefulNotification PublishedNotification(IEnumerable<IElement> content, EventMessages eventMessages)
        => new ElementPublishedNotification(content, eventMessages);

    protected override CancelableEnumerableObjectNotification<IElement> UnpublishingNotification(IElement content, EventMessages eventMessages)
        => new ElementUnpublishingNotification(content, eventMessages);

    protected override IStatefulNotification UnpublishedNotification(IElement content, EventMessages eventMessages)
        => new ElementUnpublishedNotification(content, eventMessages);

    protected override IStatefulNotification UnpublishedNotification(IElement content, EventMessages eventMessages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? unpublishedCultures)
        => new ElementUnpublishedNotification(content, eventMessages, unpublishedCultures);

    protected override RollingBackNotification<IElement> RollingBackNotification(IElement target, EventMessages messages)
        => new ElementRollingBackNotification(target, messages);

    protected override RolledBackNotification<IElement> RolledBackNotification(IElement target, EventMessages messages)
        => new ElementRolledBackNotification(target, messages);

    #endregion
}
