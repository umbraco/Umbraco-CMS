using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services.Changes;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Implements the content service.
    /// </summary>
    public class ContentService : RepositoryService, IContentService, IContentVersionService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IEntityRepository _entityRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IDocumentBlueprintRepository _documentBlueprintRepository;
        private readonly ILanguageRepository _languageRepository;
        private IQuery<IContent> _queryNotTrashed;
        //TODO: The non-lazy object should be injected
        private readonly Lazy<PropertyValidationService> _propertyValidationService = new Lazy<PropertyValidationService>(() => new PropertyValidationService());


        #region Constructors

        public ContentService(IScopeProvider provider, ILogger logger,
            IEventMessagesFactory eventMessagesFactory,
            IDocumentRepository documentRepository, IEntityRepository entityRepository, IAuditRepository auditRepository,
            IContentTypeRepository contentTypeRepository, IDocumentBlueprintRepository documentBlueprintRepository, ILanguageRepository languageRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _documentRepository = documentRepository;
            _entityRepository = entityRepository;
            _auditRepository = auditRepository;
            _contentTypeRepository = contentTypeRepository;
            _documentBlueprintRepository = documentBlueprintRepository;
            _languageRepository = languageRepository;
        }

        #endregion

        #region Static queries

        // lazy-constructed because when the ctor runs, the query factory may not be ready

        private IQuery<IContent> QueryNotTrashed => _queryNotTrashed ?? (_queryNotTrashed = Query<IContent>().Where(x => x.Trashed == false));

        #endregion

        #region Count

        public int CountPublished(string contentTypeAlias = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.CountPublished(contentTypeAlias);
            }
        }

        public int Count(string contentTypeAlias = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.Count(contentTypeAlias);
            }
        }

        public int CountChildren(int parentId, string contentTypeAlias = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.CountChildren(parentId, contentTypeAlias);
            }
        }

        public int CountDescendants(int parentId, string contentTypeAlias = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.CountDescendants(parentId, contentTypeAlias);
            }
        }

        #endregion

        #region Permissions

        /// <summary>
        /// Used to bulk update the permissions set for a content item. This will replace all permissions
        /// assigned to an entity with a list of user id & permission pairs.
        /// </summary>
        /// <param name="permissionSet"></param>
        public void SetPermissions(EntityPermissionSet permissionSet)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);
                _documentRepository.ReplaceContentPermissions(permissionSet);
                scope.Complete();
            }
        }

        /// <summary>
        /// Assigns a single permission to the current content item for the specified group ids
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permission"></param>
        /// <param name="groupIds"></param>
        public void SetPermission(IContent entity, char permission, IEnumerable<int> groupIds)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);
                _documentRepository.AssignEntityPermission(entity, permission, groupIds);
                scope.Complete();
            }
        }

        /// <summary>
        /// Returns implicit/inherited permissions assigned to the content item for all user groups
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public EntityPermissionCollection GetPermissions(IContent content)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.GetPermissionsForEntity(content.Id);
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// Creates an <see cref="IContent"/> object using the alias of the <see cref="IContentType"/>
        /// that this Content should based on.
        /// </summary>
        /// <remarks>
        /// Note that using this method will simply return a new IContent without any identity
        /// as it has not yet been persisted. It is intended as a shortcut to creating new content objects
        /// that does not invoke a save operation against the database.
        /// </remarks>
        /// <param name="name">Name of the Content object</param>
        /// <param name="parentId">Id of Parent for the new Content</param>
        /// <param name="contentTypeAlias">Alias of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional id of the user creating the content</param>
        /// <returns><see cref="IContent"/></returns>
        public IContent Create(string name, Guid parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            // TODO: what about culture?

            var parent = GetById(parentId);
            return Create(name, parent, contentTypeAlias, userId);
        }

        /// <summary>
        /// Creates an <see cref="IContent"/> object of a specified content type.
        /// </summary>
        /// <remarks>This method simply returns a new, non-persisted, IContent without any identity. It
        /// is intended as a shortcut to creating new content objects that does not invoke a save
        /// operation against the database.
        /// </remarks>
        /// <param name="name">The name of the content object.</param>
        /// <param name="parentId">The identifier of the parent, or -1.</param>
        /// <param name="contentTypeAlias">The alias of the content type.</param>
        /// <param name="userId">The optional id of the user creating the content.</param>
        /// <returns>The content object.</returns>
        public IContent Create(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            // TODO: what about culture?

            var contentType = GetContentType(contentTypeAlias);
            return Create(name, parentId, contentType, userId);
        }

        /// <summary>
        /// Creates an <see cref="IContent"/> object of a specified content type.
        /// </summary>
        /// <remarks>This method simply returns a new, non-persisted, IContent without any identity. It
        /// is intended as a shortcut to creating new content objects that does not invoke a save
        /// operation against the database.
        /// </remarks>
        /// <param name="name">The name of the content object.</param>
        /// <param name="parentId">The identifier of the parent, or -1.</param>
        /// <param name="contentType">The content type of the content</param>
        /// <param name="userId">The optional id of the user creating the content.</param>
        /// <returns>The content object.</returns>
        public IContent Create(string name, int parentId, IContentType contentType,
            int userId = Constants.Security.SuperUserId)
        {
            if (contentType is null)
            {
                throw new ArgumentException("Content type must be specified", nameof(contentType));
            }

            var parent = parentId > 0 ? GetById(parentId) : null;
            if (parentId > 0 && parent is null)
            {
                throw new ArgumentException("No content with that id.", nameof(parentId));
            }

            var content = new Content(name, parentId, contentType);
            using (var scope = ScopeProvider.CreateScope())
            {
                CreateContent(scope, content, userId, false);
                scope.Complete();
            }

            return content;
        }

        /// <summary>
        /// Creates an <see cref="IContent"/> object of a specified content type, under a parent.
        /// </summary>
        /// <remarks>This method simply returns a new, non-persisted, IContent without any identity. It
        /// is intended as a shortcut to creating new content objects that does not invoke a save
        /// operation against the database.
        /// </remarks>
        /// <param name="name">The name of the content object.</param>
        /// <param name="parent">The parent content object.</param>
        /// <param name="contentTypeAlias">The alias of the content type.</param>
        /// <param name="userId">The optional id of the user creating the content.</param>
        /// <returns>The content object.</returns>
        public IContent Create(string name, IContent parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            // TODO: what about culture?

            if (parent == null) throw new ArgumentNullException(nameof(parent));

            using (var scope = ScopeProvider.CreateScope())
            {
                // not locking since not saving anything

                var contentType = GetContentType(contentTypeAlias);
                if (contentType == null)
                    throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias)); // causes rollback

                var content = new Content(name, parent, contentType);
                CreateContent(scope, content, userId, false);

                scope.Complete();
                return content;
            }
        }

        /// <summary>
        /// Creates an <see cref="IContent"/> object of a specified content type.
        /// </summary>
        /// <remarks>This method returns a new, persisted, IContent with an identity.</remarks>
        /// <param name="name">The name of the content object.</param>
        /// <param name="parentId">The identifier of the parent, or -1.</param>
        /// <param name="contentTypeAlias">The alias of the content type.</param>
        /// <param name="userId">The optional id of the user creating the content.</param>
        /// <returns>The content object.</returns>
        public IContent CreateAndSave(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            // TODO: what about culture?

            using (var scope = ScopeProvider.CreateScope())
            {
                // locking the content tree secures content types too
                scope.WriteLock(Constants.Locks.ContentTree);

                var contentType = GetContentType(contentTypeAlias); // + locks
                if (contentType == null)
                    throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias)); // causes rollback

                var parent = parentId > 0 ? GetById(parentId) : null; // + locks
                if (parentId > 0 && parent == null)
                    throw new ArgumentException("No content with that id.", nameof(parentId)); // causes rollback

                var content = parentId > 0 ? new Content(name, parent, contentType) : new Content(name, parentId, contentType);
                CreateContent(scope, content, userId, true);

                scope.Complete();
                return content;
            }
        }

        /// <summary>
        /// Creates an <see cref="IContent"/> object of a specified content type, under a parent.
        /// </summary>
        /// <remarks>This method returns a new, persisted, IContent with an identity.</remarks>
        /// <param name="name">The name of the content object.</param>
        /// <param name="parent">The parent content object.</param>
        /// <param name="contentTypeAlias">The alias of the content type.</param>
        /// <param name="userId">The optional id of the user creating the content.</param>
        /// <returns>The content object.</returns>
        public IContent CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            // TODO: what about culture?

            if (parent == null) throw new ArgumentNullException(nameof(parent));

            using (var scope = ScopeProvider.CreateScope())
            {
                // locking the content tree secures content types too
                scope.WriteLock(Constants.Locks.ContentTree);

                var contentType = GetContentType(contentTypeAlias); // + locks
                if (contentType == null)
                    throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias)); // causes rollback

                var content = new Content(name, parent, contentType);
                CreateContent(scope, content, userId, true);

                scope.Complete();
                return content;
            }
        }

        private void CreateContent(IScope scope, IContent content, int userId, bool withIdentity)
        {
            content.CreatorId = userId;
            content.WriterId = userId;

            if (withIdentity)
            {
                var evtMsgs = EventMessagesFactory.Get();

                // if saving is cancelled, content remains without an identity
                var saveEventArgs = new ContentSavingEventArgs(content, evtMsgs);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs, nameof(Saving)))
                    return;

                _documentRepository.Save(content);

                scope.Events.Dispatch(Saved, this, saveEventArgs.ToContentSavedEventArgs(), nameof(Saved));
                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, TreeChangeTypes.RefreshNode).ToEventArgs());
            }

            if (withIdentity == false)
                return;

            Audit(AuditType.New, content.CreatorId, content.Id, $"Content '{content.Name}' was created with Id {content.Id}");
        }

        #endregion

        #region Get, Has, Is

        /// <summary>
        /// Gets an <see cref="IContent"/> object by Id
        /// </summary>
        /// <param name="id">Id of the Content to retrieve</param>
        /// <returns><see cref="IContent"/></returns>
        public IContent GetById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.Get(id);
            }
        }

        /// <summary>
        /// Gets an <see cref="IContent"/> object by Id
        /// </summary>
        /// <param name="ids">Ids of the Content to retrieve</param>
        /// <returns><see cref="IContent"/></returns>
        public IEnumerable<IContent> GetByIds(IEnumerable<int> ids)
        {
            var idsA = ids.ToArray();
            if (idsA.Length == 0) return Enumerable.Empty<IContent>();

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var items = _documentRepository.GetMany(idsA);

                var index = items.ToDictionary(x => x.Id, x => x);

                return idsA.Select(x => index.TryGetValue(x, out var c) ? c : null).WhereNotNull();
            }
        }

        /// <summary>
        /// Gets an <see cref="IContent"/> object by its 'UniqueId'
        /// </summary>
        /// <param name="key">Guid key of the Content to retrieve</param>
        /// <returns><see cref="IContent"/></returns>
        public IContent GetById(Guid key)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.Get(key);
            }
        }

        /// <summary>
        /// Gets <see cref="IContent"/> objects by Ids
        /// </summary>
        /// <param name="ids">Ids of the Content to retrieve</param>
        /// <returns><see cref="IContent"/></returns>
        public IEnumerable<IContent> GetByIds(IEnumerable<Guid> ids)
        {
            var idsA = ids.ToArray();
            if (idsA.Length == 0) return Enumerable.Empty<IContent>();

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var items = _documentRepository.GetMany(idsA);

                var index = items.ToDictionary(x => x.Key, x => x);

                return idsA.Select(x => index.TryGetValue(x, out var c) ? c : null).WhereNotNull();
            }
        }

        /// <inheritdoc />
        public IEnumerable<IContent> GetPagedOfType(int contentTypeId, long pageIndex, int pageSize, out long totalRecords
            , IQuery<IContent> filter = null, Ordering ordering = null)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            if (ordering == null)
                ordering = Ordering.By("sortOrder");

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.GetPage(
                    Query<IContent>().Where(x => x.ContentTypeId == contentTypeId),
                    pageIndex, pageSize, out totalRecords, filter, ordering);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IContent> GetPagedOfTypes(int[] contentTypeIds, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent> filter, Ordering ordering = null)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            if (ordering == null)
                ordering = Ordering.By("sortOrder");

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.GetPage(
                    Query<IContent>().Where(x => contentTypeIds.Contains(x.ContentTypeId)),
                    pageIndex, pageSize, out totalRecords, filter, ordering);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Level
        /// </summary>
        /// <param name="level">The level to retrieve Content from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        /// <remarks>Contrary to most methods, this method filters out trashed content items.</remarks>
        public IEnumerable<IContent> GetByLevel(int level)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var query = Query<IContent>().Where(x => x.Level == level && x.Trashed == false);
                return _documentRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets a specific version of an <see cref="IContent"/> item.
        /// </summary>
        /// <param name="versionId">Id of the version to retrieve</param>
        /// <returns>An <see cref="IContent"/> item</returns>
        public IContent GetVersion(int versionId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.GetVersion(versionId);
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects versions by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetVersions(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.GetAllVersions(id);
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects versions by Id
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetVersionsSlim(int id, int skip, int take)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.GetAllVersionsSlim(id, skip, take);
            }
        }

        /// <summary>
        /// Gets a list of all version Ids for the given content item ordered so latest is first
        /// </summary>
        /// <param name="id"></param>
        /// <param name="maxRows">The maximum number of rows to return</param>
        /// <returns></returns>
        public IEnumerable<int> GetVersionIds(int id, int maxRows)
        {
            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                return _documentRepository.GetVersionIds(id, maxRows);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which are ancestors of the current content.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> to retrieve ancestors for</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetAncestors(int id)
        {
            // intentionally not locking
            var content = GetById(id);
            return GetAncestors(content);
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which are ancestors of the current content.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to retrieve ancestors for</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetAncestors(IContent content)
        {
            //null check otherwise we get exceptions
            if (content.Path.IsNullOrWhiteSpace()) return Enumerable.Empty<IContent>();

            var rootId = Constants.System.RootString;
            var ids = content.Path.Split(Constants.CharArrays.Comma)
                .Where(x => x != rootId && x != content.Id.ToString(CultureInfo.InvariantCulture)).Select(int.Parse).ToArray();
            if (ids.Any() == false)
                return new List<IContent>();

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.GetMany(ids);
            }
        }

        /// <summary>
        /// Gets a collection of published <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <returns>An Enumerable list of published <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetPublishedChildren(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var query = Query<IContent>().Where(x => x.ParentId == id && x.Published);
                return _documentRepository.Get(query).OrderBy(x => x.SortOrder);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren,
            IQuery<IContent> filter = null, Ordering ordering = null)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            if (ordering == null)
                ordering = Ordering.By("sortOrder");

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);

                var query = Query<IContent>().Where(x => x.ParentId == id);
                return _documentRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren,
            IQuery<IContent> filter = null, Ordering ordering = null)
        {
            if (ordering == null)
                ordering = Ordering.By("Path");

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);

                //if the id is System Root, then just get all
                if (id != Constants.System.Root)
                {
                    var contentPath = _entityRepository.GetAllPaths(Constants.ObjectTypes.Document, id).ToArray();
                    if (contentPath.Length == 0)
                    {
                        totalChildren = 0;
                        return Enumerable.Empty<IContent>();
                    }
                    return GetPagedLocked(GetPagedDescendantQuery(contentPath[0].Path), pageIndex, pageSize, out totalChildren, filter, ordering);
                }
                return GetPagedLocked(null, pageIndex, pageSize, out totalChildren, filter, ordering);
            }
        }

        private IQuery<IContent> GetPagedDescendantQuery(string contentPath)
        {
            var query = Query<IContent>();
            if (!contentPath.IsNullOrWhiteSpace())
                query.Where(x => x.Path.SqlStartsWith($"{contentPath},", TextColumnType.NVarchar));
            return query;
        }

        private IEnumerable<IContent> GetPagedLocked(IQuery<IContent> query, long pageIndex, int pageSize, out long totalChildren,
            IQuery<IContent> filter, Ordering ordering)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (ordering == null) throw new ArgumentNullException(nameof(ordering));

            return _documentRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering);
        }

        /// <summary>
        /// Gets the parent of the current content as an <see cref="IContent"/> item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> to retrieve the parent from</param>
        /// <returns>Parent <see cref="IContent"/> object</returns>
        public IContent GetParent(int id)
        {
            // intentionally not locking
            var content = GetById(id);
            return GetParent(content);
        }

        /// <summary>
        /// Gets the parent of the current content as an <see cref="IContent"/> item.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to retrieve the parent from</param>
        /// <returns>Parent <see cref="IContent"/> object</returns>
        public IContent GetParent(IContent content)
        {
            if (content.ParentId == Constants.System.Root || content.ParentId == Constants.System.RecycleBinContent)
                return null;

            return GetById(content.ParentId);
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetRootContent()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var query = Query<IContent>().Where(x => x.ParentId == Constants.System.Root);
                return _documentRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets all published content items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<IContent> GetAllPublished()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.Get(QueryNotTrashed);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IContent> GetContentForExpiration(DateTime date)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.GetContentForExpiration(date);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IContent> GetContentForRelease(DateTime date)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.GetContentForRelease(date);
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetPagedContentInRecycleBin(long pageIndex, int pageSize, out long totalRecords,
            IQuery<IContent> filter = null, Ordering ordering = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                if (ordering == null)
                    ordering = Ordering.By("Path");

                scope.ReadLock(Constants.Locks.ContentTree);
                var query = Query<IContent>().Where(x => x.Path.StartsWith(Constants.System.RecycleBinContentPathPrefix));
                return _documentRepository.GetPage(query, pageIndex, pageSize, out totalRecords, filter, ordering);
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IContent"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/></param>
        /// <returns>True if the content has any children otherwise False</returns>
        public bool HasChildren(int id)
        {
            return CountChildren(id) > 0;
        }

        /// <summary>
        /// Checks if the passed in <see cref="IContent"/> can be published based on the ancestors publish state.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to check if ancestors are published</param>
        /// <returns>True if the Content can be published, otherwise False</returns>
        public bool IsPathPublishable(IContent content)
        {
            // fast
            if (content.ParentId == Constants.System.Root) return true; // root content is always publishable
            if (content.Trashed) return false; // trashed content is never publishable

            // not trashed and has a parent: publishable if the parent is path-published
            var parent = GetById(content.ParentId);
            return parent == null || IsPathPublished(parent);
        }

        public bool IsPathPublished(IContent content)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.IsPathPublished(content);
            }
        }

        #endregion

        #region Save, Publish, Unpublish

        /// <inheritdoc />
        public OperationResult Save(IContent content, int userId = Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            var publishedState = content.PublishedState;
            if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
                throw new InvalidOperationException($"Cannot save (un)publishing content with name: {content.Name} - and state: {content.PublishedState}, use the dedicated SavePublished method.");

            if (content.Name != null && content.Name.Length > 255)
            {
                throw new InvalidOperationException($"Content with the name {content.Name} cannot be more than 255 characters in length.");
            }

            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new ContentSavingEventArgs(content, evtMsgs);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs, nameof(Saving)))
                {
                    scope.Complete();
                    return OperationResult.Cancel(evtMsgs);
                }

                scope.WriteLock(Constants.Locks.ContentTree);

                if (content.HasIdentity == false)
                    content.CreatorId = userId;
                content.WriterId = userId;

                //track the cultures that have changed
                var culturesChanging = content.ContentType.VariesByCulture()
                    ? content.CultureInfos.Values.Where(x => x.IsDirty()).Select(x => x.Culture).ToList()
                    : null;
                // TODO: Currently there's no way to change track which variant properties have changed, we only have change
                // tracking enabled on all values on the Property which doesn't allow us to know which variants have changed.
                // in this particular case, determining which cultures have changed works with the above with names since it will
                // have always changed if it's been saved in the back office but that's not really fail safe.

                _documentRepository.Save(content);

                if (raiseEvents)
                {
                    scope.Events.Dispatch(Saved, this, saveEventArgs.ToContentSavedEventArgs(), nameof(Saved));
                }
                var changeType = TreeChangeTypes.RefreshNode;
                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, changeType).ToEventArgs());

                if (culturesChanging != null)
                {
                    var langs = string.Join(", ", _languageRepository.GetMany()
                        .Where(x => culturesChanging.InvariantContains(x.IsoCode))
                        .Select(x => x.CultureName));
                    Audit(AuditType.SaveVariant, userId, content.Id, $"Saved languages: {langs}", langs);
                }
                else
                    Audit(AuditType.Save, userId, content.Id);

                scope.Complete();
            }

            return OperationResult.Succeed(evtMsgs);
        }

        /// <inheritdoc />
        public OperationResult Save(IEnumerable<IContent> contents, int userId = Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var contentsA = contents.ToArray();

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new ContentSavingEventArgs(contentsA, evtMsgs);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs, nameof(Saving)))
                {
                    scope.Complete();
                    return OperationResult.Cancel(evtMsgs);
                }

                var treeChanges = contentsA.Select(x => new TreeChange<IContent>(x, TreeChangeTypes.RefreshNode));

                scope.WriteLock(Constants.Locks.ContentTree);
                foreach (var content in contentsA)
                {
                    if (content.HasIdentity == false)
                        content.CreatorId = userId;
                    content.WriterId = userId;

                    _documentRepository.Save(content);
                }

                if (raiseEvents)
                {
                    scope.Events.Dispatch(Saved, this, saveEventArgs.ToContentSavedEventArgs(), nameof(Saved));
                }
                scope.Events.Dispatch(TreeChanged, this, treeChanges.ToEventArgs());
                Audit(AuditType.Save, userId == -1 ? 0 : userId, Constants.System.Root, "Saved multiple content");

                scope.Complete();
            }

            return OperationResult.Succeed(evtMsgs);
        }

        /// <inheritdoc />
        public PublishResult SaveAndPublish(IContent content, string culture = "*", int userId = Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();

            var publishedState = content.PublishedState;
            if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
                throw new InvalidOperationException($"Cannot save-and-publish (un)publishing content, use the dedicated {nameof(CommitDocumentChanges)} method.");

            // cannot accept invariant (null or empty) culture for variant content type
            // cannot accept a specific culture for invariant content type (but '*' is ok)
            if (content.ContentType.VariesByCulture())
            {
                if (culture.IsNullOrWhiteSpace())
                    throw new NotSupportedException("Invariant culture is not supported by variant content types.");
            }
            else
            {
                if (!culture.IsNullOrWhiteSpace() && culture != "*")
                    throw new NotSupportedException($"Culture \"{culture}\" is not supported by invariant content types.");
            }

            if (content.Name != null && content.Name.Length > 255)
            {
                throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
            }

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                var allLangs = _languageRepository.GetMany().ToList();

                var saveEventArgs = new ContentSavingEventArgs(content, evtMsgs);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs, nameof(Saving)))
                    return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, content);

                // if culture is specific, first publish the invariant values, then publish the culture itself.
                // if culture is '*', then publish them all (including variants)

                //this will create the correct culture impact even if culture is * or null
                var impact = CultureImpact.Create(culture, IsDefaultCulture(allLangs, culture), content);

                // publish the culture(s)
                // we don't care about the response here, this response will be rechecked below but we need to set the culture info values now.
                content.PublishCulture(impact);

                var result = CommitDocumentChangesInternal(scope, content, saveEventArgs, allLangs, userId, raiseEvents);
                scope.Complete();
                return result;
            }
        }

        /// <inheritdoc />
        public PublishResult SaveAndPublish(IContent content, string[] cultures, int userId = Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (cultures == null) throw new ArgumentNullException(nameof(cultures));

            if (content.Name != null && content.Name.Length > 255)
            {
                throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
            }

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                var allLangs = _languageRepository.GetMany().ToList();

                var evtMsgs = EventMessagesFactory.Get();
                var saveEventArgs = new ContentSavingEventArgs(content, evtMsgs);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs, nameof(Saving)))
                    return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, content);

                var varies = content.ContentType.VariesByCulture();

                if (cultures.Length == 0 && !varies)
                {
                    //no cultures specified and doesn't vary, so publish it, else nothing to publish
                    return SaveAndPublish(content, userId: userId, raiseEvents: raiseEvents);
                }

                if (cultures.Any(x => x == null || x == "*"))
                    throw new InvalidOperationException("Only valid cultures are allowed to be used in this method, wildcards or nulls are not allowed");

                var impacts = cultures.Select(x => CultureImpact.Explicit(x, IsDefaultCulture(allLangs, x)));

                // publish the culture(s)
                // we don't care about the response here, this response will be rechecked below but we need to set the culture info values now.
                foreach (var impact in impacts)
                    content.PublishCulture(impact);

                var result = CommitDocumentChangesInternal(scope, content, saveEventArgs, allLangs, userId, raiseEvents);
                scope.Complete();
                return result;
            }
        }

        /// <inheritdoc />
        public PublishResult Unpublish(IContent content, string culture = "*", int userId = Constants.Security.SuperUserId)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var evtMsgs = EventMessagesFactory.Get();

            culture = culture.NullOrWhiteSpaceAsNull();

            var publishedState = content.PublishedState;
            if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
                throw new InvalidOperationException($"Cannot save-and-publish (un)publishing content, use the dedicated {nameof(CommitDocumentChanges)} method.");

            // cannot accept invariant (null or empty) culture for variant content type
            // cannot accept a specific culture for invariant content type (but '*' is ok)
            if (content.ContentType.VariesByCulture())
            {
                if (culture == null)
                    throw new NotSupportedException("Invariant culture is not supported by variant content types.");
            }
            else
            {
                if (culture != null && culture != "*")
                    throw new NotSupportedException($"Culture \"{culture}\" is not supported by invariant content types.");
            }

            // if the content is not published, nothing to do
            if (!content.Published)
                return new PublishResult(PublishResultType.SuccessUnpublishAlready, evtMsgs, content);

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                var allLangs = _languageRepository.GetMany().ToList();

                var saveEventArgs = new ContentSavingEventArgs(content, evtMsgs);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs, nameof(Saving)))
                    return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, content);

                // all cultures = unpublish whole
                if (culture == "*" || (!content.ContentType.VariesByCulture() && culture == null))
                {
                    // It's important to understand that when the document varies by culture but the "*" is used,
                    // we are just unpublishing the whole document but leaving all of the culture's as-is. This is expected
                    // because we don't want to actually unpublish every culture and then the document, we just want everything
                    // to be non-routable so that when it's re-published all variants were as they were.

                    content.PublishedState = PublishedState.Unpublishing;
                    var result = CommitDocumentChangesInternal(scope, content, saveEventArgs, allLangs, userId);
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

                    //save and publish any changes
                    var result = CommitDocumentChangesInternal(scope, content, saveEventArgs, allLangs, userId);

                    scope.Complete();

                    // In one case the result will be PublishStatusType.FailedPublishNothingToPublish which means that no cultures
                    // were specified to be published which will be the case when removed is false. In that case
                    // we want to swap the result type to PublishResultType.SuccessUnpublishAlready (that was the expectation before).
                    if (result.Result == PublishResultType.FailedPublishNothingToPublish && !removed)
                        return new PublishResult(PublishResultType.SuccessUnpublishAlready, evtMsgs, content);

                    return result;
                }
            }
        }

        /// <summary>
        /// Saves a document and publishes/unpublishes any pending publishing changes made to the document.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This MUST NOT be called from within this service, this used to be a public API and must only be used outside of this service.
        /// Internally in this service, calls must be made to CommitDocumentChangesInternal
        /// </para>
        ///
        /// <para>This is the underlying logic for both publishing and unpublishing any document</para>
        /// <para>Pending publishing/unpublishing changes on a document are made with calls to <see cref="ContentRepositoryExtensions.PublishCulture"/> and
        /// <see cref="ContentRepositoryExtensions.UnpublishCulture"/>.</para>
        /// <para>When publishing or unpublishing a single culture, or all cultures, use <see cref="SaveAndPublish"/>
        /// and <see cref="Unpublish"/>. But if the flexibility to both publish and unpublish in a single operation is required
        /// then this method needs to be used in combination with <see cref="ContentRepositoryExtensions.PublishCulture"/> and <see cref="ContentRepositoryExtensions.UnpublishCulture"/>
        /// on the content itself - this prepares the content, but does not commit anything - and then, invoke
        /// <see cref="CommitDocumentChanges"/> to actually commit the changes to the database.</para>
        /// <para>The document is *always* saved, even when publishing fails.</para>
        /// </remarks>
        internal PublishResult CommitDocumentChanges(IContent content,
            int userId = Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var evtMsgs = EventMessagesFactory.Get();

                scope.WriteLock(Constants.Locks.ContentTree);

                var saveEventArgs = new ContentSavingEventArgs(content, evtMsgs);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs, nameof(Saving)))
                    return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, content);

                var allLangs = _languageRepository.GetMany().ToList();

                var result = CommitDocumentChangesInternal(scope, content, saveEventArgs, allLangs, userId, raiseEvents);
                scope.Complete();
                return result;
            }
        }

        /// <summary>
        /// Handles a lot of business logic cases for how the document should be persisted
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="content"></param>
        /// <param name="saveEventArgs"></param>
        /// <param name="userId"></param>
        /// <param name="raiseEvents"></param>
        /// <param name="branchOne"></param>
        /// <param name="branchRoot"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Business logic cases such: as unpublishing a mandatory culture, or unpublishing the last culture, checking for pending scheduled publishing, etc... is dealt with in this method.
        /// There is quite a lot of cases to take into account along with logic that needs to deal with scheduled saving/publishing, branch saving/publishing, etc...
        /// </para>
        /// </remarks>
        private PublishResult CommitDocumentChangesInternal(IScope scope, IContent content,
            ContentSavingEventArgs saveEventArgs, IReadOnlyCollection<ILanguage> allLangs,
            int userId = Constants.Security.SuperUserId,
            bool raiseEvents = true, bool branchOne = false, bool branchRoot = false)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (saveEventArgs == null) throw new ArgumentNullException(nameof(saveEventArgs));

            var evtMsgs = saveEventArgs.Messages;

            PublishResult publishResult = null;
            PublishResult unpublishResult = null;

            // nothing set = republish it all
            if (content.PublishedState != PublishedState.Publishing && content.PublishedState != PublishedState.Unpublishing)
                content.PublishedState = PublishedState.Publishing;

            // State here is either Publishing or Unpublishing
            // Publishing to unpublish a culture may end up unpublishing everything so these flags can be flipped later
            var publishing = content.PublishedState == PublishedState.Publishing;
            var unpublishing = content.PublishedState == PublishedState.Unpublishing;

            var variesByCulture = content.ContentType.VariesByCulture();

            //track cultures that are being published, changed, unpublished
            IReadOnlyList<string> culturesPublishing = null;
            IReadOnlyList<string> culturesUnpublishing = null;
            IReadOnlyList<string> culturesChanging = variesByCulture
                ? content.CultureInfos.Values.Where(x => x.IsDirty()).Select(x => x.Culture).ToList()
                : null;

            var isNew = !content.HasIdentity;
            var changeType = isNew ? TreeChangeTypes.RefreshNode : TreeChangeTypes.RefreshBranch;
            var previouslyPublished = content.HasIdentity && content.Published;

            //inline method to persist the document with the documentRepository since this logic could be called a couple times below
            void SaveDocument(IContent c)
            {
                // save, always
                if (c.HasIdentity == false)
                    c.CreatorId = userId;
                c.WriterId = userId;

                // saving does NOT change the published version, unless PublishedState is Publishing or Unpublishing
                _documentRepository.Save(c);
            }

            if (publishing)
            {
                //determine cultures publishing/unpublishing which will be based on previous calls to content.PublishCulture and ClearPublishInfo
                culturesUnpublishing = content.GetCulturesUnpublishing();
                culturesPublishing = variesByCulture
                        ? content.PublishCultureInfos.Values.Where(x => x.IsDirty()).Select(x => x.Culture).ToList()
                        : null;

                // ensure that the document can be published, and publish handling events, business rules, etc
                publishResult = StrategyCanPublish(scope, content, /*checkPath:*/ (!branchOne || branchRoot), culturesPublishing, culturesUnpublishing, evtMsgs, saveEventArgs, allLangs);
                if (publishResult.Success)
                {
                    // note: StrategyPublish flips the PublishedState to Publishing!
                    publishResult = StrategyPublish(content, culturesPublishing, culturesUnpublishing, evtMsgs);

                    //check if a culture has been unpublished and if there are no cultures left, and then unpublish document as a whole
                    if (publishResult.Result == PublishResultType.SuccessUnpublishCulture && content.PublishCultureInfos.Count == 0)
                    {
                        // This is a special case! We are unpublishing the last culture and to persist that we need to re-publish without any cultures
                        // so the state needs to remain Publishing to do that. However, we then also need to unpublish the document and to do that
                        // the state needs to be Unpublishing and it cannot be both. This state is used within the documentRepository to know how to
                        // persist certain things. So before proceeding below, we need to save the Publishing state to publish no cultures, then we can
                        // mark the document for Unpublishing.
                        SaveDocument(content);

                        //set the flag to unpublish and continue
                        unpublishing = content.Published; // if not published yet, nothing to do
                    }
                }
                else
                {
                    // in a branch, just give up
                    if (branchOne && !branchRoot)
                        return publishResult;

                    //check for mandatory culture missing, and then unpublish document as a whole
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

            if (unpublishing) // won't happen in a branch
            {
                var newest = GetById(content.Id); // ensure we have the newest version - in scope
                if (content.VersionId != newest.VersionId)
                    return new PublishResult(PublishResultType.FailedPublishConcurrencyViolation, evtMsgs, content);

                if (content.Published)
                {
                    // ensure that the document can be unpublished, and unpublish
                    // handling events, business rules, etc
                    // note: StrategyUnpublish flips the PublishedState to Unpublishing!
                    // note: This unpublishes the entire document (not different variants)
                    unpublishResult = StrategyCanUnpublish(scope, content, evtMsgs);
                    if (unpublishResult.Success)
                        unpublishResult = StrategyUnpublish(content, evtMsgs);
                    else
                    {
                        // reset published state from temp values (publishing, unpublishing) to original value
                        // (published, unpublished) in order to save the document, unchanged - yes, this is odd,
                        // but: (a) it means we don't reproduce the PublishState logic here and (b) setting the
                        // PublishState to anything other than Publishing or Unpublishing - which is precisely
                        // what we want to do here - throws
                        content.Published = content.Published;
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

            //Persist the document
            SaveDocument(content);

            // raise the Saved event, always
            if (raiseEvents)
            {
                scope.Events.Dispatch(Saved, this, saveEventArgs.ToContentSavedEventArgs(), nameof(Saved));
            }

            if (unpublishing) // we have tried to unpublish - won't happen in a branch
            {
                if (unpublishResult.Success) // and succeeded, trigger events
                {
                    // events and audit
                    scope.Events.Dispatch(Unpublished, this, new PublishEventArgs<IContent>(content, false, false), "Unpublished");
                    scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, TreeChangeTypes.RefreshBranch).ToEventArgs());

                    if (culturesUnpublishing != null)
                    {
                        // This will mean that that we unpublished a mandatory culture or we unpublished the last culture.

                        var langs = string.Join(", ", allLangs
                                    .Where(x => culturesUnpublishing.InvariantContains(x.IsoCode))
                                    .Select(x => x.CultureName));
                        Audit(AuditType.UnpublishVariant, userId, content.Id, $"Unpublished languages: {langs}", langs);

                        if (publishResult == null)
                            throw new PanicException("publishResult == null - should not happen");

                        switch (publishResult.Result)
                        {
                            case PublishResultType.FailedPublishMandatoryCultureMissing:
                                //occurs when a mandatory culture was unpublished (which means we tried publishing the document without a mandatory culture)

                                //log that the whole content item has been unpublished due to mandatory culture unpublished
                                Audit(AuditType.Unpublish, userId, content.Id, "Unpublished (mandatory language unpublished)");
                                return new PublishResult(PublishResultType.SuccessUnpublishMandatoryCulture, evtMsgs, content);
                            case PublishResultType.SuccessUnpublishCulture:
                                //occurs when the last culture is unpublished

                                Audit(AuditType.Unpublish, userId, content.Id, "Unpublished (last language unpublished)");
                                return new PublishResult(PublishResultType.SuccessUnpublishLastCulture, evtMsgs, content);
                        }

                    }

                    Audit(AuditType.Unpublish, userId, content.Id);
                    return new PublishResult(PublishResultType.SuccessUnpublish, evtMsgs, content);
                }

                // or, failed
                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, changeType).ToEventArgs());
                return new PublishResult(PublishResultType.FailedUnpublish, evtMsgs, content); // bah
            }

            if (publishing) // we have tried to publish
            {
                if (publishResult.Success) // and succeeded, trigger events
                {
                    if (isNew == false && previouslyPublished == false)
                        changeType = TreeChangeTypes.RefreshBranch; // whole branch
                    else if (isNew == false && previouslyPublished)
                        changeType = TreeChangeTypes.RefreshNode; // single node


                    // invalidate the node/branch
                    if (!branchOne) // for branches, handled by SaveAndPublishBranch
                    {
                        scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, changeType).ToEventArgs());
                        scope.Events.Dispatch(Published, this, saveEventArgs.ToContentPublishedEventArgs(), nameof(Published));
                    }

                    // it was not published and now is... descendants that were 'published' (but
                    // had an unpublished ancestor) are 're-published' ie not explicitly published
                    // but back as 'published' nevertheless
                    if (!branchOne && isNew == false && previouslyPublished == false && HasChildren(content.Id))
                    {
                        var descendants = GetPublishedDescendantsLocked(content).ToArray();
                        scope.Events.Dispatch(Published, this, new ContentPublishedEventArgs(descendants, false, evtMsgs), "Published");
                    }

                    switch (publishResult.Result)
                    {
                        case PublishResultType.SuccessPublish:
                            Audit(AuditType.Publish, userId, content.Id);
                            break;
                        case PublishResultType.SuccessPublishCulture:
                            if (culturesPublishing != null)
                            {
                                var langs = string.Join(", ", allLangs
                                    .Where(x => culturesPublishing.InvariantContains(x.IsoCode))
                                    .Select(x => x.CultureName));
                                Audit(AuditType.PublishVariant, userId, content.Id, $"Published languages: {langs}", langs);
                            }
                            break;
                        case PublishResultType.SuccessUnpublishCulture:
                            if (culturesUnpublishing != null)
                            {
                                var langs = string.Join(", ", allLangs
                                   .Where(x => culturesUnpublishing.InvariantContains(x.IsoCode))
                                   .Select(x => x.CultureName));
                                Audit(AuditType.UnpublishVariant, userId, content.Id, $"Unpublished languages: {langs}", langs);
                            }
                            break;
                    }

                    return publishResult;
                }
            }

            // should not happen
            if (branchOne && !branchRoot)
                throw new PanicException("branchOne && !branchRoot - should not happen");

            //if publishing didn't happen or if it has failed, we still need to log which cultures were saved
            if (!branchOne && (publishResult == null || !publishResult.Success))
            {
                if (culturesChanging != null)
                {
                    var langs = string.Join(", ", allLangs
                        .Where(x => culturesChanging.InvariantContains(x.IsoCode))
                        .Select(x => x.CultureName));
                    Audit(AuditType.SaveVariant, userId, content.Id, $"Saved languages: {langs}", langs);
                }
                else
                {
                    Audit(AuditType.Save, userId, content.Id);
                }
            }

            // or, failed
            scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, changeType).ToEventArgs());
            return publishResult;
        }

        /// <inheritdoc />
        public IEnumerable<PublishResult> PerformScheduledPublish(DateTime date)
        {
            var allLangs = new Lazy<List<ILanguage>>(() => _languageRepository.GetMany().ToList());
            var evtMsgs = EventMessagesFactory.Get();
            var results = new List<PublishResult>();

            PerformScheduledPublishingRelease(date, results, evtMsgs, allLangs);
            PerformScheduledPublishingExpiration(date, results, evtMsgs, allLangs);

            return results;
        }

        private void PerformScheduledPublishingExpiration(DateTime date, List<PublishResult> results, EventMessages evtMsgs, Lazy<List<ILanguage>> allLangs)
        {
            using var scope = ScopeProvider.CreateScope();

            // do a fast read without any locks since this executes often to see if we even need to proceed
            if (_documentRepository.HasContentForExpiration(date))
            {
                // now take a write lock since we'll be updating
                scope.WriteLock(Constants.Locks.ContentTree);

                foreach (var d in _documentRepository.GetContentForExpiration(date))
                {
                    if (d.ContentType.VariesByCulture())
                    {
                        //find which cultures have pending schedules
                        var pendingCultures = d.ContentSchedule.GetPending(ContentScheduleAction.Expire, date)
                            .Select(x => x.Culture)
                            .Distinct()
                            .ToList();

                        if (pendingCultures.Count == 0)
                            continue; //shouldn't happen but no point in processing this document if there's nothing there

                        var saveEventArgs = new ContentSavingEventArgs(d, evtMsgs);
                        if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs, nameof(Saving)))
                        {
                            results.Add(new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, d));
                            continue;
                        }

                        foreach (var c in pendingCultures)
                        {
                            //Clear this schedule for this culture
                            d.ContentSchedule.Clear(c, ContentScheduleAction.Expire, date);
                            //set the culture to be published
                            d.UnpublishCulture(c);
                        }

                        var result = CommitDocumentChangesInternal(scope, d, saveEventArgs, allLangs.Value, d.WriterId);
                        if (result.Success == false)
                            Logger.Error<ContentService, int, PublishResultType>(null, "Failed to publish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);
                        results.Add(result);

                    }
                    else
                    {
                        //Clear this schedule
                        d.ContentSchedule.Clear(ContentScheduleAction.Expire, date);
                        var result = Unpublish(d, userId: d.WriterId);
                        if (result.Success == false)
                            Logger.Error<ContentService, int, PublishResultType>(null, "Failed to unpublish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);
                        results.Add(result);
                    }
                }

                _documentRepository.ClearSchedule(date, ContentScheduleAction.Expire);
            }

            scope.Complete();
        }

        private void PerformScheduledPublishingRelease(DateTime date, List<PublishResult> results, EventMessages evtMsgs, Lazy<List<ILanguage>> allLangs)
        {
            using var scope = ScopeProvider.CreateScope();

            // do a fast read without any locks since this executes often to see if we even need to proceed
            if (_documentRepository.HasContentForRelease(date))
            {
                // now take a write lock since we'll be updating
                scope.WriteLock(Constants.Locks.ContentTree);

                foreach (var d in _documentRepository.GetContentForRelease(date))
                {
                    if (d.ContentType.VariesByCulture())
                    {
                        //find which cultures have pending schedules
                        var pendingCultures = d.ContentSchedule.GetPending(ContentScheduleAction.Release, date)
                            .Select(x => x.Culture)
                            .Distinct()
                            .ToList();

                        if (pendingCultures.Count == 0)
                            continue; //shouldn't happen but no point in processing this document if there's nothing there

                        var saveEventArgs = new ContentSavingEventArgs(d, evtMsgs);
                        if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs, nameof(Saving)))
                        {
                            results.Add(new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, d));
                            continue; // this document is canceled move next
                        }

                        var publishing = true;
                        foreach (var culture in pendingCultures)
                        {
                            //Clear this schedule for this culture
                            d.ContentSchedule.Clear(culture, ContentScheduleAction.Release, date);

                            if (d.Trashed) continue; // won't publish

                            //publish the culture values and validate the property values, if validation fails, log the invalid properties so the develeper has an idea of what has failed
                            Property[] invalidProperties = null;
                            var impact = CultureImpact.Explicit(culture, IsDefaultCulture(allLangs.Value, culture));
                            var tryPublish = d.PublishCulture(impact) && _propertyValidationService.Value.IsPropertyDataValid(d, out invalidProperties, impact);
                            if (invalidProperties != null && invalidProperties.Length > 0)
                                Logger.Warn<ContentService, int, string, string>("Scheduled publishing will fail for document {DocumentId} and culture {Culture} because of invalid properties {InvalidProperties}",
                                    d.Id, culture, string.Join(",", invalidProperties.Select(x => x.Alias)));

                            publishing &= tryPublish; //set the culture to be published
                            if (!publishing) continue; // move to next document
                        }

                        PublishResult result;

                        if (d.Trashed)
                            result = new PublishResult(PublishResultType.FailedPublishIsTrashed, evtMsgs, d);
                        else if (!publishing)
                            result = new PublishResult(PublishResultType.FailedPublishContentInvalid, evtMsgs, d);
                        else
                            result = CommitDocumentChangesInternal(scope, d, saveEventArgs, allLangs.Value, d.WriterId);

                        if (result.Success == false)
                            Logger.Error<ContentService, int, PublishResultType>(null, "Failed to publish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);

                        results.Add(result);
                    }
                    else
                    {
                        //Clear this schedule
                        d.ContentSchedule.Clear(ContentScheduleAction.Release, date);

                        var result = d.Trashed
                            ? new PublishResult(PublishResultType.FailedPublishIsTrashed, evtMsgs, d)
                            : SaveAndPublish(d, userId: d.WriterId);

                        if (result.Success == false)
                            Logger.Error<ContentService, int, PublishResultType>(null, "Failed to publish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);

                        results.Add(result);
                    }
                }

                _documentRepository.ClearSchedule(date, ContentScheduleAction.Release);

            }

            scope.Complete();
        }

        // utility 'PublishCultures' func used by SaveAndPublishBranch
        private bool SaveAndPublishBranch_PublishCultures(IContent content, HashSet<string> culturesToPublish, IReadOnlyCollection<ILanguage> allLangs)
        {
            //TODO: This does not support being able to return invalid property details to bubble up to the UI

            // variant content type - publish specified cultures
            // invariant content type - publish only the invariant culture
            if (content.ContentType.VariesByCulture())
            {
                return culturesToPublish.All(culture =>
                {
                    var impact = CultureImpact.Create(culture, IsDefaultCulture(allLangs, culture), content);
                    return content.PublishCulture(impact) && _propertyValidationService.Value.IsPropertyDataValid(content, out _, impact);
                });
            }

            return content.PublishCulture(CultureImpact.Invariant)
                   && _propertyValidationService.Value.IsPropertyDataValid(content, out _, CultureImpact.Invariant);
        }

        // utility 'ShouldPublish' func used by SaveAndPublishBranch
        private HashSet<string> SaveAndPublishBranch_ShouldPublish(ref HashSet<string> cultures, string c, bool published, bool edited, bool isRoot, bool force)
        {
            // if published, republish
            if (published)
            {
                if (cultures == null) cultures = new HashSet<string>(); // empty means 'already published'
                if (edited) cultures.Add(c); // <culture> means 'republish this culture'
                return cultures;
            }

            // if not published, publish if force/root else do nothing
            if (!force && !isRoot) return cultures; // null means 'nothing to do'

            if (cultures == null) cultures = new HashSet<string>();
            cultures.Add(c); // <culture> means 'publish this culture'
            return cultures;
        }

        /// <inheritdoc />
        public IEnumerable<PublishResult> SaveAndPublishBranch(IContent content, bool force, string culture = "*", int userId = Constants.Security.SuperUserId)
        {
            // note: EditedValue and PublishedValue are objects here, so it is important to .Equals()
            // and not to == them, else we would be comparing references, and that is a bad thing

            // determines whether the document is edited, and thus needs to be published,
            // for the specified culture (it may be edited for other cultures and that
            // should not trigger a publish).

            // determines cultures to be published
            // can be: null (content is not impacted), an empty set (content is impacted but already published), or cultures
            HashSet<string> ShouldPublish(IContent c)
            {
                var isRoot = c.Id == content.Id;
                HashSet<string> culturesToPublish = null;

                if (!c.ContentType.VariesByCulture()) // invariant content type
                    return SaveAndPublishBranch_ShouldPublish(ref culturesToPublish, "*", c.Published, c.Edited, isRoot, force);

                if (culture != "*") // variant content type, specific culture
                    return SaveAndPublishBranch_ShouldPublish(ref culturesToPublish, culture, c.IsCulturePublished(culture), c.IsCultureEdited(culture), isRoot, force);

                // variant content type, all cultures
                if (c.Published)
                {
                    // then some (and maybe all) cultures will be 'already published' (unless forcing),
                    // others will have to 'republish this culture'
                    foreach (var x in c.AvailableCultures)
                        SaveAndPublishBranch_ShouldPublish(ref culturesToPublish, x, c.IsCulturePublished(x), c.IsCultureEdited(x), isRoot, force);
                    return culturesToPublish;
                }

                // if not published, publish if force/root else do nothing
                return force || isRoot
                    ? new HashSet<string> { "*" } // "*" means 'publish all'
                    : null; // null means 'nothing to do'
            }

            return SaveAndPublishBranch(content, force, ShouldPublish, SaveAndPublishBranch_PublishCultures, userId);
        }

        /// <inheritdoc />
        public IEnumerable<PublishResult> SaveAndPublishBranch(IContent content, bool force, string[] cultures, int userId = Constants.Security.SuperUserId)
        {
            // note: EditedValue and PublishedValue are objects here, so it is important to .Equals()
            // and not to == them, else we would be comparing references, and that is a bad thing

            cultures = cultures ?? Array.Empty<string>();

            // determines cultures to be published
            // can be: null (content is not impacted), an empty set (content is impacted but already published), or cultures
            HashSet<string> ShouldPublish(IContent c)
            {
                var isRoot = c.Id == content.Id;
                HashSet<string> culturesToPublish = null;

                if (!c.ContentType.VariesByCulture()) // invariant content type
                    return SaveAndPublishBranch_ShouldPublish(ref culturesToPublish, "*", c.Published, c.Edited, isRoot, force);

                // variant content type, specific cultures
                if (c.Published)
                {
                    // then some (and maybe all) cultures will be 'already published' (unless forcing),
                    // others will have to 'republish this culture'
                    foreach (var x in cultures)
                        SaveAndPublishBranch_ShouldPublish(ref culturesToPublish, x, c.IsCulturePublished(x), c.IsCultureEdited(x), isRoot, force);
                    return culturesToPublish;
                }

                // if not published, publish if force/root else do nothing
                return force || isRoot
                    ? new HashSet<string>(cultures) // means 'publish specified cultures'
                    : null; // null means 'nothing to do'
            }

            return SaveAndPublishBranch(content, force, ShouldPublish, SaveAndPublishBranch_PublishCultures, userId);
        }

        internal IEnumerable<PublishResult> SaveAndPublishBranch(IContent document, bool force,
            Func<IContent, HashSet<string>> shouldPublish,
            Func<IContent, HashSet<string>, IReadOnlyCollection<ILanguage>, bool> publishCultures,
            int userId = Constants.Security.SuperUserId)
        {
            if (shouldPublish == null) throw new ArgumentNullException(nameof(shouldPublish));
            if (publishCultures == null) throw new ArgumentNullException(nameof(publishCultures));

            var evtMsgs = EventMessagesFactory.Get();
            var results = new List<PublishResult>();
            var publishedDocuments = new List<IContent>();

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                var allLangs = _languageRepository.GetMany().ToList();

                if (!document.HasIdentity)
                    throw new InvalidOperationException("Cannot not branch-publish a new document.");

                var publishedState = document.PublishedState;
                if (publishedState == PublishedState.Publishing)
                    throw new InvalidOperationException("Cannot mix PublishCulture and SaveAndPublishBranch.");

                // deal with the branch root - if it fails, abort
                var result = SaveAndPublishBranchItem(scope, document, shouldPublish, publishCultures, true, publishedDocuments, evtMsgs, userId, allLangs);
                if (result != null)
                {
                    results.Add(result);
                    if (!result.Success) return results;
                }

                // deal with descendants
                // if one fails, abort its branch
                var exclude = new HashSet<int>();

                int count;
                var page = 0;
                const int pageSize = 100;
                do
                {
                    count = 0;
                    // important to order by Path ASC so make it explicit in case defaults change
                    // ReSharper disable once RedundantArgumentDefaultValue
                    foreach (var d in GetPagedDescendants(document.Id, page, pageSize, out _, ordering: Ordering.By("Path", Direction.Ascending)))
                    {
                        count++;

                        // if parent is excluded, exclude child too
                        if (exclude.Contains(d.ParentId))
                        {
                            exclude.Add(d.Id);
                            continue;
                        }

                        // no need to check path here, parent has to be published here
                        result = SaveAndPublishBranchItem(scope, d, shouldPublish, publishCultures, false, publishedDocuments, evtMsgs, userId, allLangs);
                        if (result != null)
                        {
                            results.Add(result);
                            if (result.Success) continue;
                        }

                        // if we could not publish the document, cut its branch
                        exclude.Add(d.Id);
                    }

                    page++;
                } while (count > 0);

                Audit(AuditType.Publish, userId, document.Id, "Branch published");

                // trigger events for the entire branch
                // (SaveAndPublishBranchOne does *not* do it)
                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(document, TreeChangeTypes.RefreshBranch).ToEventArgs());
                scope.Events.Dispatch(Published, this, new ContentPublishedEventArgs(publishedDocuments, false, evtMsgs), nameof(Published));

                scope.Complete();
            }

            return results;
        }

        // shouldPublish: a function determining whether the document has changes that need to be published
        //  note - 'force' is handled by 'editing'
        // publishValues: a function publishing values (using the appropriate PublishCulture calls)
        private PublishResult SaveAndPublishBranchItem(IScope scope, IContent document,
            Func<IContent, HashSet<string>> shouldPublish,
            Func<IContent, HashSet<string>, IReadOnlyCollection<ILanguage>, bool> publishCultures,
            bool isRoot,
            ICollection<IContent> publishedDocuments,
            EventMessages evtMsgs, int userId, IReadOnlyCollection<ILanguage> allLangs)
        {
            var culturesToPublish = shouldPublish(document);
            if (culturesToPublish == null) // null = do not include
                return null;
            if (culturesToPublish.Count == 0) // empty = already published
                return new PublishResult(PublishResultType.SuccessPublishAlready, evtMsgs, document);

            var saveEventArgs = new ContentSavingEventArgs(document, evtMsgs);
            if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs, nameof(Saving)))
                return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, document);

            // publish & check if values are valid
            if (!publishCultures(document, culturesToPublish, allLangs))
            {
                //TODO: Based on this callback behavior there is no way to know which properties may have been invalid if this failed, see other results of FailedPublishContentInvalid
                return new PublishResult(PublishResultType.FailedPublishContentInvalid, evtMsgs, document);
            }

            var result = CommitDocumentChangesInternal(scope, document, saveEventArgs, allLangs, userId, branchOne: true, branchRoot: isRoot);
            if (result.Success)
                publishedDocuments.Add(document);
            return result;
        }

        #endregion

        #region Delete

        /// <inheritdoc />
        public OperationResult Delete(IContent content, int userId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IContent>(content, evtMsgs);
                if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs, nameof(Deleting)))
                {
                    scope.Complete();
                    return OperationResult.Cancel(evtMsgs);
                }

                scope.WriteLock(Constants.Locks.ContentTree);

                // if it's not trashed yet, and published, we should unpublish
                // but... Unpublishing event makes no sense (not going to cancel?) and no need to save
                // just raise the event
                if (content.Trashed == false && content.Published)
                    scope.Events.Dispatch(Unpublished, this, new PublishEventArgs<IContent>(content, false, false), nameof(Unpublished));

                DeleteLocked(scope, content);

                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, TreeChangeTypes.Remove).ToEventArgs());
                Audit(AuditType.Delete, userId, content.Id);

                scope.Complete();
            }

            return OperationResult.Succeed(evtMsgs);
        }

        private void DeleteLocked(IScope scope, IContent content)
        {
            void DoDelete(IContent c)
            {
                _documentRepository.Delete(c);
                var args = new DeleteEventArgs<IContent>(c, false); // raise event & get flagged files
                scope.Events.Dispatch(Deleted, this, args, nameof(Deleted));

                // media files deleted by QueuingEventDispatcher
            }

            const int pageSize = 500;
            var total = long.MaxValue;
            while (total > 0)
            {
                //get descendants - ordered from deepest to shallowest
                var descendants = GetPagedDescendants(content.Id, 0, pageSize, out total, ordering: Ordering.By("Path", Direction.Descending));
                foreach (var c in descendants)
                    DoDelete(c);
            }
            DoDelete(content);
        }

        //TODO: both DeleteVersions methods below have an issue. Sort of. They do NOT take care of files the way
        // Delete does - for a good reason: the file may be referenced by other, non-deleted, versions. BUT,
        // if that's not the case, then the file will never be deleted, because when we delete the content,
        // the version referencing the file will not be there anymore. SO, we can leak files.

        /// <summary>
        /// Permanently deletes versions from an <see cref="IContent"/> object prior to a specific date.
        /// This method will never delete the latest version of a content item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> object to delete versions from</param>
        /// <param name="versionDate">Latest version date</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        public void DeleteVersions(int id, DateTime versionDate, int userId = Constants.Security.SuperUserId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteRevisionsEventArgs = new DeleteRevisionsEventArgs(id, dateToRetain: versionDate);
                if (scope.Events.DispatchCancelable(DeletingVersions, this, deleteRevisionsEventArgs))
                {
                    scope.Complete();
                    return;
                }

                scope.WriteLock(Constants.Locks.ContentTree);
                _documentRepository.DeleteVersions(id, versionDate);

                deleteRevisionsEventArgs.CanCancel = false;
                scope.Events.Dispatch(DeletedVersions, this, deleteRevisionsEventArgs);
                Audit(AuditType.Delete, userId, Constants.System.Root, "Delete (by version date)");

                scope.Complete();
            }
        }

        /// <summary>
        /// Permanently deletes specific version(s) from an <see cref="IContent"/> object.
        /// This method will never delete the latest version of a content item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> object to delete a version from</param>
        /// <param name="versionId">Id of the version to delete</param>
        /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        public void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                if (scope.Events.DispatchCancelable(DeletingVersions, this, new DeleteRevisionsEventArgs(id, /*specificVersion:*/ versionId)))
                {
                    scope.Complete();
                    return;
                }

                if (deletePriorVersions)
                {
                    var content = GetVersion(versionId);
                    DeleteVersions(id, content.UpdateDate, userId);
                }

                scope.WriteLock(Constants.Locks.ContentTree);
                var c = _documentRepository.Get(id);
                if (c.VersionId != versionId && c.PublishedVersionId != versionId) // don't delete the current or published version
                    _documentRepository.DeleteVersion(versionId);

                scope.Events.Dispatch(DeletedVersions, this, new DeleteRevisionsEventArgs(id, false,/* specificVersion:*/ versionId));
                Audit(AuditType.Delete, userId, Constants.System.Root, "Delete (by version)");

                scope.Complete();
            }
        }

        #endregion

        #region Move, RecycleBin

        /// <inheritdoc />
        public OperationResult MoveToRecycleBin(IContent content, int userId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var moves = new List<(IContent, string)>();

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                var originalPath = content.Path;
                var moveEventInfo = new MoveEventInfo<IContent>(content, originalPath, Constants.System.RecycleBinContent);
                var moveEventArgs = new MoveEventArgs<IContent>(evtMsgs, moveEventInfo);
                if (scope.Events.DispatchCancelable(Trashing, this, moveEventArgs, nameof(Trashing)))
                {
                    scope.Complete();
                    return OperationResult.Cancel(evtMsgs); // causes rollback
                }

                // if it's published we may want to force-unpublish it - that would be backward-compatible... but...
                // making a radical decision here: trashing is equivalent to moving under an unpublished node so
                // it's NOT unpublishing, only the content is now masked - allowing us to restore it if wanted
                //if (content.HasPublishedVersion)
                //{ }

                PerformMoveLocked(content, Constants.System.RecycleBinContent, null, userId, moves, true);
                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, TreeChangeTypes.RefreshBranch).ToEventArgs());

                var moveInfo = moves
                    .Select(x => new MoveEventInfo<IContent>(x.Item1, x.Item2, x.Item1.ParentId))
                    .ToArray();

                moveEventArgs.CanCancel = false;
                moveEventArgs.MoveInfoCollection = moveInfo;
                scope.Events.Dispatch(Trashed, this, moveEventArgs, nameof(Trashed));
                Audit(AuditType.Move, userId, content.Id, "Moved to recycle bin");

                scope.Complete();
            }

            return OperationResult.Succeed(evtMsgs);
        }

        /// <summary>
        /// Moves an <see cref="IContent"/> object to a new location by changing its parent id.
        /// </summary>
        /// <remarks>
        /// If the <see cref="IContent"/> object is already published it will be
        /// published after being moved to its new location. Otherwise it'll just
        /// be saved with a new parent id.
        /// </remarks>
        /// <param name="content">The <see cref="IContent"/> to move</param>
        /// <param name="parentId">Id of the Content's new Parent</param>
        /// <param name="userId">Optional Id of the User moving the Content</param>
        public void Move(IContent content, int parentId, int userId = Constants.Security.SuperUserId)
        {
            // if moving to the recycle bin then use the proper method
            if (parentId == Constants.System.RecycleBinContent)
            {
                MoveToRecycleBin(content, userId);
                return;
            }

            var moves = new List<(IContent, string)>();

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                var parent = parentId == Constants.System.Root ? null : GetById(parentId);
                if (parentId != Constants.System.Root && (parent == null || parent.Trashed))
                    throw new InvalidOperationException("Parent does not exist or is trashed."); // causes rollback

                var moveEventInfo = new MoveEventInfo<IContent>(content, content.Path, parentId);
                var moveEventArgs = new MoveEventArgs<IContent>(moveEventInfo);
                if (scope.Events.DispatchCancelable(Moving, this, moveEventArgs, nameof(Moving)))
                {
                    scope.Complete();
                    return; // causes rollback
                }

                // if content was trashed, and since we're not moving to the recycle bin,
                // indicate that the trashed status should be changed to false, else just
                // leave it unchanged
                var trashed = content.Trashed ? false : (bool?)null;

                // if the content was trashed under another content, and so has a published version,
                // it cannot move back as published but has to be unpublished first - that's for the
                // root content, everything underneath will retain its published status
                if (content.Trashed && content.Published)
                {
                    // however, it had been masked when being trashed, so there's no need for
                    // any special event here - just change its state
                    content.PublishedState = PublishedState.Unpublishing;
                }

                PerformMoveLocked(content, parentId, parent, userId, moves, trashed);

                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, TreeChangeTypes.RefreshBranch).ToEventArgs());

                var moveInfo = moves //changes
                    .Select(x => new MoveEventInfo<IContent>(x.Item1, x.Item2, x.Item1.ParentId))
                    .ToArray();

                moveEventArgs.MoveInfoCollection = moveInfo;
                moveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Moved, this, moveEventArgs, nameof(Moved));
                Audit(AuditType.Move, userId, content.Id);

                scope.Complete();
            }
        }

        // MUST be called from within WriteLock
        // trash indicates whether we are trashing, un-trashing, or not changing anything
        private void PerformMoveLocked(IContent content, int parentId, IContent parent, int userId,
            ICollection<(IContent, string)> moves,
            bool? trash)
        {
            content.WriterId = userId;
            content.ParentId = parentId;

            // get the level delta (old pos to new pos)
            // note that recycle bin (id:-20) level is 0!
            var levelDelta = 1 - content.Level + (parent?.Level ?? 0);

            var paths = new Dictionary<int, string>();

            moves.Add((content, content.Path)); // capture original path

            //need to store the original path to lookup descendants based on it below
            var originalPath = content.Path;

            // these will be updated by the repo because we changed parentId
            //content.Path = (parent == null ? "-1" : parent.Path) + "," + content.Id;
            //content.SortOrder = ((ContentRepository) repository).NextChildSortOrder(parentId);
            //content.Level += levelDelta;
            PerformMoveContentLocked(content, userId, trash);

            // if uow is not immediate, content.Path will be updated only when the UOW commits,
            // and because we want it now, we have to calculate it by ourselves
            //paths[content.Id] = content.Path;
            paths[content.Id] = (parent == null ? (parentId == Constants.System.RecycleBinContent ? "-1,-20" : Constants.System.RootString) : parent.Path) + "," + content.Id;

            const int pageSize = 500;
            var query = GetPagedDescendantQuery(originalPath);
            long total;
            do
            {
                // We always page a page 0 because for each page, we are moving the result so the resulting total will be reduced
                var descendants = GetPagedLocked(query, 0, pageSize, out total, null, Ordering.By("Path", Direction.Ascending));

                foreach (var descendant in descendants)
                {
                    moves.Add((descendant, descendant.Path)); // capture original path

                    // update path and level since we do not update parentId
                    descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
                    descendant.Level += levelDelta;
                    PerformMoveContentLocked(descendant, userId, trash);
                }

            } while (total > pageSize);

        }

        private void PerformMoveContentLocked(IContent content, int userId, bool? trash)
        {
            if (trash.HasValue) ((ContentBase)content).Trashed = trash.Value;
            content.WriterId = userId;
            _documentRepository.Save(content);
        }

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IContent"/> that resides in the bin
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use EmptyRecycleBin with explicit indication of user ID instead")]
        public OperationResult EmptyRecycleBin() => EmptyRecycleBin(Constants.Security.SuperUserId);

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IContent"/> that resides in the bin
        /// </summary>
        public OperationResult EmptyRecycleBin(int userId = Constants.Security.SuperUserId)
        {
            var nodeObjectType = Constants.ObjectTypes.Document;
            var deleted = new List<IContent>();
            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                // v7 EmptyingRecycleBin and EmptiedRecycleBin events are greatly simplified since
                // each deleted items will have its own deleting/deleted events. so, files and such
                // are managed by Delete, and not here.

                // no idea what those events are for, keep a simplified version
                var recycleBinEventArgs = new RecycleBinEventArgs(nodeObjectType, evtMsgs);
                if (scope.Events.DispatchCancelable(EmptyingRecycleBin, this, recycleBinEventArgs))
                {
                    scope.Complete();
                    return OperationResult.Cancel(evtMsgs);
                }

                // emptying the recycle bin means deleting whatever is in there - do it properly!
                var query = Query<IContent>().Where(x => x.ParentId == Constants.System.RecycleBinContent);
                var contents = _documentRepository.Get(query).ToArray();
                foreach (var content in contents)
                {
                    DeleteLocked(scope, content);
                    deleted.Add(content);
                }

                recycleBinEventArgs.CanCancel = false;
                recycleBinEventArgs.RecycleBinEmptiedSuccessfully = true; // oh my?!
                scope.Events.Dispatch(EmptiedRecycleBin, this, recycleBinEventArgs);
                scope.Events.Dispatch(TreeChanged, this, deleted.Select(x => new TreeChange<IContent>(x, TreeChangeTypes.Remove)).ToEventArgs());
                Audit(AuditType.Delete, userId, Constants.System.RecycleBinContent, "Recycle bin emptied");

                scope.Complete();
            }

            return OperationResult.Succeed(evtMsgs);
        }

        public bool RecycleBinSmells()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return _documentRepository.RecycleBinSmells();
            }
        }

        #endregion

        #region Others

        /// <summary>
        /// Copies an <see cref="IContent"/> object by creating a new Content object of the same type and copies all data from the current
        /// to the new copy which is returned. Recursively copies all children.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to copy</param>
        /// <param name="parentId">Id of the Content's new Parent</param>
        /// <param name="relateToOriginal">Boolean indicating whether the copy should be related to the original</param>
        /// <param name="userId">Optional Id of the User copying the Content</param>
        /// <returns>The newly created <see cref="IContent"/> object</returns>
        public IContent Copy(IContent content, int parentId, bool relateToOriginal, int userId = Constants.Security.SuperUserId)
        {
            return Copy(content, parentId, relateToOriginal, true, userId);
        }

        /// <summary>
        /// Copies an <see cref="IContent"/> object by creating a new Content object of the same type and copies all data from the current
        /// to the new copy which is returned.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to copy</param>
        /// <param name="parentId">Id of the Content's new Parent</param>
        /// <param name="relateToOriginal">Boolean indicating whether the copy should be related to the original</param>
        /// <param name="recursive">A value indicating whether to recursively copy children.</param>
        /// <param name="userId">Optional Id of the User copying the Content</param>
        /// <returns>The newly created <see cref="IContent"/> object</returns>
        public IContent Copy(IContent content, int parentId, bool relateToOriginal, bool recursive, int userId = Constants.Security.SuperUserId)
        {
            var copy = content.DeepCloneWithResetIdentities();
            copy.ParentId = parentId;

            using (var scope = ScopeProvider.CreateScope())
            {
                var copyEventArgs = new CopyEventArgs<IContent>(content, copy, true, parentId, relateToOriginal);
                if (scope.Events.DispatchCancelable(Copying, this, copyEventArgs))
                {
                    scope.Complete();
                    return null;
                }

                // note - relateToOriginal is not managed here,
                // it's just part of the Copied event args so the RelateOnCopyHandler knows what to do
                // meaning that the event has to trigger for every copied content including descendants

                var copies = new List<Tuple<IContent, IContent>>();

                scope.WriteLock(Constants.Locks.ContentTree);

                // a copy is not published (but not really unpublishing either)
                // update the create author and last edit author
                if (copy.Published)
                    copy.Published = false;
                copy.CreatorId = userId;
                copy.WriterId = userId;

                //get the current permissions, if there are any explicit ones they need to be copied
                var currentPermissions = GetPermissions(content);
                currentPermissions.RemoveWhere(p => p.IsDefaultPermissions);

                // save and flush because we need the ID for the recursive Copying events
                _documentRepository.Save(copy);

                //add permissions
                if (currentPermissions.Count > 0)
                {
                    var permissionSet = new ContentPermissionSet(copy, currentPermissions);
                    _documentRepository.AddOrUpdatePermissions(permissionSet);
                }

                // keep track of copies
                copies.Add(Tuple.Create(content, copy));
                var idmap = new Dictionary<int, int> { [content.Id] = copy.Id };

                if (recursive) // process descendants
                {
                    const int pageSize = 500;
                    var page = 0;
                    var total = long.MaxValue;
                    while (page * pageSize < total)
                    {
                        var descendants = GetPagedDescendants(content.Id, page++, pageSize, out total);
                        foreach (var descendant in descendants)
                        {
                            // if parent has not been copied, skip, else gets its copy id
                            if (idmap.TryGetValue(descendant.ParentId, out parentId) == false) continue;

                            var descendantCopy = descendant.DeepCloneWithResetIdentities();
                            descendantCopy.ParentId = parentId;

                            if (scope.Events.DispatchCancelable(Copying, this, new CopyEventArgs<IContent>(descendant, descendantCopy, parentId)))
                                continue;

                            // a copy is not published (but not really unpublishing either)
                            // update the create author and last edit author
                            if (descendantCopy.Published)
                                descendantCopy.Published = false;
                            descendantCopy.CreatorId = userId;
                            descendantCopy.WriterId = userId;

                            // save and flush (see above)
                            _documentRepository.Save(descendantCopy);

                            copies.Add(Tuple.Create(descendant, descendantCopy));
                            idmap[descendant.Id] = descendantCopy.Id;
                        }
                    }
                }

                // not handling tags here, because
                // - tags should be handled by the content repository
                // - a copy is unpublished and therefore has no impact on tags in DB

                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(copy, TreeChangeTypes.RefreshBranch).ToEventArgs());
                foreach (var x in copies)
                    scope.Events.Dispatch(Copied, this, new CopyEventArgs<IContent>(x.Item1, x.Item2, false, x.Item2.ParentId, relateToOriginal));
                Audit(AuditType.Copy, userId, content.Id);

                scope.Complete();
            }

            return copy;
        }

        /// <summary>
        /// Sends an <see cref="IContent"/> to Publication, which executes handlers and events for the 'Send to Publication' action.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to send to publication</param>
        /// <param name="userId">Optional Id of the User issuing the send to publication</param>
        /// <returns>True if sending publication was successful otherwise false</returns>
        public bool SendToPublication(IContent content, int userId = Constants.Security.SuperUserId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var sendToPublishEventArgs = new SendToPublishEventArgs<IContent>(content);
                if (scope.Events.DispatchCancelable(SendingToPublish, this, sendToPublishEventArgs))
                {
                    scope.Complete();
                    return false;
                }

                //track the cultures changing for auditing
                var culturesChanging = content.ContentType.VariesByCulture()
                    ? string.Join(",", content.CultureInfos.Values.Where(x => x.IsDirty()).Select(x => x.Culture))
                    : null;

                // TODO: Currently there's no way to change track which variant properties have changed, we only have change
                // tracking enabled on all values on the Property which doesn't allow us to know which variants have changed.
                // in this particular case, determining which cultures have changed works with the above with names since it will
                // have always changed if it's been saved in the back office but that's not really fail safe.

                //Save before raising event
                var saveResult = Save(content, userId);

                // always complete (but maybe return a failed status)
                scope.Complete();

                if (!saveResult.Success)
                    return saveResult.Success;

                sendToPublishEventArgs.CanCancel = false;
                scope.Events.Dispatch(SentToPublish, this, sendToPublishEventArgs);

                if (culturesChanging != null)
                    Audit(AuditType.SendToPublishVariant, userId, content.Id, $"Send To Publish for cultures: {culturesChanging}", culturesChanging);
                else
                    Audit(AuditType.SendToPublish, content.WriterId, content.Id);

                return saveResult.Success;
            }
        }

        /// <summary>
        /// Sorts a collection of <see cref="IContent"/> objects by updating the SortOrder according
        /// to the ordering of items in the passed in <paramref name="items"/>.
        /// </summary>
        /// <remarks>
        /// Using this method will ensure that the Published-state is maintained upon sorting
        /// so the cache is updated accordingly - as needed.
        /// </remarks>
        /// <param name="items"></param>
        /// <param name="userId"></param>
        /// <param name="raiseEvents"></param>
        /// <returns>Result indicating what action was taken when handling the command.</returns>
        public OperationResult Sort(IEnumerable<IContent> items, int userId = Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();

            var itemsA = items.ToArray();
            if (itemsA.Length == 0) return new OperationResult(OperationResultType.NoOperation, evtMsgs);

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                var ret = Sort(scope, itemsA, userId, evtMsgs, raiseEvents);
                scope.Complete();
                return ret;
            }
        }

        /// <summary>
        /// Sorts a collection of <see cref="IContent"/> objects by updating the SortOrder according
        /// to the ordering of items identified by the <paramref name="ids"/>.
        /// </summary>
        /// <remarks>
        /// Using this method will ensure that the Published-state is maintained upon sorting
        /// so the cache is updated accordingly - as needed.
        /// </remarks>
        /// <param name="ids"></param>
        /// <param name="userId"></param>
        /// <param name="raiseEvents"></param>
        /// <returns>Result indicating what action was taken when handling the command.</returns>
        public OperationResult Sort(IEnumerable<int> ids, int userId = Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();

            var idsA = ids.ToArray();
            if (idsA.Length == 0) return new OperationResult(OperationResultType.NoOperation, evtMsgs);

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);
                var itemsA = GetByIds(idsA).ToArray();

                var ret = Sort(scope, itemsA, userId, evtMsgs, raiseEvents);
                scope.Complete();
                return ret;
            }
        }

        private OperationResult Sort(IScope scope, IContent[] itemsA, int userId, EventMessages evtMsgs, bool raiseEvents)
        {
            var saveEventArgs = new ContentSavingEventArgs(itemsA, evtMsgs);
            if (raiseEvents)
            {
                //raise cancelable sorting event
                if (scope.Events.DispatchCancelable(Sorting, this, saveEventArgs, nameof(Sorting)))
                    return OperationResult.Cancel(evtMsgs);

                //raise saving event (this one cannot be canceled)
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saving, this, saveEventArgs, nameof(Saving));
            }

            var published = new List<IContent>();
            var saved = new List<IContent>();
            var sortOrder = 0;

            foreach (var content in itemsA)
            {
                // if the current sort order equals that of the content we don't
                // need to update it, so just increment the sort order and continue.
                if (content.SortOrder == sortOrder)
                {
                    sortOrder++;
                    continue;
                }

                // else update
                content.SortOrder = sortOrder++;
                content.WriterId = userId;

                // if it's published, register it, no point running StrategyPublish
                // since we're not really publishing it and it cannot be cancelled etc
                if (content.Published)
                    published.Add(content);

                // save
                saved.Add(content);
                _documentRepository.Save(content);
            }

            if (raiseEvents)
            {
                var savedEventsArgs = saveEventArgs.ToContentSavedEventArgs();
                //first saved, then sorted
                scope.Events.Dispatch(Saved, this, savedEventsArgs, nameof(Saved));
                scope.Events.Dispatch(Sorted, this, savedEventsArgs, nameof(Sorted));
            }

            scope.Events.Dispatch(TreeChanged, this, saved.Select(x => new TreeChange<IContent>(x, TreeChangeTypes.RefreshNode)).ToEventArgs());

            if (raiseEvents && published.Any())
                scope.Events.Dispatch(Published, this, new ContentPublishedEventArgs(published, false, evtMsgs), "Published");

            Audit(AuditType.Sort, userId, 0, "Sorting content performed by user");
            return OperationResult.Succeed(evtMsgs);
        }

        public ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                var report = _documentRepository.CheckDataIntegrity(options);

                if (report.FixedIssues.Count > 0)
                {
                    //The event args needs a content item so we'll make a fake one with enough properties to not cause a null ref
                    var root = new Content("root", -1, new ContentType(-1)) { Id = -1, Key = Guid.Empty };
                    scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>.EventArgs(new TreeChange<IContent>(root, TreeChangeTypes.RefreshAll)));
                }

                return report;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> descendants by the first Parent.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> item to retrieve Descendants from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        internal IEnumerable<IContent> GetPublishedDescendants(IContent content)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                return GetPublishedDescendantsLocked(content).ToArray(); // ToArray important in uow!
            }
        }

        internal IEnumerable<IContent> GetPublishedDescendantsLocked(IContent content)
        {
            var pathMatch = content.Path + ",";
            var query = Query<IContent>().Where(x => x.Id != content.Id && x.Path.StartsWith(pathMatch) /*&& x.Trashed == false*/);
            var contents = _documentRepository.Get(query);

            // beware! contents contains all published version below content
            // including those that are not directly published because below an unpublished content
            // these must be filtered out here

            var parents = new List<int> { content.Id };
            foreach (var c in contents)
            {
                if (parents.Contains(c.ParentId))
                {
                    yield return c;
                    parents.Add(c.Id);
                }
            }
        }

        #endregion

        #region Private Methods

        private void Audit(AuditType type, int userId, int objectId, string message = null, string parameters = null)
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId, ObjectTypes.GetName(UmbracoObjectTypes.Document), message, parameters));
        }

        private bool IsDefaultCulture(IReadOnlyCollection<ILanguage> langs, string culture) => langs.Any(x => x.IsDefault && x.IsoCode.InvariantEquals(culture));
        private bool IsMandatoryCulture(IReadOnlyCollection<ILanguage> langs, string culture) => langs.Any(x => x.IsMandatory && x.IsoCode.InvariantEquals(culture));

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IContentService, DeleteEventArgs<IContent>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IContentService, DeleteEventArgs<IContent>> Deleted;

        /// <summary>
        /// Occurs before Delete Versions
        /// </summary>
        public static event TypedEventHandler<IContentService, DeleteRevisionsEventArgs> DeletingVersions;

        /// <summary>
        /// Occurs after Delete Versions
        /// </summary>
        public static event TypedEventHandler<IContentService, DeleteRevisionsEventArgs> DeletedVersions;

        /// <summary>
        /// Occurs before Sorting
        /// </summary>
        public static event TypedEventHandler<IContentService, SaveEventArgs<IContent>> Sorting;

        /// <summary>
        /// Occurs after Sorting
        /// </summary>
        public static event TypedEventHandler<IContentService, SaveEventArgs<IContent>> Sorted;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IContentService, ContentSavingEventArgs> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IContentService, ContentSavedEventArgs> Saved;

        /// <summary>
        /// Occurs before Copy
        /// </summary>
        public static event TypedEventHandler<IContentService, CopyEventArgs<IContent>> Copying;

        /// <summary>
        /// Occurs after Copy
        /// </summary>
        public static event TypedEventHandler<IContentService, CopyEventArgs<IContent>> Copied;

        /// <summary>
        /// Occurs before Content is moved to Recycle Bin
        /// </summary>
        public static event TypedEventHandler<IContentService, MoveEventArgs<IContent>> Trashing;

        /// <summary>
        /// Occurs after Content is moved to Recycle Bin
        /// </summary>
        public static event TypedEventHandler<IContentService, MoveEventArgs<IContent>> Trashed;

        /// <summary>
        /// Occurs before Move
        /// </summary>
        public static event TypedEventHandler<IContentService, MoveEventArgs<IContent>> Moving;

        /// <summary>
        /// Occurs after Move
        /// </summary>
        public static event TypedEventHandler<IContentService, MoveEventArgs<IContent>> Moved;

        /// <summary>
        /// Occurs before Rollback
        /// </summary>
        public static event TypedEventHandler<IContentService, RollbackEventArgs<IContent>> RollingBack;

        /// <summary>
        /// Occurs after Rollback
        /// </summary>
        public static event TypedEventHandler<IContentService, RollbackEventArgs<IContent>> RolledBack;

        /// <summary>
        /// Occurs before Send to Publish
        /// </summary>
        public static event TypedEventHandler<IContentService, SendToPublishEventArgs<IContent>> SendingToPublish;

        /// <summary>
        /// Occurs after Send to Publish
        /// </summary>
        public static event TypedEventHandler<IContentService, SendToPublishEventArgs<IContent>> SentToPublish;

        /// <summary>
        /// Occurs before the Recycle Bin is emptied
        /// </summary>
        public static event TypedEventHandler<IContentService, RecycleBinEventArgs> EmptyingRecycleBin;

        /// <summary>
        /// Occurs after the Recycle Bin has been Emptied
        /// </summary>
        public static event TypedEventHandler<IContentService, RecycleBinEventArgs> EmptiedRecycleBin;

        /// <summary>
        /// Occurs before publish
        /// </summary>
        public static event TypedEventHandler<IContentService, ContentPublishingEventArgs> Publishing;

        /// <summary>
        /// Occurs after publish
        /// </summary>
        public static event TypedEventHandler<IContentService, ContentPublishedEventArgs> Published;

        /// <summary>
        /// Occurs before unpublish
        /// </summary>
        public static event TypedEventHandler<IContentService, PublishEventArgs<IContent>> Unpublishing;

        /// <summary>
        /// Occurs after unpublish
        /// </summary>
        public static event TypedEventHandler<IContentService, PublishEventArgs<IContent>> Unpublished;

        /// <summary>
        /// Occurs after change.
        /// </summary>
        public static event TypedEventHandler<IContentService, TreeChange<IContent>.EventArgs> TreeChanged;

        /// <summary>
        /// Occurs after a blueprint has been saved.
        /// </summary>
        public static event TypedEventHandler<IContentService, SaveEventArgs<IContent>> SavedBlueprint;

        /// <summary>
        /// Occurs after a blueprint has been deleted.
        /// </summary>
        public static event TypedEventHandler<IContentService, DeleteEventArgs<IContent>> DeletedBlueprint;

        #endregion

        #region Publishing Strategies

        /// <summary>
        /// Ensures that a document can be published
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="content"></param>
        /// <param name="checkPath"></param>
        /// <param name="culturesUnpublishing"></param>
        /// <param name="evtMsgs"></param>
        /// <param name="culturesPublishing"></param>
        /// <param name="savingEventArgs"></param>
        /// <returns></returns>
        private PublishResult StrategyCanPublish(IScope scope, IContent content, bool checkPath, IReadOnlyList<string> culturesPublishing,
            IReadOnlyCollection<string> culturesUnpublishing, EventMessages evtMsgs, ContentSavingEventArgs savingEventArgs,
            IReadOnlyCollection<ILanguage> allLangs)
        {
            // raise Publishing event
            if (scope.Events.DispatchCancelable(Publishing, this, savingEventArgs.ToContentPublishingEventArgs()))
            {
                Logger.Info<ContentService, string, int, string>("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "publishing was cancelled");
                return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, content);
            }

            var variesByCulture = content.ContentType.VariesByCulture();

            var impactsToPublish = culturesPublishing == null
                ? new[] { CultureImpact.Invariant } //if it's null it's invariant
                : culturesPublishing.Select(x => CultureImpact.Explicit(x, allLangs.Any(lang => lang.IsoCode.InvariantEquals(x) && lang.IsMandatory))).ToArray();

            // publish the culture(s)
            if (!impactsToPublish.All(content.PublishCulture))
                return new PublishResult(PublishResultType.FailedPublishContentInvalid, evtMsgs, content);

            //validate the property values
            Property[] invalidProperties = null;
            if (!impactsToPublish.All(x => _propertyValidationService.Value.IsPropertyDataValid(content, out invalidProperties, x)))
                return new PublishResult(PublishResultType.FailedPublishContentInvalid, evtMsgs, content)
                {
                    InvalidProperties = invalidProperties
                };

            //Check if mandatory languages fails, if this fails it will mean anything that the published flag on the document will
            // be changed to Unpublished and any culture currently published will not be visible.
            if (variesByCulture)
            {
                if (culturesPublishing == null)
                    throw new InvalidOperationException("Internal error, variesByCulture but culturesPublishing is null.");

                if (content.Published && culturesPublishing.Count == 0 && culturesUnpublishing.Count == 0)
                {
                    // no published cultures = cannot be published
                    // This will occur if for example, a culture that is already unpublished is sent to be unpublished again, or vice versa, in that case
                    // there will be nothing to publish/unpublish.
                    return new PublishResult(PublishResultType.FailedPublishNothingToPublish, evtMsgs, content);
                }


                // missing mandatory culture = cannot be published
                var mandatoryCultures = allLangs.Where(x => x.IsMandatory).Select(x => x.IsoCode);
                var mandatoryMissing = mandatoryCultures.Any(x => !content.PublishedCultures.Contains(x, StringComparer.OrdinalIgnoreCase));
                if (mandatoryMissing)
                    return new PublishResult(PublishResultType.FailedPublishMandatoryCultureMissing, evtMsgs, content);

                if (culturesPublishing.Count == 0 && culturesUnpublishing.Count > 0)
                    return new PublishResult(PublishResultType.SuccessUnpublishCulture, evtMsgs, content);
            }

            // ensure that the document has published values
            // either because it is 'publishing' or because it already has a published version
            if (content.PublishedState != PublishedState.Publishing && content.PublishedVersionId == 0)
            {
                Logger.Info<ContentService, string, int, string>("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "document does not have published values");
                return new PublishResult(PublishResultType.FailedPublishNothingToPublish, evtMsgs, content);
            }

            //loop over each culture publishing - or string.Empty for invariant
            foreach (var culture in culturesPublishing ?? (new[] { string.Empty }))
            {
                // ensure that the document status is correct
                // note: culture will be string.Empty for invariant
                switch (content.GetStatus(culture))
                {
                    case ContentStatus.Expired:
                        if (!variesByCulture)
                            Logger.Info<ContentService, string, int, string>("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "document has expired");
                        else
                            Logger.Info<ContentService>("Document {ContentName} (id={ContentId}) culture {Culture} cannot be published: {Reason}", content.Name, content.Id, culture, "document culture has expired");
                        return new PublishResult(!variesByCulture ? PublishResultType.FailedPublishHasExpired : PublishResultType.FailedPublishCultureHasExpired, evtMsgs, content);

                    case ContentStatus.AwaitingRelease:
                        if (!variesByCulture)
                            Logger.Info<ContentService, string, int, string>("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "document is awaiting release");
                        else
                            Logger.Info<ContentService>("Document {ContentName} (id={ContentId}) culture {Culture} cannot be published: {Reason}", content.Name, content.Id, culture, "document is culture awaiting release");
                        return new PublishResult(!variesByCulture ? PublishResultType.FailedPublishAwaitingRelease : PublishResultType.FailedPublishCultureAwaitingRelease, evtMsgs, content);

                    case ContentStatus.Trashed:
                        Logger.Info<ContentService, string, int, string>("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "document is trashed");
                        return new PublishResult(PublishResultType.FailedPublishIsTrashed, evtMsgs, content);
                }
            }

            if (checkPath)
            {
                // check if the content can be path-published
                // root content can be published
                // else check ancestors - we know we are not trashed
                var pathIsOk = content.ParentId == Constants.System.Root || IsPathPublished(GetParent(content));
                if (!pathIsOk)
                {
                    Logger.Info<ContentService, string, int, string>("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "parent is not published");
                    return new PublishResult(PublishResultType.FailedPublishPathNotPublished, evtMsgs, content);
                }
            }

            //If we are both publishing and unpublishing cultures, then return a mixed status
            if (variesByCulture && culturesPublishing.Count > 0 && culturesUnpublishing.Count > 0)
                return new PublishResult(PublishResultType.SuccessMixedCulture, evtMsgs, content);

            return new PublishResult(evtMsgs, content);
        }

        /// <summary>
        /// Publishes a document
        /// </summary>
        /// <param name="content"></param>
        /// <param name="culturesUnpublishing"></param>
        /// <param name="evtMsgs"></param>
        /// <param name="culturesPublishing"></param>
        /// <returns></returns>
        /// <remarks>
        /// It is assumed that all publishing checks have passed before calling this method like <see cref="StrategyCanPublish"/>
        /// </remarks>
        private PublishResult StrategyPublish(IContent content,
            IReadOnlyCollection<string> culturesPublishing, IReadOnlyCollection<string> culturesUnpublishing,
            EventMessages evtMsgs)
        {
            // change state to publishing
            content.PublishedState = PublishedState.Publishing;

            //if this is a variant then we need to log which cultures have been published/unpublished and return an appropriate result
            if (content.ContentType.VariesByCulture())
            {
                if (content.Published && culturesUnpublishing.Count == 0 && culturesPublishing.Count == 0)
                    return new PublishResult(PublishResultType.FailedPublishNothingToPublish, evtMsgs, content);

                if (culturesUnpublishing.Count > 0)
                    Logger.Info<ContentService, string, int, string>("Document {ContentName} (id={ContentId}) cultures: {Cultures} have been unpublished.",
                        content.Name, content.Id, string.Join(",", culturesUnpublishing));

                if (culturesPublishing.Count > 0)
                    Logger.Info<ContentService, string, int, string>("Document {ContentName} (id={ContentId}) cultures: {Cultures} have been published.",
                        content.Name, content.Id, string.Join(",", culturesPublishing));

                if (culturesUnpublishing.Count > 0 && culturesPublishing.Count > 0)
                    return new PublishResult(PublishResultType.SuccessMixedCulture, evtMsgs, content);

                if (culturesUnpublishing.Count > 0 && culturesPublishing.Count == 0)
                    return new PublishResult(PublishResultType.SuccessUnpublishCulture, evtMsgs, content);

                return new PublishResult(PublishResultType.SuccessPublishCulture, evtMsgs, content);
            }

            Logger.Info<ContentService, string, int>("Document {ContentName} (id={ContentId}) has been published.", content.Name, content.Id);
            return new PublishResult(evtMsgs, content);
        }

        /// <summary>
        /// Ensures that a document can be unpublished
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="content"></param>
        /// <param name="evtMsgs"></param>
        /// <returns></returns>
        private PublishResult StrategyCanUnpublish(IScope scope, IContent content, EventMessages evtMsgs)
        {
            // raise Unpublishing event
            if (scope.Events.DispatchCancelable(Unpublishing, this, new PublishEventArgs<IContent>(content, evtMsgs)))
            {
                Logger.Info<ContentService, string, int>("Document {ContentName} (id={ContentId}) cannot be unpublished: unpublishing was cancelled.", content.Name, content.Id);
                return new PublishResult(PublishResultType.FailedUnpublishCancelledByEvent, evtMsgs, content);
            }

            return new PublishResult(PublishResultType.SuccessUnpublish, evtMsgs, content);
        }

        /// <summary>
        /// Unpublishes a document
        /// </summary>
        /// <param name="content"></param>
        /// <param name="evtMsgs"></param>
        /// <returns></returns>
        /// <remarks>
        /// It is assumed that all unpublishing checks have passed before calling this method like <see cref="StrategyCanUnpublish"/>
        /// </remarks>
        private PublishResult StrategyUnpublish(IContent content, EventMessages evtMsgs)
        {
            var attempt = new PublishResult(PublishResultType.SuccessUnpublish, evtMsgs, content);

            //TODO: What is this check?? we just created this attempt and of course it is Success?!
            if (attempt.Success == false)
                return attempt;

            // if the document has any release dates set to before now,
            // they should be removed so they don't interrupt an unpublish
            // otherwise it would remain released == published

            var pastReleases = content.ContentSchedule.GetPending(ContentScheduleAction.Expire, DateTime.Now);
            foreach (var p in pastReleases)
                content.ContentSchedule.Remove(p);
            if (pastReleases.Count > 0)
                Logger.Info<ContentService, string, int>("Document {ContentName} (id={ContentId}) had its release date removed, because it was unpublished.", content.Name, content.Id);

            // change state to unpublishing
            content.PublishedState = PublishedState.Unpublishing;

            Logger.Info<ContentService, string, int>("Document {ContentName} (id={ContentId}) has been unpublished.", content.Name, content.Id);
            return attempt;
        }

        #endregion

        #region Content Types

        /// <summary>
        /// Deletes all content of specified type. All children of deleted content is moved to Recycle Bin.
        /// </summary>
        /// <remarks>
        /// <para>This needs extra care and attention as its potentially a dangerous and extensive operation.</para>
        /// <para>Deletes content items of the specified type, and only that type. Does *not* handle content types
        /// inheritance and compositions, which need to be managed outside of this method.</para>
        /// </remarks>
        /// <param name="contentTypeIds">Id of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional Id of the user issuing the delete operation</param>
        public void DeleteOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId)
        {
            // TODO: This currently this is called from the ContentTypeService but that needs to change,
            // if we are deleting a content type, we should just delete the data and do this operation slightly differently.
            // This method will recursively go lookup every content item, check if any of it's descendants are
            // of a different type, move them to the recycle bin, then permanently delete the content items.
            // The main problem with this is that for every content item being deleted, events are raised...
            // which we need for many things like keeping caches in sync, but we can surely do this MUCH better.

            var changes = new List<TreeChange<IContent>>();
            var moves = new List<(IContent, string)>();
            var contentTypeIdsA = contentTypeIds.ToArray();

            // using an immediate uow here because we keep making changes with
            // PerformMoveLocked and DeleteLocked that must be applied immediately,
            // no point queuing operations
            //
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                var query = Query<IContent>().WhereIn(x => x.ContentTypeId, contentTypeIdsA);
                var contents = _documentRepository.Get(query).ToArray();

                if (scope.Events.DispatchCancelable(Deleting, this, new DeleteEventArgs<IContent>(contents), nameof(Deleting)))
                {
                    scope.Complete();
                    return;
                }

                // order by level, descending, so deepest first - that way, we cannot move
                // a content of the deleted type, to the recycle bin (and then delete it...)
                foreach (var content in contents.OrderByDescending(x => x.ParentId))
                {
                    // if it's not trashed yet, and published, we should unpublish
                    // but... Unpublishing event makes no sense (not going to cancel?) and no need to save
                    // just raise the event
                    if (content.Trashed == false && content.Published)
                        scope.Events.Dispatch(Unpublished, this, new PublishEventArgs<IContent>(content, false, false), nameof(Unpublished));

                    // if current content has children, move them to trash
                    var c = content;
                    var childQuery = Query<IContent>().Where(x => x.ParentId == c.Id);
                    var children = _documentRepository.Get(childQuery);
                    foreach (var child in children)
                    {
                        // see MoveToRecycleBin
                        PerformMoveLocked(child, Constants.System.RecycleBinContent, null, userId, moves, true);
                        changes.Add(new TreeChange<IContent>(content, TreeChangeTypes.RefreshBranch));
                    }

                    // delete content
                    // triggers the deleted event (and handles the files)
                    DeleteLocked(scope, content);
                    changes.Add(new TreeChange<IContent>(content, TreeChangeTypes.Remove));
                }

                var moveInfos = moves
                    .Select(x => new MoveEventInfo<IContent>(x.Item1, x.Item2, x.Item1.ParentId))
                    .ToArray();
                if (moveInfos.Length > 0)
                    scope.Events.Dispatch(Trashed, this, new MoveEventArgs<IContent>(false, moveInfos), nameof(Trashed));
                scope.Events.Dispatch(TreeChanged, this, changes.ToEventArgs());

                Audit(AuditType.Delete, userId, Constants.System.Root, $"Delete content of type {string.Join(",", contentTypeIdsA)}");

                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes all content items of specified type. All children of deleted content item is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="contentTypeId">Id of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional id of the user deleting the media</param>
        public void DeleteOfType(int contentTypeId, int userId = Constants.Security.SuperUserId)
        {
            DeleteOfTypes(new[] { contentTypeId }, userId);
        }

        private IContentType GetContentType(IScope scope, string contentTypeAlias)
        {
            if (contentTypeAlias == null) throw new ArgumentNullException(nameof(contentTypeAlias));
            if (string.IsNullOrWhiteSpace(contentTypeAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(contentTypeAlias));

            scope.ReadLock(Constants.Locks.ContentTypes);

            var query = Query<IContentType>().Where(x => x.Alias == contentTypeAlias);
            var contentType = _contentTypeRepository.Get(query).FirstOrDefault();

            if (contentType == null)
                throw new Exception($"No ContentType matching the passed in Alias: '{contentTypeAlias}' was found"); // causes rollback

            return contentType;
        }

        private IContentType GetContentType(string contentTypeAlias)
        {
            if (contentTypeAlias == null) throw new ArgumentNullException(nameof(contentTypeAlias));
            if (string.IsNullOrWhiteSpace(contentTypeAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(contentTypeAlias));

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return GetContentType(scope, contentTypeAlias);
            }
        }

        #endregion

        #region Blueprints

        public IContent GetBlueprintById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var blueprint = _documentBlueprintRepository.Get(id);
                if (blueprint != null)
                    blueprint.Blueprint = true;
                return blueprint;
            }
        }

        public IContent GetBlueprintById(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var blueprint = _documentBlueprintRepository.Get(id);
                if (blueprint != null)
                    blueprint.Blueprint = true;
                return blueprint;
            }
        }

        public void SaveBlueprint(IContent content, int userId = Constants.Security.SuperUserId)
        {
            //always ensure the blueprint is at the root
            if (content.ParentId != -1)
                content.ParentId = -1;

            content.Blueprint = true;

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                if (content.HasIdentity == false)
                {
                    content.CreatorId = userId;
                }
                content.WriterId = userId;

                _documentBlueprintRepository.Save(content);

                Audit(AuditType.Save, Constants.Security.SuperUserId, content.Id, $"Saved content template: {content.Name}");

                scope.Events.Dispatch(SavedBlueprint, this, new SaveEventArgs<IContent>(content), "SavedBlueprint");

                scope.Complete();
            }
        }

        public void DeleteBlueprint(IContent content, int userId = Constants.Security.SuperUserId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);
                _documentBlueprintRepository.Delete(content);
                scope.Events.Dispatch(DeletedBlueprint, this, new DeleteEventArgs<IContent>(content), nameof(DeletedBlueprint));
                scope.Complete();
            }
        }

        private static readonly string[] ArrayOfOneNullString = { null };

        public IContent CreateContentFromBlueprint(IContent blueprint, string name, int userId = Constants.Security.SuperUserId)
        {
            if (blueprint == null) throw new ArgumentNullException(nameof(blueprint));

            var contentType = GetContentType(blueprint.ContentType.Alias);
            var content = new Content(name, -1, contentType);
            content.Path = string.Concat(content.ParentId.ToString(), ",", content.Id);

            content.CreatorId = userId;
            content.WriterId = userId;

            IEnumerable<string> cultures = ArrayOfOneNullString;
            if (blueprint.CultureInfos.Count > 0)
            {
                cultures = blueprint.CultureInfos.Values.Select(x => x.Culture);
                using (var scope = ScopeProvider.CreateScope())
                {
                    if (blueprint.CultureInfos.TryGetValue(_languageRepository.GetDefaultIsoCode(), out var defaultCulture))
                    {
                        defaultCulture.Name = name;
                    }

                    scope.Complete();
                }
            }

            var now = DateTime.Now;
            foreach (var culture in cultures)
            {
                foreach (var property in blueprint.Properties)
                {
                    var propertyCulture = property.PropertyType.VariesByCulture() ? culture : null;
                    content.SetValue(property.Alias, property.GetValue(propertyCulture), propertyCulture);
                }

                if (!string.IsNullOrEmpty(culture))
                {
                    content.SetCultureInfo(culture, blueprint.GetCultureName(culture), now);
                }
            }

            return content;
        }

        public IEnumerable<IContent> GetBlueprintsForContentTypes(params int[] contentTypeId)
        {
            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IContent>();
                if (contentTypeId.Length > 0)
                {
                    query.Where(x => contentTypeId.Contains(x.ContentTypeId));
                }
                return _documentBlueprintRepository.Get(query).Select(x =>
                {
                    x.Blueprint = true;
                    return x;
                });
            }
        }

        public void DeleteBlueprintsOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                var contentTypeIdsA = contentTypeIds.ToArray();
                var query = Query<IContent>();
                if (contentTypeIdsA.Length > 0)
                    query.Where(x => contentTypeIdsA.Contains(x.ContentTypeId));

                var blueprints = _documentBlueprintRepository.Get(query).Select(x =>
                {
                    x.Blueprint = true;
                    return x;
                }).ToArray();

                foreach (var blueprint in blueprints)
                {
                    _documentBlueprintRepository.Delete(blueprint);
                }

                scope.Events.Dispatch(DeletedBlueprint, this, new DeleteEventArgs<IContent>(blueprints), nameof(DeletedBlueprint));
                scope.Complete();
            }
        }

        public void DeleteBlueprintsOfType(int contentTypeId, int userId = Constants.Security.SuperUserId)
        {
            DeleteBlueprintsOfTypes(new[] { contentTypeId }, userId);
        }

        #endregion

        #region Rollback

        public OperationResult Rollback(int id, int versionId, string culture = "*", int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            //Get the current copy of the node
            var content = GetById(id);

            //Get the version
            var version = GetVersion(versionId);

            //Good ole null checks
            if (content == null || version == null || content.Trashed)
            {
                return new OperationResult(OperationResultType.FailedCannot, evtMsgs);
            }

            //Store the result of doing the save of content for the rollback
            OperationResult rollbackSaveResult;

            using (var scope = ScopeProvider.CreateScope())
            {
                var rollbackEventArgs = new RollbackEventArgs<IContent>(content);

                //Emit RollingBack event aka before
                if (scope.Events.DispatchCancelable(RollingBack, this, rollbackEventArgs))
                {
                    scope.Complete();
                    return OperationResult.Cancel(evtMsgs);
                }

                //Copy the changes from the version
                content.CopyFrom(version, culture);

                //Save the content for the rollback
                rollbackSaveResult = Save(content, userId);

                //Depending on the save result - is what we log & audit along with what we return
                if (rollbackSaveResult.Success == false)
                {
                    //Log the error/warning
                    Logger.Error<ContentService, int, int, int>("User '{UserId}' was unable to rollback content '{ContentId}' to version '{VersionId}'", userId, id, versionId);
                }
                else
                {
                    //Emit RolledBack event aka after
                    rollbackEventArgs.CanCancel = false;
                    scope.Events.Dispatch(RolledBack, this, rollbackEventArgs);

                    //Logging & Audit message
                    Logger.Info<ContentService, int, int, int>("User '{UserId}' rolled back content '{ContentId}' to version '{VersionId}'", userId, id, versionId);
                    Audit(AuditType.RollBack, userId, id, $"Content '{content.Name}' was rolled back to version '{versionId}'");
                }

                scope.Complete();
            }

            return rollbackSaveResult;
        }

        #endregion

        /// <inheritdoc />
        /// <remarks>
        /// In v9 this can live in another class as we publish the notifications via IEventAggregator.
        /// But for v8 must be here for access to the static events.
        /// </remarks>
        public IReadOnlyCollection<ContentVersionMeta> PerformContentVersionCleanup(DateTime asAtDate)
        {
            return CleanupDocumentVersions(asAtDate);
            // Media - ignored
            // Members - ignored
        }

        /// <inheritdoc />
        public IEnumerable<ContentVersionMeta> GetPagedContentVersions(int contentId, long pageIndex, int pageSize, out long totalRecords, string culture = null)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            // NOTE: v9 - don't service locate
            var documentVersionRepository = Composing.Current.Factory.GetInstance<IDocumentVersionRepository>();

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var languageId = _languageRepository.GetIdByIsoCode(culture, throwOnNotFound: true);
                scope.ReadLock(Constants.Locks.ContentTree);
                return documentVersionRepository.GetPagedItemsByContentId(contentId, pageIndex, pageSize, out totalRecords, languageId);
            }
        }

        /// <inheritdoc />
        public void SetPreventCleanup(int versionId, bool preventCleanup, int userId = -1)
        {
            // NOTE: v9 - don't service locate
            var documentVersionRepository = Composing.Current.Factory.GetInstance<IDocumentVersionRepository>();

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.WriteLock(Constants.Locks.ContentTree);
                documentVersionRepository.SetPreventCleanup(versionId, preventCleanup);

                var version = documentVersionRepository.Get(versionId);

                var auditType = preventCleanup
                    ? AuditType.ContentVersionPreventCleanup
                    : AuditType.ContentVersionEnableCleanup;

                var message = $"set preventCleanup = '{preventCleanup}' for version '{versionId}'";

                Audit(auditType, userId, version.ContentId, message, $"{version.VersionDate}");
            }
        }

        /// <remarks>
        /// v9 - move to another class
        /// </remarks>
        private IReadOnlyCollection<ContentVersionMeta> CleanupDocumentVersions(DateTime asAtDate)
        {
            // NOTE: v9 - don't service locate
            var documentVersionRepository = Composing.Current.Factory.GetInstance<IDocumentVersionRepository>();

            // NOTE: v9 - don't service locate
            var cleanupPolicy = Composing.Current.Factory.GetInstance<IContentVersionCleanupPolicy>();

            List<ContentVersionMeta> versionsToDelete;

            /* Why so many scopes?
             *
             * We could just work out the set to delete at SQL infra level which was the original plan, however we agreed that really we should fire
             * ContentService.DeletingVersions so people can hook & cancel if required.
             *
             * On first time run of cleanup on a site with a lot of history there may be a lot of historic ContentVersions to remove e.g. 200K for our.umbraco.com.
             * If we weren't supporting SQL CE we could do TVP, or use temp tables to bulk delete with joins to our list of version ids to nuke.
             * (much nicer, we can kill 100k in sub second time-frames).
             *
             * However we are supporting SQL CE, so the easiest thing to do is use the Umbraco InGroupsOf helper to create a query with 2K args of version 
             * ids to delete at a time.
             *
             * This is already done at the repository level, however if we only had a single scope at service level we're still locking
             * the ContentVersions table (and other related tables) for a couple of minutes which makes the back office unusable.
             *
             * As a quick fix, we can also use InGroupsOf at service level, create a scope per group to give other connections a chance
             * to grab the locks and execute their queries.
             *
             * This makes the back office a tiny bit sluggish during first run but it is usable for loading tree and publishing content.
             *
             * There are optimizations we can do, we could add a bulk delete for SqlServerSyntaxProvider which differs in implementation
             * and fallback to this naive approach only for SQL CE, however we agreed it is not worth the effort as this is a one time pain,
             * subsequent runs shouldn't have huge numbers of versions to cleanup.
             *
             * tl;dr lots of scopes to enable other connections to use the DB whilst we work.
             */
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var allHistoricVersions = documentVersionRepository.GetDocumentVersionsEligibleForCleanup();

                Logger.Debug<ContentService>("Discovered {count} candidate(s) for ContentVersion cleanup.", allHistoricVersions.Count);
                versionsToDelete = new List<ContentVersionMeta>(allHistoricVersions.Count);

                var filteredContentVersions = cleanupPolicy.Apply(asAtDate, allHistoricVersions);

                foreach (var version in filteredContentVersions)
                {
                    var args = new DeleteRevisionsEventArgs(version.ContentId, version.VersionId);

                    if (scope.Events.DispatchCancelable(ContentService.DeletingVersions, this, args))
                    {
                        Logger.Debug<ContentService>("Delete cancelled for ContentVersion [{versionId}]",  version.VersionId);
                        continue;
                    }

                    versionsToDelete.Add(version);
                }
            }

            if (!versionsToDelete.Any())
            {
                Logger.Debug<ContentService>("No remaining ContentVersions for cleanup.", versionsToDelete.Count);
                return Array.Empty<ContentVersionMeta>();
            }

            Logger.Debug<ContentService>("Removing {count} ContentVersion(s).", versionsToDelete.Count);

            foreach (var group in versionsToDelete.InGroupsOf(Constants.Sql.MaxParameterCount))
            {
                using (var scope = ScopeProvider.CreateScope(autoComplete: true))
                {
                    scope.WriteLock(Constants.Locks.ContentTree);
                    var groupEnumerated = group.ToList();
                    documentVersionRepository.DeleteVersions(groupEnumerated.Select(x => x.VersionId));

                    foreach (var version in groupEnumerated)
                    {
                        var args = new DeleteRevisionsEventArgs(version.ContentId, version.VersionId);
                        scope.Events.Dispatch(ContentService.DeletedVersions, this, args);
                    }
                }
            }

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                Audit(AuditType.Delete, Constants.Security.SuperUserId, -1, $"Removed {versionsToDelete.Count} ContentVersion(s) according to cleanup policy.");
            }

            return versionsToDelete;
        }
    }
}
