using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Implements content CRUD (Create, Read, Update, Delete) operations.
/// </summary>
public class ContentCrudService : ContentServiceBase, IContentCrudService
{
    private readonly IEntityRepository _entityRepository;
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly ILogger<ContentCrudService> _logger;

    public ContentCrudService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentRepository documentRepository,
        IEntityRepository entityRepository,
        IContentTypeRepository contentTypeRepository,
        IAuditService auditService,
        IUserIdKeyResolver userIdKeyResolver,
        ILanguageRepository languageRepository)
        : base(provider, loggerFactory, eventMessagesFactory, documentRepository, auditService, userIdKeyResolver)
    {
        _entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
        _contentTypeRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
        _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
        _logger = loggerFactory.CreateLogger<ContentCrudService>();
    }

    #region Create

    /// <inheritdoc />
    public IContent Create(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        IContentType? contentType = GetContentType(contentTypeAlias);
        if (contentType == null)
        {
            throw new ArgumentException($"No ContentType matching the passed in Alias: '{contentTypeAlias}' was found", nameof(contentTypeAlias));
        }

        return Create(name, parentId, contentType, userId);
    }

    /// <inheritdoc />
    public IContent Create(string name, Guid parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        IContent? parent = GetById(parentId);
        if (parent is null)
        {
            throw new ArgumentException($"No content with key '{parentId}' exists.", nameof(parentId));
        }
        return Create(name, parent, contentTypeAlias, userId);
    }

    /// <inheritdoc />
    public IContent Create(string name, int parentId, IContentType contentType, int userId = Constants.Security.SuperUserId)
    {
        if (contentType is null)
        {
            throw new ArgumentException("Content type must be specified", nameof(contentType));
        }

        IContent? parent = parentId > 0 ? GetById(parentId) : null;
        if (parentId > 0 && parent is null)
        {
            throw new ArgumentException("No content with that id.", nameof(parentId));
        }

        var content = new Content(name, parentId, contentType, userId);

        return content;
    }

    /// <inheritdoc />
    public IContent Create(string name, IContent? parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        IContentType contentType = GetContentType(contentTypeAlias)
            ?? throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias));

        var content = new Content(name, parent, contentType, userId);

        return content;
    }

    /// <inheritdoc />
    public IContent CreateAndSave(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            // locking the content tree secures content types too
            scope.WriteLock(Constants.Locks.ContentTree);
            scope.ReadLock(Constants.Locks.Languages);

            IContentType contentType = GetContentTypeLocked(contentTypeAlias)
                ?? throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias));

            IContent? parent = parentId > 0 ? GetById(parentId) : null;
            if (parentId > 0 && parent == null)
            {
                throw new ArgumentException("No content with that id.", nameof(parentId));
            }

            if (parent?.Trashed == true)
            {
                throw new InvalidOperationException(
                    $"Cannot create content under trashed parent '{parent.Name}' (id={parent.Id}).");
            }

            Content content = parentId > 0
                ? new Content(name, parent!, contentType, userId)
                : new Content(name, parentId, contentType, userId);

            SaveLocked(scope, content, userId);

            scope.Notifications.Publish(
                new ContentSavedNotification(content, eventMessages));
            scope.Notifications.Publish(
                new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshNode, eventMessages));

            scope.Complete();

            return content;
        }
    }

    /// <inheritdoc />
    public IContent CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            // locking the content tree secures content types too
            scope.WriteLock(Constants.Locks.ContentTree);
            scope.ReadLock(Constants.Locks.Languages);

            IContentType contentType = GetContentTypeLocked(contentTypeAlias)
                ?? throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias));

            if (parent.Trashed == true)
            {
                throw new InvalidOperationException(
                    $"Cannot create content under trashed parent '{parent.Name}' (id={parent.Id}).");
            }

            var content = new Content(name, parent, contentType, userId);

            SaveLocked(scope, content, userId);

            scope.Notifications.Publish(
                new ContentSavedNotification(content, eventMessages));
            scope.Notifications.Publish(
                new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshNode, eventMessages));

            scope.Complete();

            return content;
        }
    }

    #endregion

    #region Read

    /// <inheritdoc />
    public IContent? GetById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.Get(id);
        }
    }

    /// <inheritdoc />
    public IContent? GetById(Guid key)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.Get(key);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetByIds(IEnumerable<int> ids)
    {
        int[] idsA = ids.ToArray();
        if (idsA.Length == 0)
        {
            return Enumerable.Empty<IContent>();
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IEnumerable<IContent>? items = DocumentRepository.GetMany(idsA);

            if (items is not null)
            {
                // Use GroupBy to handle potential duplicate keys from repository
                var index = items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First());
                return idsA.Select(x => index.GetValueOrDefault(x)).WhereNotNull();
            }

            return Enumerable.Empty<IContent>();
        }
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetByIds(IEnumerable<Guid> ids)
    {
        Guid[] idsA = ids.ToArray();
        if (idsA.Length == 0)
        {
            return Enumerable.Empty<IContent>();
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IEnumerable<IContent>? items = DocumentRepository.GetMany(idsA);

            if (items is not null)
            {
                // Use GroupBy to handle potential duplicate keys from repository
                var index = items.GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.First());
                return idsA.Select(x => index.GetValueOrDefault(x)).WhereNotNull();
            }

            return Enumerable.Empty<IContent>();
        }
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetRootContent()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IQuery<IContent> query = Query<IContent>().Where(x => x.ParentId == Constants.System.Root);
            return DocumentRepository.Get(query);
        }
    }

    /// <inheritdoc />
    public IContent? GetParent(int id)
    {
        IContent? content = GetById(id);
        return GetParent(content);
    }

    /// <inheritdoc />
    public IContent? GetParent(IContent? content)
    {
        if (content?.ParentId == Constants.System.Root ||
            content?.ParentId == Constants.System.RecycleBinContent ||
            content is null)
        {
            return null;
        }

        return GetById(content.ParentId);
    }

    #endregion

    #region Read (Tree Traversal)

    /// <inheritdoc />
    public IEnumerable<IContent> GetAncestors(int id)
    {
        // intentionally not locking
        IContent? content = GetById(id);
        if (content is null)
        {
            return Enumerable.Empty<IContent>();
        }

        return GetAncestors(content);
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetAncestors(IContent content)
    {
        if (content?.Path == null || content.Level <= 1)
        {
            return Enumerable.Empty<IContent>();
        }

        // Parse path to get ancestor IDs: "-1,123,456,789" -> [123, 456]
        // Skip root (-1) and exclude self
        // Use TryParse for resilience against malformed path data
        var ancestorIds = content.Path
            .Split(',')
            .Skip(1)  // Skip root (-1)
            .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
            .Where(id => id.HasValue && id.Value != content.Id)  // Exclude nulls and self
            .Select(id => id!.Value)
            .ToArray();

        // Log warning if path appears malformed (expected ancestors but found none)
        if (ancestorIds.Length == 0 && content.Level > 1)
        {
            _logger.LogWarning(
                "Malformed path '{Path}' for content {ContentId} at level {Level} - expected {ExpectedCount} ancestors but parsed {ActualCount}",
                content.Path, content.Id, content.Level, content.Level - 1, ancestorIds.Length);
        }

        return GetByIds(ancestorIds);  // Single batch query instead of N+1
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null)
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

            IQuery<IContent>? query = Query<IContent>()?.Where(x => x.ParentId == id);
            return DocumentRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null)
    {
        ordering ??= Ordering.By("Path");

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);

            return GetPagedDescendantsLocked(id, pageIndex, pageSize, out totalChildren, filter, ordering);
        }
    }

    /// <inheritdoc />
    public bool HasChildren(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IQuery<IContent>? query = Query<IContent>()?.Where(x => x.ParentId == id);
            var count = DocumentRepository.Count(query);
            return count > 0;
        }
    }

    /// <inheritdoc />
    public bool Exists(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.Exists(id);
        }
    }

    /// <inheritdoc />
    public bool Exists(Guid key)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.Exists(key);
        }
    }

    #endregion

    #region Save

    /// <inheritdoc />
    public OperationResult Save(IContent content, int? userId = null, ContentScheduleCollection? contentSchedule = null)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingNotification = new ContentSavingNotification(content, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return OperationResult.Cancel(eventMessages);
            }

            scope.WriteLock(Constants.Locks.ContentTree);
            scope.ReadLock(Constants.Locks.Languages);
            userId ??= Constants.Security.SuperUserId;

            SaveLocked(scope, content, userId.Value, contentSchedule);

            scope.Notifications.Publish(
                new ContentSavedNotification(content, eventMessages).WithStateFrom(savingNotification));

            scope.Notifications.Publish(
                new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshNode, eventMessages));

            scope.Complete();
        }

        return OperationResult.Succeed(eventMessages);
    }

    /// <inheritdoc />
    public OperationResult Save(IEnumerable<IContent> contents, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        IContent[] contentsA = contents.ToArray();

        if (contentsA.Length == 0)
        {
            return OperationResult.Succeed(eventMessages);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingNotification = new ContentSavingNotification(contentsA, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return OperationResult.Cancel(eventMessages);
            }

            scope.WriteLock(Constants.Locks.ContentTree);
            scope.ReadLock(Constants.Locks.Languages);
            foreach (IContent content in contentsA)
            {
                if (content.HasIdentity == false)
                {
                    content.CreatorId = userId;
                }

                content.WriterId = userId;

                DocumentRepository.Save(content);
            }

            scope.Notifications.Publish(
                new ContentSavedNotification(contentsA, eventMessages).WithStateFrom(savingNotification));

            scope.Notifications.Publish(
                new ContentTreeChangeNotification(contentsA, TreeChangeTypes.RefreshNode, eventMessages));

            Audit(AuditType.Save, userId, Constants.System.Root, $"Saved {contentsA.Length} content items");

            scope.Complete();
        }

        return OperationResult.Succeed(eventMessages);
    }

    #endregion

    #region Delete

    /// <inheritdoc />
    public OperationResult Delete(IContent content, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            if (scope.Notifications.PublishCancelable(new ContentDeletingNotification(content, eventMessages)))
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
                scope.Notifications.Publish(new ContentUnpublishedNotification(content, eventMessages));
            }

            DeleteLocked(scope, content, eventMessages);

            scope.Notifications.Publish(
                new ContentTreeChangeNotification(content, TreeChangeTypes.Remove, eventMessages));
            Audit(AuditType.Delete, userId, content.Id);

            scope.Complete();
        }

        return OperationResult.Succeed(eventMessages);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Gets a content type by alias without creating a scope (must be called within an existing scope).
    /// </summary>
    private IContentType? GetContentType(string contentTypeAlias)
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
            return GetContentTypeLocked(contentTypeAlias);
        }
    }

    /// <summary>
    /// Gets a content type by alias within an existing scope with proper locking.
    /// </summary>
    private IContentType? GetContentTypeLocked(string contentTypeAlias)
    {
        if (contentTypeAlias == null)
        {
            throw new ArgumentNullException(nameof(contentTypeAlias));
        }

        if (string.IsNullOrWhiteSpace(contentTypeAlias))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(contentTypeAlias));
        }

        IQuery<IContentType> query = Query<IContentType>().Where(x => x.Alias == contentTypeAlias);
        IContentType? contentType = _contentTypeRepository.Get(query).FirstOrDefault();

        return contentType;
    }

    /// <summary>
    /// Saves content within an existing scope with proper locking.
    /// </summary>
    /// <remarks>
    /// Must be called from within a scope that holds ContentTree write lock.
    /// If content type varies by culture, caller must also hold Languages read lock.
    /// </remarks>
    private void SaveLocked(ICoreScope scope, IContent content, int userId, ContentScheduleCollection? contentSchedule = null)
    {
        // Validation INSIDE locked section to prevent race conditions
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

        if (content.HasIdentity == false)
        {
            content.CreatorId = userId;
        }

        content.WriterId = userId;

        // track the cultures that have changed
        List<string>? culturesChanging = content.ContentType.VariesByCulture()
            ? content.CultureInfos?.Values.Where(x => x.IsDirty()).Select(x => x.Culture).ToList()
            : null;

        DocumentRepository.Save(content);

        if (contentSchedule != null)
        {
            DocumentRepository.PersistContentSchedule(content, contentSchedule);
        }

        if (culturesChanging != null && culturesChanging.Any())
        {
            var langs = GetLanguageDetailsForAuditEntryLocked(culturesChanging);
            Audit(AuditType.SaveVariant, userId, content.Id, $"Saved languages: {langs}", langs);
        }
        else
        {
            Audit(AuditType.Save, userId, content.Id);
        }
    }

    /// <summary>
    /// Deletes content and all descendants within an existing scope.
    /// </summary>
    /// <remarks>
    /// Must be called from within a scope that holds ContentTree write lock.
    /// Uses paging to handle large trees without loading everything into memory.
    /// Iteration bound prevents infinite loops if database is in inconsistent state.
    /// </remarks>
    private void DeleteLocked(ICoreScope scope, IContent content, EventMessages evtMsgs)
    {
        void DoDelete(IContent c)
        {
            DocumentRepository.Delete(c);
            scope.Notifications.Publish(new ContentDeletedNotification(c, evtMsgs));
        }

        const int pageSize = 500;
        const int maxIterations = 10000; // Safety bound to prevent infinite loops
        var iteration = 0;
        var total = long.MaxValue;

        while (total > 0 && iteration < maxIterations)
        {
            // get descendants - ordered from deepest to shallowest
            IEnumerable<IContent> descendants = GetPagedDescendantsLocked(
                content.Id,
                0,
                pageSize,
                out total,
                ordering: Ordering.By("Path", Direction.Descending));

            var batch = descendants.ToList();

            // Exit if we got an empty batch (even if total > 0, which indicates inconsistency)
            if (batch.Count == 0)
            {
                if (total > 0)
                {
                    _logger.LogWarning(
                        "GetPagedDescendants reported {Total} total descendants but returned empty batch for content {ContentId}",
                        total,
                        content.Id);
                }
                break;
            }

            foreach (IContent c in batch)
            {
                DoDelete(c);
            }

            iteration++;
        }

        if (iteration >= maxIterations)
        {
            _logger.LogError(
                "DeleteLocked exceeded maximum iteration limit ({MaxIterations}) for content {ContentId}. Tree may be incompletely deleted.",
                maxIterations,
                content.Id);
        }

        DoDelete(content);
    }

    /// <summary>
    /// Gets paged descendants within an existing scope.
    /// </summary>
    /// <remarks>
    /// Must be called from within a scope that holds ContentTree read lock.
    /// </remarks>
    private IEnumerable<IContent> GetPagedDescendantsLocked(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (ordering == null)
        {
            throw new ArgumentNullException(nameof(ordering));
        }

        // if the id is System Root, then just get all
        if (id != Constants.System.Root)
        {
            TreeEntityPath[] contentPath =
                _entityRepository.GetAllPaths(Constants.ObjectTypes.Document, id).ToArray();
            if (contentPath.Length == 0)
            {
                totalChildren = 0;
                return Enumerable.Empty<IContent>();
            }

            IQuery<IContent>? query = GetPagedDescendantQuery(contentPath[0].Path);
            return DocumentRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering);
        }

        return DocumentRepository.GetPage(null, pageIndex, pageSize, out totalChildren, filter, ordering);
    }

    /// <summary>
    /// Builds a query for descendants based on path.
    /// </summary>
    private IQuery<IContent>? GetPagedDescendantQuery(string contentPath)
    {
        IQuery<IContent>? query = Query<IContent>();
        if (!contentPath.IsNullOrWhiteSpace())
        {
            query?.Where(x => x.Path.SqlStartsWith($"{contentPath},", TextColumnType.NVarchar));
        }

        return query;
    }

    /// <summary>
    /// Gets language details for audit entry (creates new scope).
    /// </summary>
    private string GetLanguageDetailsForAuditEntry(IEnumerable<string> affectedCultures)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.Languages);
            return GetLanguageDetailsForAuditEntryLocked(affectedCultures);
        }
    }

    /// <summary>
    /// Gets language details for audit entry within an existing scope.
    /// </summary>
    /// <remarks>
    /// Must be called from within a scope. Caller should hold Languages read lock if concurrent modifications are possible.
    /// </remarks>
    private string GetLanguageDetailsForAuditEntryLocked(IEnumerable<string> affectedCultures)
    {
        IEnumerable<ILanguage> languages = _languageRepository.GetMany();
        IEnumerable<string> languageIsoCodes = languages
            .Where(x => affectedCultures.InvariantContains(x.IsoCode))
            .Select(x => x.IsoCode);
        return string.Join(", ", languageIsoCodes);
    }

    #endregion
}
