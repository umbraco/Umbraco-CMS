using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
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
// TODO ELEMENTS: everything structural (children, ancestors, descendants, branches, sort) should be omitted from this base
// TODO ELEMENTS: implement recycle bin
// TODO ELEMENTS: implement copy and move
// TODO ELEMENTS: replace all "document" with "content" (variables, names and comments)
// TODO ELEMENTS: ensure all read and write locks use the abstract lock IDs (ReadLockIds, WriteLockIds)
// TODO ELEMENTS: rename _documentRepository to _contentRepository
public abstract class PublishableContentServiceBase<TContent> : RepositoryService
    where TContent : class, IPublishableContentBase
{
    private readonly IAuditService _auditService;
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly IPublishableContentRepository<TContent> _documentRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly Lazy<IPropertyValidationService> _propertyValidationService;
    private readonly ICultureImpactFactory _cultureImpactFactory;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IIdKeyMap _idKeyMap;

    protected PublishableContentServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        IContentTypeRepository contentTypeRepository,
        IPublishableContentRepository<TContent> contentRepository,
        ILanguageRepository languageRepository,
        Lazy<IPropertyValidationService> propertyValidationService,
        ICultureImpactFactory cultureImpactFactory,
        IUserIdKeyResolver userIdKeyResolver,
        PropertyEditorCollection propertyEditorCollection,
        IIdKeyMap idKeyMap)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _auditService = auditService;
        _contentTypeRepository = contentTypeRepository;
        _documentRepository = contentRepository;
        _languageRepository = languageRepository;
        _propertyValidationService = propertyValidationService;
        _cultureImpactFactory = cultureImpactFactory;
        _userIdKeyResolver = userIdKeyResolver;
        _propertyEditorCollection = propertyEditorCollection;
        _idKeyMap = idKeyMap;
    }

    protected abstract UmbracoObjectTypes ContentObjectType { get; }

    protected abstract int[] ReadLockIds { get; }

    protected abstract int[] WriteLockIds { get; }

    protected abstract bool SupportsBranchPublishing { get; }

    protected abstract ILogger<PublishableContentServiceBase<TContent>> Logger { get; }

    protected abstract TContent CreateContentInstance(string name, int parentId, IContentType contentType, int userId);

    protected abstract TContent CreateContentInstance(string name, TContent parent, IContentType contentType, int userId);

    protected virtual PublishResult CommitDocumentChanges(
        ICoreScope scope,
        TContent content,
        EventMessages eventMessages,
        IReadOnlyCollection<ILanguage> allLangs,
        IDictionary<string, object?>? notificationState,
        int userId)
        => CommitDocumentChangesInternal(scope, content, eventMessages, allLangs, notificationState, userId);

    protected abstract void DeleteLocked(ICoreScope scope, TContent content, EventMessages evtMsgs);

    protected abstract SavingNotification<TContent> SavingNotification(TContent content, EventMessages eventMessages);

    protected abstract SavedNotification<TContent> SavedNotification(TContent content, EventMessages eventMessages);

    protected abstract SavingNotification<TContent> SavingNotification(IEnumerable<TContent> content, EventMessages eventMessages);

    protected abstract SavedNotification<TContent> SavedNotification(IEnumerable<TContent> content, EventMessages eventMessages);

    protected abstract TreeChangeNotification<TContent> TreeChangeNotification(TContent content, TreeChangeTypes changeTypes, EventMessages eventMessages);

    protected abstract TreeChangeNotification<TContent> TreeChangeNotification(TContent content, TreeChangeTypes changeTypes, IEnumerable<string>? publishedCultures, IEnumerable<string>? unpublishedCultures, EventMessages eventMessages);

    protected abstract TreeChangeNotification<TContent> TreeChangeNotification(IEnumerable<TContent> content, TreeChangeTypes changeTypes, EventMessages eventMessages);

    protected abstract DeletingNotification<TContent> DeletingNotification(TContent content, EventMessages eventMessages);

    // TODO ELEMENTS: create a base class for publishing notifications to reuse between IContent and IElement
    protected abstract CancelableEnumerableObjectNotification<TContent> PublishingNotification(TContent content, EventMessages eventMessages);

    protected abstract IStatefulNotification PublishedNotification(TContent content, EventMessages eventMessages);

    protected abstract IStatefulNotification PublishedNotification(IEnumerable<TContent> content, EventMessages eventMessages);

    protected abstract CancelableEnumerableObjectNotification<TContent> UnpublishingNotification(TContent content, EventMessages eventMessages);

    protected abstract IStatefulNotification UnpublishedNotification(TContent content, EventMessages eventMessages);

    protected abstract RollingBackNotification<TContent> RollingBackNotification(TContent target, EventMessages messages);

    protected abstract RolledBackNotification<TContent> RolledBackNotification(TContent target, EventMessages messages);

    #region Rollback

    public OperationResult Rollback(int id, int versionId, string culture = "*", int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        // Get the current copy of the node
        TContent? content = GetById(id);

        // Get the version
        TContent? version = GetVersion(versionId);

        // Good old null checks
        if (content == null || version == null || content.Trashed)
        {
            return new OperationResult(OperationResultType.FailedCannot, evtMsgs);
        }

        // Store the result of doing the save of content for the rollback
        OperationResult rollbackSaveResult;

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var rollingBackNotification = RollingBackNotification(content, evtMsgs);
            if (scope.Notifications.PublishCancelable(rollingBackNotification))
            {
                scope.Complete();
                return OperationResult.Cancel(evtMsgs);
            }

            // Copy the changes from the version
            content.CopyFrom(version, culture);

            // Save the content for the rollback
            rollbackSaveResult = Save(content, userId);

            // Depending on the save result - is what we log & audit along with what we return
            if (rollbackSaveResult.Success == false)
            {
                // Log the error/warning
                Logger.LogError(
                    "User '{UserId}' was unable to rollback content '{ContentId}' to version '{VersionId}'", userId, id, versionId);
            }
            else
            {
                scope.Notifications.Publish(RolledBackNotification(content, evtMsgs).WithStateFrom(rollingBackNotification));

                // Logging & Audit message
                Logger.LogInformation("User '{UserId}' rolled back content '{ContentId}' to version '{VersionId}'", userId, id, versionId);
                Audit(AuditType.RollBack, userId, id, $"Content '{content.Name}' was rolled back to version '{versionId}'");
            }

            scope.Complete();
        }

        return rollbackSaveResult;
    }

    #endregion

    #region Count

    public int CountPublished(string? contentTypeAlias = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(ReadLockIds);
            return _documentRepository.CountPublished(contentTypeAlias);
        }
    }

    public int Count(string? contentTypeAlias = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(ReadLockIds);
            return _documentRepository.Count(contentTypeAlias);
        }
    }

    public int CountChildren(int parentId, string? contentTypeAlias = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(ReadLockIds);
            return _documentRepository.CountChildren(parentId, contentTypeAlias);
        }
    }

    public int CountDescendants(int parentId, string? contentTypeAlias = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(ReadLockIds);
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
            scope.ReadLock(ReadLockIds);
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
            scope.ReadLock(ReadLockIds);
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
            scope.ReadLock(ReadLockIds);
            return _documentRepository.Get(key);
        }
    }

    /// <inheritdoc />
    public ContentScheduleCollection GetContentScheduleByContentId(int contentId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(ReadLockIds);
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
    public void PersistContentSchedule(IPublishableContentBase content, ContentScheduleCollection contentSchedule)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(WriteLockIds);
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
            scope.ReadLock(ReadLockIds);
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
            scope.ReadLock(ReadLockIds);
            return _documentRepository.GetPage(
                Query<TContent>()?.Where(x => x.ContentTypeId == contentTypeId),
                pageIndex,
                pageSize,
                out totalRecords,
                null,
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
            // Need to use a List here because the expression tree cannot convert the array when used in Contains.
            // See ExpressionTests.Sql_In().
            List<int> contentTypeIdsAsList = [.. contentTypeIds];

            scope.ReadLock(ReadLockIds);
            return _documentRepository.GetPage(
                Query<TContent>()?.Where(x => contentTypeIdsAsList.Contains(x.ContentTypeId)),
                pageIndex,
                pageSize,
                out totalRecords,
                null,
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
            scope.ReadLock(ReadLockIds);
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
            scope.ReadLock(ReadLockIds);
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
            scope.ReadLock(ReadLockIds);
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
            scope.ReadLock(ReadLockIds);
            return _documentRepository.GetContentForExpiration(date);
        }
    }

    /// <inheritdoc />
    public IEnumerable<TContent> GetContentForRelease(DateTime date)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(ReadLockIds);
            return _documentRepository.GetContentForRelease(date);
        }
    }

    /// <summary>
    ///     Checks whether an <see cref="IContent" /> item has any children
    /// </summary>
    /// <param name="id">Id of the <see cref="IContent" /></param>
    /// <returns>True if the content has any children otherwise False</returns>
    public bool HasChildren(int id) => CountChildren(id) > 0;

    public bool IsPathPublished(TContent? content)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(ReadLockIds);
            return _documentRepository.IsPathPublished(content);
        }
    }

    /// <summary>
    ///     Gets the parent of the current content as an <see cref="TContent" /> item.
    /// </summary>
    /// <param name="content"><see cref="TContent" /> to retrieve the parent from</param>
    /// <returns>Parent <see cref="TContent" /> object</returns>
    public TContent? GetParent(TContent? content)
    {
        if (content?.ParentId == Constants.System.Root || content?.ParentId == Constants.System.RecycleBinContent ||
            content is null)
        {
            return null;
        }

        return GetById(content.ParentId);
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

            scope.WriteLock(WriteLockIds);
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

            scope.WriteLock(WriteLockIds);
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
            scope.WriteLock(WriteLockIds);

            var allLangs = _languageRepository.GetMany().ToList();

            // this will create the correct culture impact even if culture is * or null
            IEnumerable<CultureImpact?> impacts =
                cultures.Select(culture => _cultureImpactFactory.Create(culture, IsDefaultCulture(allLangs, culture), content));

            // publish the culture(s)
            // we don't care about the response here, this response will be rechecked below but we need to set the culture info values now.
            var publishTime = DateTime.UtcNow;
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
            scope.WriteLock(WriteLockIds);

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
            scope.WriteLock(WriteLockIds);

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
            scope.WriteLock(WriteLockIds);

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

    /// <summary>
    ///     Handles a lot of business logic cases for how the document should be persisted
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="content"></param>
    /// <param name="allLangs"></param>
    /// <param name="notificationState"></param>
    /// <param name="userId"></param>
    /// <param name="branchOne"></param>
    /// <param name="branchRoot"></param>
    /// <param name="eventMessages"></param>
    /// <returns></returns>
    /// <remarks>
    ///     <para>
    ///         Business logic cases such: as unpublishing a mandatory culture, or unpublishing the last culture, checking for
    ///         pending scheduled publishing, etc... is dealt with in this method.
    ///         There is quite a lot of cases to take into account along with logic that needs to deal with scheduled
    ///         saving/publishing, branch saving/publishing, etc...
    ///     </para>
    /// </remarks>
    protected PublishResult CommitDocumentChangesInternal(
        ICoreScope scope,
        TContent content,
        EventMessages eventMessages,
        IReadOnlyCollection<ILanguage> allLangs,
        IDictionary<string, object?>? notificationState,
        int userId,
        bool branchOne = false,
        bool branchRoot = false)
    {
        if (scope == null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (eventMessages == null)
        {
            throw new ArgumentNullException(nameof(eventMessages));
        }

        PublishResult? publishResult = null;
        PublishResult? unpublishResult = null;

        // nothing set = republish it all
        if (content.PublishedState != PublishedState.Publishing &&
            content.PublishedState != PublishedState.Unpublishing)
        {
            content.PublishedState = PublishedState.Publishing;
        }

        // State here is either Publishing or Unpublishing
        // Publishing to unpublish a culture may end up unpublishing everything so these flags can be flipped later
        var publishing = content.PublishedState == PublishedState.Publishing;
        var unpublishing = content.PublishedState == PublishedState.Unpublishing;

        var variesByCulture = content.ContentType.VariesByCulture();

        // Track cultures that are being published, changed, unpublished
        IReadOnlyList<string>? culturesPublishing = null;
        IReadOnlyList<string>? culturesUnpublishing = null;
        IReadOnlyList<string>? culturesChanging = variesByCulture
            ? content.CultureInfos?.Values.Where(x => x.IsDirty()).Select(x => x.Culture).ToList()
            : null;

        var isNew = !content.HasIdentity;
        TreeChangeTypes changeType = isNew || SupportsBranchPublishing is false ? TreeChangeTypes.RefreshNode : TreeChangeTypes.RefreshBranch;
        var previouslyPublished = content.HasIdentity && content.Published;

        // Inline method to persist the document with the documentRepository since this logic could be called a couple times below
        void SaveDocument(TContent c)
        {
            // save, always
            if (c.HasIdentity == false)
            {
                c.CreatorId = userId;
            }

            c.WriterId = userId;

            // saving does NOT change the published version, unless PublishedState is Publishing or Unpublishing
            _documentRepository.Save(c);
        }

        if (publishing)
        {
            // Determine cultures publishing/unpublishing which will be based on previous calls to content.PublishCulture and ClearPublishInfo
            culturesUnpublishing = content.GetCulturesUnpublishing();
            culturesPublishing = variesByCulture
                ? content.PublishCultureInfos?.Values.Where(x => x.IsDirty()).Select(x => x.Culture).ToList()
                : null;

            // ensure that the document can be published, and publish handling events, business rules, etc
            publishResult = StrategyCanPublish(
                scope,
                content, /*checkPath:*/
                !branchOne || branchRoot,
                culturesPublishing,
                culturesUnpublishing,
                eventMessages,
                allLangs,
                notificationState);

            if (publishResult.Success)
            {
                // raise Publishing notification
                if (scope.Notifications.PublishCancelable(
                        PublishingNotification(content, eventMessages).WithState(notificationState)))
                {
                    Logger.LogInformation("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "publishing was cancelled");
                    return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, eventMessages, content);
                }

                // note: StrategyPublish flips the PublishedState to Publishing!
                publishResult = StrategyPublish(content, culturesPublishing, culturesUnpublishing, eventMessages);

                // Check if a culture has been unpublished and if there are no cultures left, and then unpublish document as a whole
                if (publishResult.Result == PublishResultType.SuccessUnpublishCulture &&
                    content.PublishCultureInfos?.Count == 0)
                {
                    // This is a special case! We are unpublishing the last culture and to persist that we need to re-publish without any cultures
                    // so the state needs to remain Publishing to do that. However, we then also need to unpublish the document and to do that
                    // the state needs to be Unpublishing and it cannot be both. This state is used within the documentRepository to know how to
                    // persist certain things. So before proceeding below, we need to save the Publishing state to publish no cultures, then we can
                    // mark the document for Unpublishing.
                    SaveDocument(content);

                    // Set the flag to unpublish and continue
                    unpublishing = content.Published; // if not published yet, nothing to do
                }
            }
            else
            {
                // in a branch, just give up
                if (branchOne && !branchRoot)
                {
                    return publishResult;
                }

                // Check for mandatory culture missing, and then unpublish document as a whole
                if (publishResult.Result == PublishResultType.FailedPublishMandatoryCultureMissing)
                {
                    publishing = false;
                    unpublishing = content.Published; // if not published yet, nothing to do

                    // we may end up in a state where we won't publish nor unpublish
                    // keep going, though, as we want to save anyways
                }

                // reset published state from temp values (publishing, unpublishing) to original value
                // (published, unpublished) in order to save the document, unchanged - yes, this is odd,
                // but: (a) it means we don't reproduce the PublishState logic here and (b) setting the
                // PublishState to anything other than Publishing or Unpublishing - which is precisely
                // what we want to do here - throws
                content.Published = content.Published;
            }
        }

        // won't happen in a branch
        if (unpublishing)
        {
            TContent? newest = GetById(content.Id); // ensure we have the newest version - in scope
            if (content.VersionId != newest?.VersionId)
            {
                return new PublishResult(PublishResultType.FailedPublishConcurrencyViolation, eventMessages, content);
            }

            if (content.Published)
            {
                // ensure that the document can be unpublished, and unpublish
                // handling events, business rules, etc
                // note: StrategyUnpublish flips the PublishedState to Unpublishing!
                // note: This unpublishes the entire document (not different variants)
                unpublishResult = StrategyCanUnpublish(scope, content, eventMessages, notificationState);
                if (unpublishResult.Success)
                {
                    unpublishResult = StrategyUnpublish(content, eventMessages);
                }
                else
                {
                    // reset published state from temp values (publishing, unpublishing) to original value
                    // (published, unpublished) in order to save the document, unchanged - yes, this is odd,
                    // but: (a) it means we don't reproduce the PublishState logic here and (b) setting the
                    // PublishState to anything other than Publishing or Unpublishing - which is precisely
                    // what we want to do here - throws
                    content.Published = content.Published;
                    return unpublishResult;
                }
            }
            else
            {
                // already unpublished - optimistic concurrency collision, really,
                // and I am not sure at all what we should do, better die fast, else
                // we may end up corrupting the db
                throw new InvalidOperationException("Concurrency collision.");
            }
        }

        // Persist the document
        SaveDocument(content);

        // we have tried to unpublish - won't happen in a branch
        if (unpublishing)
        {
            // and succeeded, trigger events
            if (unpublishResult?.Success ?? false)
            {
                // events and audit
                scope.Notifications.Publish(UnpublishedNotification(content, eventMessages).WithState(notificationState));
                scope.Notifications.Publish(TreeChangeNotification(
                    content,
                    SupportsBranchPublishing ? TreeChangeTypes.RefreshBranch : TreeChangeTypes.RefreshNode,
                    variesByCulture ? culturesPublishing.IsCollectionEmpty() ? null : culturesPublishing : null,
                    variesByCulture ? culturesUnpublishing.IsCollectionEmpty() ? null : culturesUnpublishing : ["*"],
                    eventMessages));

                if (culturesUnpublishing != null)
                {
                    // This will mean that that we unpublished a mandatory culture or we unpublished the last culture.
                    var langs = GetLanguageDetailsForAuditEntry(allLangs, culturesUnpublishing);
                    Audit(AuditType.UnpublishVariant, userId, content.Id, $"Unpublished languages: {langs}", langs);

                    if (publishResult == null)
                    {
                        throw new PanicException("publishResult == null - should not happen");
                    }

                    switch (publishResult.Result)
                    {
                        case PublishResultType.FailedPublishMandatoryCultureMissing:
                            // Occurs when a mandatory culture was unpublished (which means we tried publishing the document without a mandatory culture)

                            // Log that the whole content item has been unpublished due to mandatory culture unpublished
                            Audit(AuditType.Unpublish, userId, content.Id, "Unpublished (mandatory language unpublished)");
                            return new PublishResult(PublishResultType.SuccessUnpublishMandatoryCulture, eventMessages, content);
                        case PublishResultType.SuccessUnpublishCulture:
                            // Occurs when the last culture is unpublished
                            Audit(AuditType.Unpublish, userId, content.Id, "Unpublished (last language unpublished)");
                            return new PublishResult(PublishResultType.SuccessUnpublishLastCulture, eventMessages, content);
                    }
                }

                Audit(AuditType.Unpublish, userId, content.Id);
                return new PublishResult(PublishResultType.SuccessUnpublish, eventMessages, content);
            }

            // or, failed
            scope.Notifications.Publish(TreeChangeNotification(content, changeType, eventMessages));
            return new PublishResult(PublishResultType.FailedUnpublish, eventMessages, content); // bah
        }

        // we have tried to publish
        if (publishing)
        {
            // and succeeded, trigger events
            if (publishResult?.Success ?? false)
            {
                if (isNew == false && previouslyPublished == false && SupportsBranchPublishing)
                {
                    changeType = TreeChangeTypes.RefreshBranch; // whole branch
                }
                else if (isNew == false && previouslyPublished)
                {
                    changeType = TreeChangeTypes.RefreshNode; // single node
                }

                // invalidate the node/branch
                // for branches, handled by SaveAndPublishBranch
                if (!branchOne)
                {
                    scope.Notifications.Publish(
                        TreeChangeNotification(
                            content,
                            changeType,
                            variesByCulture ? culturesPublishing.IsCollectionEmpty() ? null : culturesPublishing : ["*"],
                            variesByCulture ? culturesUnpublishing.IsCollectionEmpty() ? null : culturesUnpublishing : null,
                            eventMessages));
                    scope.Notifications.Publish(
                        PublishedNotification(content, eventMessages).WithState(notificationState));
                }

                // it was not published and now is... descendants that were 'published' (but
                // had an unpublished ancestor) are 're-published' ie not explicitly published
                // but back as 'published' nevertheless
                if (!branchOne && isNew == false && previouslyPublished == false && HasChildren(content.Id))
                {
                    TContent[] descendants = GetPublishedDescendantsLocked(content).ToArray();
                    scope.Notifications.Publish(
                        PublishedNotification(descendants, eventMessages).WithState(notificationState));
                }

                switch (publishResult.Result)
                {
                    case PublishResultType.SuccessPublish:
                        Audit(AuditType.Publish, userId, content.Id);
                        break;
                    case PublishResultType.SuccessPublishCulture:
                        if (culturesPublishing != null)
                        {
                            var langs = GetLanguageDetailsForAuditEntry(allLangs, culturesPublishing);
                            Audit(AuditType.PublishVariant, userId, content.Id, $"Published languages: {langs}", langs);
                        }

                        break;
                    case PublishResultType.SuccessUnpublishCulture:
                        if (culturesUnpublishing != null)
                        {
                            var langs = GetLanguageDetailsForAuditEntry(allLangs, culturesUnpublishing);
                            Audit(AuditType.UnpublishVariant, userId, content.Id, $"Unpublished languages: {langs}", langs);
                        }

                        break;
                }

                return publishResult;
            }
        }

        // should not happen
        if (branchOne && !branchRoot)
        {
            throw new PanicException("branchOne && !branchRoot - should not happen");
        }

        // if publishing didn't happen or if it has failed, we still need to log which cultures were saved
        if (!branchOne && (publishResult == null || !publishResult.Success))
        {
            if (culturesChanging != null)
            {
                var langs = GetLanguageDetailsForAuditEntry(allLangs, culturesChanging);
                Audit(AuditType.SaveVariant, userId, content.Id, $"Saved languages: {langs}", langs);
            }
            else
            {
                Audit(AuditType.Save, userId, content.Id);
            }
        }

        // or, failed
        scope.Notifications.Publish(TreeChangeNotification(content, changeType, eventMessages));
        return publishResult!;
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

            scope.WriteLock(WriteLockIds);

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

            scope.WriteLock(WriteLockIds);
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
                DeleteVersions(id, content?.UpdateDate ?? DateTime.UtcNow, userId);
            }

            scope.WriteLock(WriteLockIds);
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

    #region Internal Methods

    internal IEnumerable<TContent> GetPublishedDescendantsLocked(TContent content)
    {
        var pathMatch = content.Path + ",";
        IQuery<TContent> query = Query<TContent>()
            .Where(x => x.Id != content.Id && x.Path.StartsWith(pathMatch) /*&& culture.Trashed == false*/);
        IEnumerable<TContent> contents = _documentRepository.Get(query);

        // beware! contents contains all published version below content
        // including those that are not directly published because below an unpublished content
        // these must be filtered out here
        var parents = new List<int> { content.Id };
        if (contents is not null)
        {
            foreach (TContent c in contents)
            {
                if (parents.Contains(c.ParentId))
                {
                    yield return c;
                    parents.Add(c.Id);
                }
            }
        }
    }

    #endregion

    #region Auditing

    protected void Audit(AuditType type, int userId, int objectId, string? message = null, string? parameters = null) =>
        AuditAsync(type, userId, objectId, message, parameters).GetAwaiter().GetResult();

    protected async Task AuditAsync(AuditType type, int userId, int objectId, string? message = null, string? parameters = null)
    {
        Guid userKey = await _userIdKeyResolver.GetAsync(userId);

        await _auditService.AddAsync(
            type,
            userKey,
            objectId,
            ContentObjectType.GetName(),
            message,
            parameters);
    }

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

        scope.ReadLock(ReadLockIds);

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

    #region Publishing Strategies

    /// <summary>
    ///     Ensures that a document can be published
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="content"></param>
    /// <param name="checkPath"></param>
    /// <param name="culturesUnpublishing"></param>
    /// <param name="evtMsgs"></param>
    /// <param name="culturesPublishing"></param>
    /// <param name="allLangs"></param>
    /// <param name="notificationState"></param>
    /// <returns></returns>
    private PublishResult StrategyCanPublish(
        ICoreScope scope,
        TContent content,
        bool checkPath,
        IReadOnlyList<string>? culturesPublishing,
        IReadOnlyCollection<string>? culturesUnpublishing,
        EventMessages evtMsgs,
        IReadOnlyCollection<ILanguage> allLangs,
        IDictionary<string, object?>? notificationState)
    {
        var variesByCulture = content.ContentType.VariesByCulture();

        // If it's null it's invariant
        CultureImpact[] impactsToPublish = culturesPublishing == null
                ? new[] { _cultureImpactFactory.ImpactInvariant() }
            : culturesPublishing.Select(x =>
                _cultureImpactFactory.ImpactExplicit(
                        x,
                        allLangs.Any(lang => lang.IsoCode.InvariantEquals(x) && lang.IsMandatory)))
                    .ToArray();

        // publish the culture(s)
        var publishTime = DateTime.UtcNow;
        if (!impactsToPublish.All(impact => content.PublishCulture(impact, publishTime, _propertyEditorCollection)))
        {
            return new PublishResult(PublishResultType.FailedPublishContentInvalid, evtMsgs, content);
        }

        // Validate the property values
        IProperty[]? invalidProperties = null;
        if (!impactsToPublish.All(x =>
                _propertyValidationService.Value.IsPropertyDataValid(content, out invalidProperties, x)))
        {
            return new PublishResult(PublishResultType.FailedPublishContentInvalid, evtMsgs, content)
            {
                InvalidProperties = invalidProperties,
            };
        }

        // Check if mandatory languages fails, if this fails it will mean anything that the published flag on the document will
        // be changed to Unpublished and any culture currently published will not be visible.
        if (variesByCulture)
        {
            if (culturesPublishing == null)
            {
                throw new InvalidOperationException(
                    "Internal error, variesByCulture but culturesPublishing is null.");
            }

            if (content.Published && culturesPublishing.Count == 0 && culturesUnpublishing?.Count == 0)
            {
                // no published cultures = cannot be published
                // This will occur if for example, a culture that is already unpublished is sent to be unpublished again, or vice versa, in that case
                // there will be nothing to publish/unpublish.
                return new PublishResult(PublishResultType.FailedPublishNothingToPublish, evtMsgs, content);
            }

            // missing mandatory culture = cannot be published
            IEnumerable<string> mandatoryCultures = allLangs.Where(x => x.IsMandatory).Select(x => x.IsoCode);
            var mandatoryMissing = mandatoryCultures.Any(x =>
                !content.PublishedCultures.Contains(x, StringComparer.OrdinalIgnoreCase));
            if (mandatoryMissing)
            {
                return new PublishResult(PublishResultType.FailedPublishMandatoryCultureMissing, evtMsgs, content);
            }

            if (culturesPublishing.Count == 0 && culturesUnpublishing?.Count > 0)
            {
                return new PublishResult(PublishResultType.SuccessUnpublishCulture, evtMsgs, content);
            }
        }

        // ensure that the document has published values
        // either because it is 'publishing' or because it already has a published version
        if (content.PublishedState != PublishedState.Publishing && content.PublishedVersionId == 0)
        {
            Logger.LogInformation(
                "Document {ContentName} (id={ContentId}) cannot be published: {Reason}",
                content.Name,
                content.Id,
                "document does not have published values");
            return new PublishResult(PublishResultType.FailedPublishNothingToPublish, evtMsgs, content);
        }

        ContentScheduleCollection contentSchedule = _documentRepository.GetContentSchedule(content.Id);

        // loop over each culture publishing - or InvariantCulture for invariant
        foreach (var culture in culturesPublishing ?? new[] { Constants.System.InvariantCulture })
        {
            // ensure that the document status is correct
            // note: culture will be string.Empty for invariant
            switch (content.GetStatus(contentSchedule, culture))
            {
                case ContentStatus.Expired:
                    if (!variesByCulture)
                    {
                        Logger.LogInformation(
                            "Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "document has expired");
                    }
                    else
                    {
                        Logger.LogInformation(
                            "Document {ContentName} (id={ContentId}) culture {Culture} cannot be published: {Reason}", content.Name, content.Id, culture, "document culture has expired");
                    }

                    return new PublishResult(
                        !variesByCulture
                            ? PublishResultType.FailedPublishHasExpired : PublishResultType.FailedPublishCultureHasExpired,
                        evtMsgs,
                        content);

                case ContentStatus.AwaitingRelease:
                    if (!variesByCulture)
                    {
                        Logger.LogInformation(
                            "Document {ContentName} (id={ContentId}) cannot be published: {Reason}",
                            content.Name,
                            content.Id,
                            "document is awaiting release");
                    }
                    else
                    {
                        Logger.LogInformation(
                            "Document {ContentName} (id={ContentId}) culture {Culture} cannot be published: {Reason}",
                            content.Name,
                            content.Id,
                            culture,
                            "document has culture awaiting release");
                    }

                    return new PublishResult(
                        !variesByCulture
                            ? PublishResultType.FailedPublishAwaitingRelease
                            : PublishResultType.FailedPublishCultureAwaitingRelease,
                        evtMsgs,
                        content);

                case ContentStatus.Trashed:
                    Logger.LogInformation(
                        "Document {ContentName} (id={ContentId}) cannot be published: {Reason}",
                        content.Name,
                        content.Id,
                        "document is trashed");
                    return new PublishResult(PublishResultType.FailedPublishIsTrashed, evtMsgs, content);
            }
        }

        if (checkPath && SupportsBranchPublishing)
        {
            // check if the content can be path-published
            // root content can be published
            // else check ancestors - we know we are not trashed
            var pathIsOk = content.ParentId == Constants.System.Root || IsPathPublished(GetParent(content));
            if (!pathIsOk)
            {
                Logger.LogInformation(
                    "Document {ContentName} (id={ContentId}) cannot be published: {Reason}",
                    content.Name,
                    content.Id,
                    "parent is not published");
                return new PublishResult(PublishResultType.FailedPublishPathNotPublished, evtMsgs, content);
            }
        }

        // If we are both publishing and unpublishing cultures, then return a mixed status
        if (variesByCulture && culturesPublishing?.Count > 0 && culturesUnpublishing?.Count > 0)
        {
            return new PublishResult(PublishResultType.SuccessMixedCulture, evtMsgs, content);
        }

        return new PublishResult(evtMsgs, content);
    }

    /// <summary>
    ///     Publishes a document
    /// </summary>
    /// <param name="content"></param>
    /// <param name="culturesUnpublishing"></param>
    /// <param name="evtMsgs"></param>
    /// <param name="culturesPublishing"></param>
    /// <returns></returns>
    /// <remarks>
    ///     It is assumed that all publishing checks have passed before calling this method like
    ///     <see cref="StrategyCanPublish" />
    /// </remarks>
    private PublishResult StrategyPublish(
        TContent content,
        IReadOnlyCollection<string>? culturesPublishing,
        IReadOnlyCollection<string>? culturesUnpublishing,
        EventMessages evtMsgs)
    {
        // change state to publishing
        content.PublishedState = PublishedState.Publishing;

        // if this is a variant then we need to log which cultures have been published/unpublished and return an appropriate result
        if (content.ContentType.VariesByCulture())
        {
            if (content.Published && culturesUnpublishing?.Count == 0 && culturesPublishing?.Count == 0)
            {
                return new PublishResult(PublishResultType.FailedPublishNothingToPublish, evtMsgs, content);
            }

            if (culturesUnpublishing?.Count > 0)
            {
                Logger.LogInformation(
                    "Document {ContentName} (id={ContentId}) cultures: {Cultures} have been unpublished.",
                    content.Name,
                    content.Id,
                    string.Join(",", culturesUnpublishing));
            }

            if (culturesPublishing?.Count > 0)
            {
                Logger.LogInformation(
                    "Document {ContentName} (id={ContentId}) cultures: {Cultures} have been published.",
                    content.Name,
                    content.Id,
                    string.Join(",", culturesPublishing));
            }

            if (culturesUnpublishing?.Count > 0 && culturesPublishing?.Count > 0)
            {
                return new PublishResult(PublishResultType.SuccessMixedCulture, evtMsgs, content);
            }

            if (culturesUnpublishing?.Count > 0 && culturesPublishing?.Count == 0)
            {
                return new PublishResult(PublishResultType.SuccessUnpublishCulture, evtMsgs, content);
            }

            return new PublishResult(PublishResultType.SuccessPublishCulture, evtMsgs, content);
        }

        Logger.LogInformation("Document {ContentName} (id={ContentId}) has been published.", content.Name, content.Id);
        return new PublishResult(evtMsgs, content);
    }

    /// <summary>
    ///     Ensures that a document can be unpublished
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="content"></param>
    /// <param name="evtMsgs"></param>
    /// <param name="notificationState"></param>
    /// <returns></returns>
    private PublishResult StrategyCanUnpublish(
        ICoreScope scope,
        TContent content,
        EventMessages evtMsgs,
        IDictionary<string, object?>? notificationState)
    {
        // raise Unpublishing notification
        CancelableEnumerableObjectNotification<TContent> notification = UnpublishingNotification(content, evtMsgs).WithState(notificationState);
        var notificationResult = scope.Notifications.PublishCancelable(notification);

        if (notificationResult)
        {
            Logger.LogInformation(
                "Document {ContentName} (id={ContentId}) cannot be unpublished: unpublishing was cancelled.", content.Name, content.Id);
            return new PublishResult(PublishResultType.FailedUnpublishCancelledByEvent, evtMsgs, content);
        }

        return new PublishResult(PublishResultType.SuccessUnpublish, evtMsgs, content);
    }

    /// <summary>
    ///     Unpublishes a document
    /// </summary>
    /// <param name="content"></param>
    /// <param name="evtMsgs"></param>
    /// <returns></returns>
    /// <remarks>
    ///     It is assumed that all unpublishing checks have passed before calling this method like
    ///     <see cref="StrategyCanUnpublish" />
    /// </remarks>
    private PublishResult StrategyUnpublish(TContent content, EventMessages evtMsgs)
    {
        var attempt = new PublishResult(PublishResultType.SuccessUnpublish, evtMsgs, content);

        // TODO: What is this check?? we just created this attempt and of course it is Success?!
        if (attempt.Success == false)
        {
            return attempt;
        }

        // if the document has any release dates set to before now,
        // they should be removed so they don't interrupt an unpublish
        // otherwise it would remain released == published
        ContentScheduleCollection contentSchedule = _documentRepository.GetContentSchedule(content.Id);
        IReadOnlyList<ContentSchedule> pastReleases =
            contentSchedule.GetPending(ContentScheduleAction.Expire, DateTime.UtcNow);
        foreach (ContentSchedule p in pastReleases)
        {
            contentSchedule.Remove(p);
        }

        if (pastReleases.Count > 0)
        {
            Logger.LogInformation(
                "Document {ContentName} (id={ContentId}) had its release date removed, because it was unpublished.", content.Name, content.Id);
        }

        _documentRepository.PersistContentSchedule(content, contentSchedule);

        // change state to unpublishing
        content.PublishedState = PublishedState.Unpublishing;

        Logger.LogInformation("Document {ContentName} (id={ContentId}) has been unpublished.", content.Name, content.Id);
        return attempt;
    }

    #endregion
}
