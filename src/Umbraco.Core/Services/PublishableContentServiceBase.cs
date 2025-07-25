using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

// TODO ELEMENTS: ensure this implementation is up to date with the current state of ContentService
// TODO ELEMENTS: handle caching etc.
// TODO ELEMENTS: everything structural (children, ancestors, descendants, branches, sort) should be omitted
// TODO ELEMENTS: implement recycle bin
// TODO ELEMENTS: implement copy and move
// TODO ELEMENTS: rename _documentRepository to _contentRepository
public abstract class PublishableContentServiceBase<TContent> : RepositoryService
    where TContent : class, IPublishableContentBase
{
    private readonly IAuditRepository _auditRepository;
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly IPublishableContentRepository<TContent> _documentRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly Lazy<IPropertyValidationService> _propertyValidationService;
    private readonly ICultureImpactFactory _cultureImpactFactory;
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IIdKeyMap _idKeyMap;

    protected PublishableContentServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditRepository auditRepository,
        IContentTypeRepository contentTypeRepository,
        IPublishableContentRepository<TContent> contentRepository,
        ILanguageRepository languageRepository,
        Lazy<IPropertyValidationService> propertyValidationService,
        ICultureImpactFactory cultureImpactFactory,
        PropertyEditorCollection propertyEditorCollection,
        IIdKeyMap idKeyMap)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _auditRepository = auditRepository;
        _contentTypeRepository = contentTypeRepository;
        _documentRepository = contentRepository;
        _languageRepository = languageRepository;
        _propertyValidationService = propertyValidationService;
        _cultureImpactFactory = cultureImpactFactory;
        _propertyEditorCollection = propertyEditorCollection;
        _idKeyMap = idKeyMap;
    }

    protected abstract TContent CreateContentInstance(string name, int parentId, IContentType contentType, int userId);

    protected abstract TContent CreateContentInstance(string name, TContent parent, IContentType contentType, int userId);

    protected abstract UmbracoObjectTypes ContentObjectType { get; }

    protected abstract PublishResult CommitDocumentChanges(
        ICoreScope scope,
        TContent content,
        EventMessages eventMessages,
        IReadOnlyCollection<ILanguage> allLangs,
        IDictionary<string, object?>? notificationState,
        int userId);

    protected abstract void DeleteLocked(ICoreScope scope, TContent content, EventMessages evtMsgs);

    protected abstract int[] ReadLockIds { get; }

    protected abstract int[] WriteLockIds { get; }

    protected abstract ILogger<PublishableContentServiceBase<TContent>> Logger { get; }

    protected abstract SavingNotification<TContent> SavingNotification(TContent content, EventMessages eventMessages);

    protected abstract SavedNotification<TContent> SavedNotification(TContent content, EventMessages eventMessages);

    protected abstract SavingNotification<TContent> SavingNotification(IEnumerable<TContent> content, EventMessages eventMessages);

    protected abstract SavedNotification<TContent> SavedNotification(IEnumerable<TContent> content, EventMessages eventMessages);

    protected abstract TreeChangeNotification<TContent> TreeChangeNotification(TContent content, TreeChangeTypes changeTypes, EventMessages eventMessages);

    protected abstract TreeChangeNotification<TContent> TreeChangeNotification(IEnumerable<TContent> content, TreeChangeTypes changeTypes, EventMessages eventMessages);

    protected abstract DeletingNotification<TContent> DeletingNotification(TContent content, EventMessages eventMessages);

    // TODO ELEMENTS: create a base class for publishing notifications to reuse between IContent and IElement
    protected abstract IStatefulNotification UnpublishedNotification(TContent content, EventMessages eventMessages);

    // TODO ELEMENTS: same as above
    protected abstract CancelableEnumerableObjectNotification<TContent> PublishingNotification(TContent content, EventMessages eventMessages);

    protected abstract DeletingVersionsNotification<TContent> DeletingVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default);

    protected abstract DeletedVersionsNotification<TContent> DeletedVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default);


    #region Count

    public int CountPublished(string? contentTypeAlias = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.CountPublished(contentTypeAlias);
        }
    }

    public int Count(string? contentTypeAlias = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.Count(contentTypeAlias);
        }
    }

    public int CountChildren(int parentId, string? contentTypeAlias = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.CountChildren(parentId, contentTypeAlias);
        }
    }

    public int CountDescendants(int parentId, string? contentTypeAlias = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.CountDescendants(parentId, contentTypeAlias);
        }
    }

    #endregion

    #region Get, Has, Is


    /// <summary>
    ///     Gets an <see cref="TContent" /> object by Id
    /// </summary>
    /// <param name="id">Id of the Content to retrieve</param>
    /// <returns>
    ///     <see cref="TContent" />
    /// </returns>
    public TContent? GetById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.Get(id);
        }
    }

    /// <summary>
    ///     Gets an <see cref="TContent" /> object by Id
    /// </summary>
    /// <param name="ids">Ids of the Content to retrieve</param>
    /// <returns>
    ///     <see cref="TContent" />
    /// </returns>
    public IEnumerable<TContent> GetByIds(IEnumerable<int> ids)
    {
        var idsA = ids.ToArray();
        if (idsA.Length == 0)
        {
            return Enumerable.Empty<TContent>();
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IEnumerable<TContent> items = _documentRepository.GetMany(idsA);
            var index = items.ToDictionary(x => x.Id, x => x);
            return idsA.Select(x => index.GetValueOrDefault(x)).WhereNotNull();
        }
    }

    /// <summary>
    ///     Gets an <see cref="TContent" /> object by its 'UniqueId'
    /// </summary>
    /// <param name="key">Guid key of the Content to retrieve</param>
    /// <returns>
    ///     <see cref="TContent" />
    /// </returns>
    public TContent? GetById(Guid key)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.Get(key);
        }
    }

    /// <inheritdoc />
    public ContentScheduleCollection GetContentScheduleByContentId(int contentId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.GetContentSchedule(contentId);
        }
    }

    public ContentScheduleCollection GetContentScheduleByContentId(Guid contentId)
    {
        Attempt<int> idAttempt = _idKeyMap.GetIdForKey(contentId, UmbracoObjectTypes.Document);
        if (idAttempt.Success is false)
        {
            return new ContentScheduleCollection();
        }

        return GetContentScheduleByContentId(idAttempt.Result);
    }

    /// <inheritdoc />
    public void PersistContentSchedule(TContent content, ContentScheduleCollection contentSchedule)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            _documentRepository.PersistContentSchedule(content, contentSchedule);
            scope.Complete();
        }
    }

    /// <summary>
    ///     Gets <see cref="TContent" /> objects by Ids
    /// </summary>
    /// <param name="ids">Ids of the Content to retrieve</param>
    /// <returns>
    ///     <see cref="TContent" />
    /// </returns>
    public IEnumerable<TContent> GetByIds(IEnumerable<Guid> ids)
    {
        Guid[] idsA = ids.ToArray();
        if (idsA.Length == 0)
        {
            return Enumerable.Empty<TContent>();
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IEnumerable<TContent>? items = _documentRepository.GetMany(idsA);

            if (items is not null)
            {
                var index = items.ToDictionary(x => x.Key, x => x);

                return idsA.Select(x => index.GetValueOrDefault(x)).WhereNotNull();
            }

            return Enumerable.Empty<TContent>();
        }
    }

    /// <inheritdoc />
    public IEnumerable<TContent> GetPagedOfType(
        int contentTypeId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<TContent>? filter = null,
        Ordering? ordering = null)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        ordering ??= Ordering.By("sortOrder");

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.GetPage(
                Query<TContent>()?.Where(x => x.ContentTypeId == contentTypeId),
                pageIndex,
                pageSize,
                out totalRecords,
                filter,
                ordering);
        }
    }

    /// <inheritdoc />
    public IEnumerable<TContent> GetPagedOfTypes(int[] contentTypeIds, long pageIndex, int pageSize, out long totalRecords, IQuery<TContent>? filter, Ordering? ordering = null)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        ordering ??= Ordering.By("sortOrder");

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.GetPage(
                Query<TContent>()?.Where(x => contentTypeIds.Contains(x.ContentTypeId)),
                pageIndex,
                pageSize,
                out totalRecords,
                filter,
                ordering);
        }
    }

    /// <summary>
    ///     Gets a specific version of an <see cref="TContent" /> item.
    /// </summary>
    /// <param name="versionId">Id of the version to retrieve</param>
    /// <returns>An <see cref="TContent" /> item</returns>
    public TContent? GetVersion(int versionId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.GetVersion(versionId);
        }
    }

    /// <summary>
    ///     Gets a collection of an <see cref="TContent" /> objects versions by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>An Enumerable list of <see cref="TContent" /> objects</returns>
    public IEnumerable<TContent> GetVersions(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.GetAllVersions(id);
        }
    }

    /// <summary>
    ///     Gets a collection of an <see cref="TContent" /> objects versions by Id
    /// </summary>
    /// <returns>An Enumerable list of <see cref="TContent" /> objects</returns>
    public IEnumerable<TContent> GetVersionsSlim(int id, int skip, int take)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.GetAllVersionsSlim(id, skip, take);
        }
    }

    /// <summary>
    ///     Gets a list of all version Ids for the given content item ordered so latest is first
    /// </summary>
    /// <param name="id"></param>
    /// <param name="maxRows">The maximum number of rows to return</param>
    /// <returns></returns>
    public IEnumerable<int> GetVersionIds(int id, int maxRows)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _documentRepository.GetVersionIds(id, maxRows);
        }
    }

    /// <inheritdoc />
    public IEnumerable<TContent> GetContentForExpiration(DateTime date)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.GetContentForExpiration(date);
        }
    }

    /// <inheritdoc />
    public IEnumerable<TContent> GetContentForRelease(DateTime date)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentRepository.GetContentForRelease(date);
        }
    }

    #endregion

    #region Save, Publish, Unpublish

    /// <inheritdoc />
    public OperationResult Save(TContent content, int? userId = null, ContentScheduleCollection? contentSchedule = null)
    {
        PublishedState publishedState = content.PublishedState;
        if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
        {
            throw new InvalidOperationException(
                $"Cannot save (un)publishing content with name: {content.Name} - and state: {content.PublishedState}, use the dedicated SavePublished method.");
        }

        if (content.Name != null && content.Name.Length > 255)
        {
            throw new InvalidOperationException(
                $"Content with the name {content.Name} cannot be more than 255 characters in length.");
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            SavingNotification<TContent> savingNotification = SavingNotification(content, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return OperationResult.Cancel(eventMessages);
            }

            scope.WriteLock(Constants.Locks.ContentTree);
            userId ??= Constants.Security.SuperUserId;

            if (content.HasIdentity == false)
            {
                content.CreatorId = userId.Value;
            }

            content.WriterId = userId.Value;

            // track the cultures that have changed
            List<string>? culturesChanging = content.ContentType.VariesByCulture()
                ? content.CultureInfos?.Values.Where(x => x.IsDirty()).Select(x => x.Culture).ToList()
                : null;

            // TODO: Currently there's no way to change track which variant properties have changed, we only have change
            // tracking enabled on all values on the Property which doesn't allow us to know which variants have changed.
            // in this particular case, determining which cultures have changed works with the above with names since it will
            // have always changed if it's been saved in the back office but that's not really fail safe.
            _documentRepository.Save(content);

            if (contentSchedule != null)
            {
                _documentRepository.PersistContentSchedule(content, contentSchedule);
            }

            scope.Notifications.Publish(SavedNotification(content, eventMessages).WithStateFrom(savingNotification));

            // TODO: we had code here to FORCE that this event can never be suppressed. But that just doesn't make a ton of sense?!
            // I understand that if its suppressed that the caches aren't updated, but that would be expected. If someone
            // is supressing events then I think it's expected that nothing will happen. They are probably doing it for perf
            // reasons like bulk import and in those cases we don't want this occuring.
            scope.Notifications.Publish(TreeChangeNotification(content, TreeChangeTypes.RefreshNode, eventMessages));

            if (culturesChanging != null)
            {
                var langs = GetLanguageDetailsForAuditEntry(culturesChanging);
                Audit(AuditType.SaveVariant, userId.Value, content.Id, $"Saved languages: {langs}", langs);
            }
            else
            {
                Audit(AuditType.Save, userId.Value, content.Id);
            }

            scope.Complete();
        }

        return OperationResult.Succeed(eventMessages);
    }

    /// <inheritdoc />
    public OperationResult Save(IEnumerable<TContent> contents, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        TContent[] contentsA = contents.ToArray();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            SavingNotification<TContent> savingNotification = SavingNotification(contentsA, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return OperationResult.Cancel(eventMessages);
            }

            scope.WriteLock(Constants.Locks.ContentTree);
            foreach (TContent content in contentsA)
            {
                if (content.HasIdentity == false)
                {
                    content.CreatorId = userId;
                }

                content.WriterId = userId;

                _documentRepository.Save(content);
            }

            scope.Notifications.Publish(SavedNotification(contentsA, eventMessages).WithStateFrom(savingNotification));

            // TODO: See note above about supressing events
            scope.Notifications.Publish(TreeChangeNotification(contentsA, TreeChangeTypes.RefreshNode, eventMessages));

            string contentIds = string.Join(", ", contentsA.Select(x => x.Id));
            Audit(AuditType.Save, userId, Constants.System.Root, $"Saved multiple content items (#{contentIds.Length})");

            scope.Complete();
        }

        return OperationResult.Succeed(eventMessages);
    }

    /// <inheritdoc/>
    public PublishResult Publish(TContent content, string[] cultures, int userId = Constants.Security.SuperUserId)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (cultures is null)
        {
            throw new ArgumentNullException(nameof(cultures));
        }

        if (cultures.Any(c => c.IsNullOrWhiteSpace()) || cultures.Distinct().Count() != cultures.Length)
        {
            throw new ArgumentException("Cultures cannot be null or whitespace", nameof(cultures));
        }

        cultures = cultures.Select(x => x.EnsureCultureCode()!).ToArray();

        EventMessages evtMsgs = EventMessagesFactory.Get();

        // we need to guard against unsaved changes before proceeding; the content will be saved, but we're not firing any saved notifications
        if (HasUnsavedChanges(content))
        {
            return new PublishResult(PublishResultType.FailedPublishUnsavedChanges, evtMsgs, content);
        }

        if (content.Name != null && content.Name.Length > 255)
        {
            throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
        }

        PublishedState publishedState = content.PublishedState;
        if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
        {
            throw new InvalidOperationException(
                $"Cannot save-and-publish (un)publishing content, use the dedicated {nameof(CommitDocumentChanges)} method.");
        }

        // cannot accept invariant (null or empty) culture for variant content type
        // cannot accept a specific culture for invariant content type (but '*' is ok)
        if (content.ContentType.VariesByCulture())
        {
            if (cultures.Length > 1 && cultures.Contains("*"))
            {
                throw new ArgumentException("Cannot combine wildcard and specific cultures when publishing variant content types.", nameof(cultures));
            }
        }
        else
        {
            if (cultures.Length == 0)
            {
                cultures = new[] { "*" };
            }

            if (cultures[0] != "*" || cultures.Length > 1)
            {
                throw new ArgumentException($"Only wildcard culture is supported when publishing invariant content types.", nameof(cultures));
            }
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            var allLangs = _languageRepository.GetMany().ToList();

            // this will create the correct culture impact even if culture is * or null
            IEnumerable<CultureImpact?> impacts =
                cultures.Select(culture => _cultureImpactFactory.Create(culture, IsDefaultCulture(allLangs, culture), content));

            // publish the culture(s)
            // we don't care about the response here, this response will be rechecked below but we need to set the culture info values now.
            var publishTime = DateTime.Now;
            foreach (CultureImpact? impact in impacts)
            {
                content.PublishCulture(impact, publishTime, _propertyEditorCollection);
            }

            // Change state to publishing
            content.PublishedState = PublishedState.Publishing;

            PublishResult result = CommitDocumentChanges(scope, content, evtMsgs, allLangs, new Dictionary<string, object?>(), userId);
            scope.Complete();
            return result;
        }
    }

    /// <inheritdoc />
    public PublishResult Unpublish(TContent content, string? culture = "*", int userId = Constants.Security.SuperUserId)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        culture = culture?.NullOrWhiteSpaceAsNull().EnsureCultureCode();

        PublishedState publishedState = content.PublishedState;
        if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
        {
            throw new InvalidOperationException(
                $"Cannot save-and-publish (un)publishing content, use the dedicated {nameof(CommitDocumentChanges)} method.");
        }

        // cannot accept invariant (null or empty) culture for variant content type
        // cannot accept a specific culture for invariant content type (but '*' is ok)
        if (content.ContentType.VariesByCulture())
        {
            if (culture == null)
            {
                throw new NotSupportedException("Invariant culture is not supported by variant content types.");
            }
        }
        else
        {
            if (culture != null && culture != "*")
            {
                throw new NotSupportedException(
                    $"Culture \"{culture}\" is not supported by invariant content types.");
            }
        }

        // if the content is not published, nothing to do
        if (!content.Published)
        {
            return new PublishResult(PublishResultType.SuccessUnpublishAlready, evtMsgs, content);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            var allLangs = _languageRepository.GetMany().ToList();

            SavingNotification<TContent> savingNotification = SavingNotification(content, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, content);
            }

            // all cultures = unpublish whole
            if (culture == "*" || (!content.ContentType.VariesByCulture() && culture == null))
            {
                // Unpublish the culture, this will change the document state to Publishing! ... which is expected because this will
                // essentially be re-publishing the document with the requested culture removed
                // We are however unpublishing all cultures, so we will set this to unpublishing.
                content.UnpublishCulture(culture);
                content.PublishedState = PublishedState.Unpublishing;
                PublishResult result = CommitDocumentChanges(scope, content, evtMsgs, allLangs, savingNotification.State, userId);
                scope.Complete();
                return result;
            }
            else
            {
                // Unpublish the culture, this will change the document state to Publishing! ... which is expected because this will
                // essentially be re-publishing the document with the requested culture removed.
                // The call to CommitDocumentChangesInternal will perform all the checks like if this is a mandatory culture or the last culture being unpublished
                // and will then unpublish the document accordingly.
                // If the result of this is false it means there was no culture to unpublish (i.e. it was already unpublished or it did not exist)
                var removed = content.UnpublishCulture(culture);

                // Save and publish any changes
                PublishResult result = CommitDocumentChanges(scope, content, evtMsgs, allLangs, savingNotification.State, userId);

                scope.Complete();

                // In one case the result will be PublishStatusType.FailedPublishNothingToPublish which means that no cultures
                // were specified to be published which will be the case when removed is false. In that case
                // we want to swap the result type to PublishResultType.SuccessUnpublishAlready (that was the expectation before).
                if (result.Result == PublishResultType.FailedPublishNothingToPublish && !removed)
                {
                    return new PublishResult(PublishResultType.SuccessUnpublishAlready, evtMsgs, content);
                }

                return result;
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<PublishResult> PerformScheduledPublish(DateTime date)
    {
        var allLangs = new Lazy<List<ILanguage>>(() => _languageRepository.GetMany().ToList());
        EventMessages evtMsgs = EventMessagesFactory.Get();
        var results = new List<PublishResult>();

        PerformScheduledPublishingRelease(date, results, evtMsgs, allLangs);
        PerformScheduledPublishingExpiration(date, results, evtMsgs, allLangs);

        return results;
    }

    private void PerformScheduledPublishingExpiration(DateTime date, List<PublishResult> results, EventMessages evtMsgs, Lazy<List<ILanguage>> allLangs)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // do a fast read without any locks since this executes often to see if we even need to proceed
        if (_documentRepository.HasContentForExpiration(date))
        {
            // now take a write lock since we'll be updating
            scope.WriteLock(Constants.Locks.ContentTree);

            foreach (TContent d in _documentRepository.GetContentForExpiration(date))
            {
                ContentScheduleCollection contentSchedule = _documentRepository.GetContentSchedule(d.Id);
                if (d.ContentType.VariesByCulture())
                {
                    // find which cultures have pending schedules
                    var pendingCultures = contentSchedule.GetPending(ContentScheduleAction.Expire, date)
                        .Select(x => x.Culture)
                        .Distinct()
                        .ToList();

                    if (pendingCultures.Count == 0)
                    {
                        continue; // shouldn't happen but no point in processing this document if there's nothing there
                    }

                    SavingNotification<TContent> savingNotification = SavingNotification(d, evtMsgs);
                    if (scope.Notifications.PublishCancelable(savingNotification))
                    {
                        results.Add(new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, d));
                        continue;
                    }

                    foreach (var c in pendingCultures)
                    {
                        // Clear this schedule for this culture
                        contentSchedule.Clear(c, ContentScheduleAction.Expire, date);

                        // set the culture to be published
                        d.UnpublishCulture(c);
                    }

                    _documentRepository.PersistContentSchedule(d, contentSchedule);
                    PublishResult result = CommitDocumentChanges(scope, d, evtMsgs, allLangs.Value, savingNotification.State, d.WriterId);
                    if (result.Success == false)
                    {
                        Logger.LogError(null, "Failed to publish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);
                    }

                    results.Add(result);
                }
                else
                {
                    // Clear this schedule for this culture
                    contentSchedule.Clear(ContentScheduleAction.Expire, date);
                    _documentRepository.PersistContentSchedule(d, contentSchedule);
                    PublishResult result = Unpublish(d, userId: d.WriterId);
                    if (result.Success == false)
                    {
                        Logger.LogError(null, "Failed to unpublish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);
                    }

                    results.Add(result);
                }
            }

            _documentRepository.ClearSchedule(date, ContentScheduleAction.Expire);
        }

        scope.Complete();
    }

    private void PerformScheduledPublishingRelease(DateTime date, List<PublishResult> results, EventMessages evtMsgs, Lazy<List<ILanguage>> allLangs)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // do a fast read without any locks since this executes often to see if we even need to proceed
        if (_documentRepository.HasContentForRelease(date))
        {
            // now take a write lock since we'll be updating
            scope.WriteLock(Constants.Locks.ContentTree);

            foreach (TContent d in _documentRepository.GetContentForRelease(date))
            {
                ContentScheduleCollection contentSchedule = _documentRepository.GetContentSchedule(d.Id);
                if (d.ContentType.VariesByCulture())
                {
                    // find which cultures have pending schedules
                    var pendingCultures = contentSchedule.GetPending(ContentScheduleAction.Release, date)
                        .Select(x => x.Culture)
                        .Distinct()
                        .ToList();

                    if (pendingCultures.Count == 0)
                    {
                        continue; // shouldn't happen but no point in processing this document if there's nothing there
                    }
                    SavingNotification<TContent> savingNotification = SavingNotification(d, evtMsgs);
                    if (scope.Notifications.PublishCancelable(savingNotification))
                    {
                        results.Add(new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, d));
                        continue;
                    }


                    var publishing = true;
                    foreach (var culture in pendingCultures)
                    {
                        // Clear this schedule for this culture
                        contentSchedule.Clear(culture, ContentScheduleAction.Release, date);

                        if (d.Trashed)
                        {
                            continue; // won't publish
                        }

                        // publish the culture values and validate the property values, if validation fails, log the invalid properties so the develeper has an idea of what has failed
                        IProperty[]? invalidProperties = null;
                        CultureImpact impact = _cultureImpactFactory.ImpactExplicit(culture, IsDefaultCulture(allLangs.Value, culture));
                        var tryPublish = d.PublishCulture(impact, date, _propertyEditorCollection) &&
                                         _propertyValidationService.Value.IsPropertyDataValid(d, out invalidProperties, impact);
                        if (invalidProperties != null && invalidProperties.Length > 0)
                        {
                            Logger.LogWarning(
                                "Scheduled publishing will fail for document {DocumentId} and culture {Culture} because of invalid properties {InvalidProperties}",
                                d.Id,
                                culture,
                                string.Join(",", invalidProperties.Select(x => x.Alias)));
                        }

                        publishing &= tryPublish; // set the culture to be published
                        if (!publishing)
                        {
                        }
                    }

                    PublishResult result;

                    if (d.Trashed)
                    {
                        result = new PublishResult(PublishResultType.FailedPublishIsTrashed, evtMsgs, d);
                    }
                    else if (!publishing)
                    {
                        result = new PublishResult(PublishResultType.FailedPublishContentInvalid, evtMsgs, d);
                    }
                    else
                    {
                        _documentRepository.PersistContentSchedule(d, contentSchedule);
                        result = CommitDocumentChanges(scope, d, evtMsgs, allLangs.Value, savingNotification.State, d.WriterId);
                    }

                    if (result.Success == false)
                    {
                        Logger.LogError(null, "Failed to publish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);
                    }

                    results.Add(result);
                }
                else
                {
                    // Clear this schedule
                    contentSchedule.Clear(ContentScheduleAction.Release, date);

                    PublishResult? result = null;

                    if (d.Trashed)
                    {
                        result = new PublishResult(PublishResultType.FailedPublishIsTrashed, evtMsgs, d);
                    }
                    else
                    {
                        _documentRepository.PersistContentSchedule(d, contentSchedule);
                        result = Publish(d, d.AvailableCultures.ToArray(), userId: d.WriterId);
                    }

                    if (result.Success == false)
                    {
                        Logger.LogError(null, "Failed to publish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);
                    }

                    results.Add(result);
                }
            }

            _documentRepository.ClearSchedule(date, ContentScheduleAction.Release);
        }

        scope.Complete();
    }

    #endregion

    #region Delete

    /// <inheritdoc />
    public OperationResult Delete(TContent content, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            if (scope.Notifications.PublishCancelable(DeletingNotification(content, eventMessages)))
            {
                scope.Complete();
                return OperationResult.Cancel(eventMessages);
            }

            scope.WriteLock(Constants.Locks.ContentTree);

            // if it's not trashed yet, and published, we should unpublish
            // but... Unpublishing event makes no sense (not going to cancel?) and no need to save
            // just raise the event
            if (content.Trashed == false && content.Published)
            {
                scope.Notifications.Publish(UnpublishedNotification(content, eventMessages));
            }

            DeleteLocked(scope, content, eventMessages);

            scope.Notifications.Publish(TreeChangeNotification(content, TreeChangeTypes.Remove, eventMessages));
            Audit(AuditType.Delete, userId, content.Id);

            scope.Complete();
        }

        return OperationResult.Succeed(eventMessages);
    }

    // TODO: both DeleteVersions methods below have an issue. Sort of. They do NOT take care of files the way
    // Delete does - for a good reason: the file may be referenced by other, non-deleted, versions. BUT,
    // if that's not the case, then the file will never be deleted, because when we delete the content,
    // the version referencing the file will not be there anymore. SO, we can leak files.

    /// <summary>
    ///     Permanently deletes versions from an <see cref="TContent" /> object prior to a specific date.
    ///     This method will never delete the latest version of a content item.
    /// </summary>
    /// <param name="id">Id of the <see cref="TContent" /> object to delete versions from</param>
    /// <param name="versionDate">Latest version date</param>
    /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
    public void DeleteVersions(int id, DateTime versionDate, int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var deletingVersionsNotification =
                new ContentDeletingVersionsNotification(id, evtMsgs, dateToRetain: versionDate);
            if (scope.Notifications.PublishCancelable(deletingVersionsNotification))
            {
                scope.Complete();
                return;
            }

            scope.WriteLock(Constants.Locks.ContentTree);
            _documentRepository.DeleteVersions(id, versionDate);

            scope.Notifications.Publish(
                new ContentDeletedVersionsNotification(id, evtMsgs, dateToRetain: versionDate).WithStateFrom(
                    deletingVersionsNotification));
            Audit(AuditType.Delete, userId, Constants.System.Root, "Delete (by version date)");

            scope.Complete();
        }
    }

    /// <summary>
    ///     Permanently deletes specific version(s) from an <see cref="TContent" /> object.
    ///     This method will never delete the latest version of a content item.
    /// </summary>
    /// <param name="id">Id of the <see cref="TContent" /> object to delete a version from</param>
    /// <param name="versionId">Id of the version to delete</param>
    /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
    /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
    public void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var deletingVersionsNotification = new ContentDeletingVersionsNotification(id, evtMsgs, versionId);
            if (scope.Notifications.PublishCancelable(deletingVersionsNotification))
            {
                scope.Complete();
                return;
            }

            if (deletePriorVersions)
            {
                TContent? content = GetVersion(versionId);
                DeleteVersions(id, content?.UpdateDate ?? DateTime.Now, userId);
            }

            scope.WriteLock(Constants.Locks.ContentTree);
            TContent? c = _documentRepository.Get(id);

            // don't delete the current or published version
            if (c?.VersionId != versionId &&
                c?.PublishedVersionId != versionId)
            {
                _documentRepository.DeleteVersion(versionId);
            }

            scope.Notifications.Publish(
                new ContentDeletedVersionsNotification(id, evtMsgs, versionId).WithStateFrom(
                    deletingVersionsNotification));
            Audit(AuditType.Delete, userId, Constants.System.Root, "Delete (by version)");

            scope.Complete();
        }
    }

    #endregion

    #region Others

    protected static bool HasUnsavedChanges(TContent content) => content.HasIdentity is false || content.IsDirty();

    protected static bool IsDefaultCulture(IReadOnlyCollection<ILanguage>? langs, string culture) =>
        langs?.Any(x => x.IsDefault && x.IsoCode.InvariantEquals(culture)) ?? false;

    #endregion

    #region Auditing

    protected void Audit(AuditType type, int userId, int objectId, string? message = null, string? parameters = null) =>
        _auditRepository.Save(new AuditItem(objectId, type, userId, UmbracoObjectTypes.Document.GetName(), message, parameters));


    protected string GetLanguageDetailsForAuditEntry(IEnumerable<string> affectedCultures)
        => GetLanguageDetailsForAuditEntry(_languageRepository.GetMany(), affectedCultures);

    protected static string GetLanguageDetailsForAuditEntry(IEnumerable<ILanguage> languages, IEnumerable<string> affectedCultures)
    {
        IEnumerable<string> languageIsoCodes = languages
            .Where(x => affectedCultures.InvariantContains(x.IsoCode))
            .Select(x => x.IsoCode);
        return string.Join(", ", languageIsoCodes);
    }

    #endregion

    #region Content Types

    private IContentType GetContentType(ICoreScope scope, string contentTypeAlias)
    {
        if (contentTypeAlias == null)
        {
            throw new ArgumentNullException(nameof(contentTypeAlias));
        }

        if (string.IsNullOrWhiteSpace(contentTypeAlias))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(contentTypeAlias));
        }

        scope.ReadLock(Constants.Locks.ContentTypes);

        IQuery<IContentType> query = Query<IContentType>().Where(x => x.Alias == contentTypeAlias);
        IContentType? contentType = _contentTypeRepository.Get(query).FirstOrDefault()
                                    ??
                                    // causes rollback
                                    throw new Exception($"No ContentType matching the passed in Alias: '{contentTypeAlias}'" +
                                                        $" was found");

        return contentType;
    }

    protected IContentType GetContentType(string contentTypeAlias)
    {
        if (contentTypeAlias == null)
        {
            throw new ArgumentNullException(nameof(contentTypeAlias));
        }

        if (string.IsNullOrWhiteSpace(contentTypeAlias))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(contentTypeAlias));
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return GetContentType(scope, contentTypeAlias);
        }
    }

    #endregion
}
