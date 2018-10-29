using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
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
    internal class ContentService : RepositoryService, IContentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IEntityRepository _entityRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IDocumentBlueprintRepository _documentBlueprintRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly MediaFileSystem _mediaFileSystem;
        private IQuery<IContent> _queryNotTrashed;

        #region Constructors

        public ContentService(IScopeProvider provider, ILogger logger,
            IEventMessagesFactory eventMessagesFactory, MediaFileSystem mediaFileSystem,
            IDocumentRepository documentRepository, IEntityRepository entityRepository, IAuditRepository auditRepository,
            IContentTypeRepository contentTypeRepository, IDocumentBlueprintRepository documentBlueprintRepository, ILanguageRepository languageRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _mediaFileSystem = mediaFileSystem;
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
        public IContent Create(string name, Guid parentId, string contentTypeAlias, int userId = 0)
        {
            //fixme - what about culture?

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
        public IContent Create(string name, int parentId, string contentTypeAlias, int userId = 0)
        {
            //fixme - what about culture?

            var contentType = GetContentType(contentTypeAlias);
            if (contentType == null)
                throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias));
            var parent = parentId > 0 ? GetById(parentId) : null;
            if (parentId > 0 && parent == null)
                throw new ArgumentException("No content with that id.", nameof(parentId));

            var content = new Content(name, parentId, contentType);
            using (var scope = ScopeProvider.CreateScope())
            {
                CreateContent(scope, content, parent, userId, false);
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
        public IContent Create(string name, IContent parent, string contentTypeAlias, int userId = 0)
        {
            //fixme - what about culture?

            if (parent == null) throw new ArgumentNullException(nameof(parent));

            using (var scope = ScopeProvider.CreateScope())
            {
                // not locking since not saving anything

                var contentType = GetContentType(contentTypeAlias);
                if (contentType == null)
                    throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias)); // causes rollback

                var content = new Content(name, parent, contentType);
                CreateContent(scope, content, parent, userId, false);

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
        public IContent CreateAndSave(string name, int parentId, string contentTypeAlias, int userId = 0)
        {
            //fixme - what about culture?

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
                CreateContent(scope, content, parent, userId, true);

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
        public IContent CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId = 0)
        {
            //fixme - what about culture?

            if (parent == null) throw new ArgumentNullException(nameof(parent));

            using (var scope = ScopeProvider.CreateScope())
            {
                // locking the content tree secures content types too
                scope.WriteLock(Constants.Locks.ContentTree);

                var contentType = GetContentType(contentTypeAlias); // + locks
                if (contentType == null)
                    throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias)); // causes rollback

                var content = new Content(name, parent, contentType);
                CreateContent(scope, content, parent, userId, true);

                scope.Complete();
                return content;
            }
        }

        private void CreateContent(IScope scope, Content content, IContent parent, int userId, bool withIdentity)
        {
            content.CreatorId = userId;
            content.WriterId = userId;

            if (withIdentity)
            {
                // if saving is cancelled, content remains without an identity
                var saveEventArgs = new SaveEventArgs<IContent>(content);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs, "Saving"))
                    return;

                _documentRepository.Save(content);

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs, "Saved");
                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, TreeChangeTypes.RefreshNode).ToEventArgs());
            }

            scope.Events.Dispatch(Created, this, new NewEventArgs<IContent>(content, false, content.ContentType.Alias, parent));

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

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by the Id of the <see cref="IContentType"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetByType(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var query = Query<IContent>().Where(x => x.ContentTypeId == id);
                return _documentRepository.Get(query);
            }
        }

        internal IEnumerable<IContent> GetPublishedContentOfContentType(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var query = Query<IContent>().Where(x => x.ContentTypeId == id);
                return _documentRepository.Get(query);
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
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
            // intentionnaly not locking
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

            var rootId = Constants.System.Root.ToInvariantString();
            var ids = content.Path.Split(',')
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
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetChildren(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var query = Query<IContent>().Where(x => x.ParentId == id);
                return _documentRepository.Get(query).OrderBy(x => x.SortOrder);
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
            string filter = null, Ordering ordering = null)
        {
            var filterQuery = filter.IsNullOrWhiteSpace()
                ? null
                : Query<IContent>().Where(x => x.Name.Contains(filter));

            return GetPagedChildren(id, pageIndex, pageSize, out totalChildren, filterQuery, ordering);
        }

        /// <inheritdoc />
        public IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren,
            IQuery<IContent> filter, Ordering ordering = null)
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
        public IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, string orderBy = "Path", Direction orderDirection = Direction.Ascending, string filter = "")
        {
            var filterQuery = filter.IsNullOrWhiteSpace()
                ? null
                : Query<IContent>().Where(x => x.Name.Contains(filter));

            return GetPagedDescendants(id, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, true, filterQuery);
        }

        /// <inheritdoc />
        public IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IContent> filter)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);

                var query = Query<IContent>();

                //if the id is System Root, then just get all
                if (id != Constants.System.Root)
                {
                    var contentPath = _entityRepository.GetAllPaths(Constants.ObjectTypes.Document, id).ToArray();
                    if (contentPath.Length == 0)
                    {
                        totalChildren = 0;
                        return Enumerable.Empty<IContent>();
                    }
                    query.Where(x => x.Path.SqlStartsWith($"{contentPath[0].Path},", TextColumnType.NVarchar));
                }

                return _documentRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, Ordering.By(orderBy, orderDirection, isCustomField: !orderBySystemField));
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by its name or partial name
        /// </summary>
        /// <param name="parentId">Id of the Parent to retrieve Children from</param>
        /// <param name="name">Full or partial name of the children</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetChildren(int parentId, string name)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var query = Query<IContent>().Where(x => x.ParentId == parentId && x.Name.Contains(name));
                return _documentRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Descendants from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetDescendants(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var content = GetById(id);
                if (content == null)
                {
                    scope.Complete(); // else causes rollback
                    return Enumerable.Empty<IContent>();
                }
                var pathMatch = content.Path + ",";
                var query = Query<IContent>().Where(x => x.Id != content.Id && x.Path.StartsWith(pathMatch));
                return _documentRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="content"><see cref="IContent"/> item to retrieve Descendants from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetDescendants(IContent content)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var pathMatch = content.Path + ",";
                var query = Query<IContent>().Where(x => x.Id != content.Id && x.Path.StartsWith(pathMatch));
                return _documentRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets the parent of the current content as an <see cref="IContent"/> item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> to retrieve the parent from</param>
        /// <returns>Parent <see cref="IContent"/> object</returns>
        public IContent GetParent(int id)
        {
            // intentionnaly not locking
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

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has an expiration date less than or equal to today.
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentForExpiration()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var query = Query<IContent>().Where(x => x.Published && x.ExpireDate <= DateTime.Now);
                return _documentRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has a release date less than or equal to today.
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentForRelease()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var query = Query<IContent>().Where(x => x.Published == false && x.ReleaseDate <= DateTime.Now);
                return _documentRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentInRecycleBin()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                var query = Query<IContent>().Where(x => x.Path.StartsWith(Constants.System.RecycleBinContentPathPrefix));
                return _documentRepository.Get(query);
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
        /// Checks if the passed in <see cref="IContent"/> can be published based on the anscestors publish state.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to check if anscestors are published</param>
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

        // fixme - kill all those raiseEvents

        /// <inheritdoc />
        public OperationResult Save(IContent content, int userId = 0, bool raiseEvents = true)
        {
            var publishedState = content.PublishedState;
            if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
                throw new InvalidOperationException("Cannot save (un)publishing content, use the dedicated SavePublished method.");

            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IContent>(content, evtMsgs);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs, "Saving"))
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
                    ? string.Join(",", content.CultureInfos.Where(x => x.Value.IsDirty()).Select(x => x.Key))
                    : null;
                //TODO: Currently there's no way to change track which variant properties have changed, we only have change
                // tracking enabled on all values on the Property which doesn't allow us to know which variants have changed.
                // in this particular case, determining which cultures have changed works with the above with names since it will
                // have always changed if it's been saved in the back office but that's not really fail safe.

                _documentRepository.Save(content);

                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Saved, this, saveEventArgs, "Saved");
                }
                var changeType = TreeChangeTypes.RefreshNode;
                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, changeType).ToEventArgs());

                if (culturesChanging != null)
                    Audit(AuditType.SaveVariant, userId, content.Id, $"Saved cultures: {culturesChanging}", culturesChanging);
                else
                    Audit(AuditType.Save, userId, content.Id);

                scope.Complete();
            }

            return OperationResult.Succeed(evtMsgs);
        }

        /// <inheritdoc />
        public OperationResult Save(IEnumerable<IContent> contents, int userId = 0, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var contentsA = contents.ToArray();

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IContent>(contentsA, evtMsgs);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs, "Saving"))
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
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Saved, this, saveEventArgs, "Saved");
                }
                scope.Events.Dispatch(TreeChanged, this, treeChanges.ToEventArgs());
                Audit(AuditType.Save, userId == -1 ? 0 : userId, Constants.System.Root, "Saved multiple content");

                scope.Complete();
            }

            return OperationResult.Succeed(evtMsgs);
        }

        /// <inheritdoc />
        public PublishResult SaveAndPublish(IContent content, string culture = "*", int userId = 0, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();

            var publishedState = content.PublishedState;
            if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
                throw new InvalidOperationException($"Cannot save-and-publish (un)publishing content, use the dedicated {nameof(SavePublishing)} method.");

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

            // if culture is specific, first publish the invariant values, then publish the culture itself.
            // if culture is '*', then publish them all (including variants)

            // explicitely SaveAndPublish a specific culture also publishes invariant values
            if (!culture.IsNullOrWhiteSpace() && culture != "*")
            {
                // publish the invariant values
                var publishInvariant = content.PublishCulture(null);
                if (!publishInvariant)
                    return new PublishResult(PublishResultType.FailedContentInvalid, evtMsgs, content);
            }

            // publish the culture(s)
            var publishCulture = content.PublishCulture(culture);
            if (!publishCulture)
                return new PublishResult(PublishResultType.FailedContentInvalid, evtMsgs, content);

            // finally, "save publishing"
            // what happens next depends on whether the content can be published or not
            return SavePublishing(content, userId, raiseEvents);
        }

        /// <inheritdoc />
        public UnpublishResult Unpublish(IContent content, string culture = "*", int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();

            culture = culture.NullOrWhiteSpaceAsNull();

            var publishedState = content.PublishedState;
            if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
                throw new InvalidOperationException($"Cannot save-and-publish (un)publishing content, use the dedicated {nameof(SavePublishing)} method.");

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
                return new UnpublishResult(UnpublishResultType.SuccessAlready, evtMsgs, content);

            // all cultures = unpublish whole
            if (culture == "*" || (!content.ContentType.VariesByCulture() && culture == null))
            {
                ((Content) content).PublishedState = PublishedState.Unpublishing;
            }
            else
            {
                // if the culture we want to unpublish was already unpublished, nothing to do
                if (!content.WasCulturePublished(culture))
                    return new UnpublishResult(UnpublishResultType.SuccessAlready, evtMsgs, content);

                // unpublish the culture
                content.UnpublishCulture(culture);
            }

            // finally, "save publishing"
            // what happens next depends on whether the content can be published or not
            using (var scope = ScopeProvider.CreateScope())
            {
                var saved = SavePublishing(content, userId);
                if (saved.Success)
                {
                    UnpublishResultType result;
                    if (culture == "*" || culture == null)
                    {
                        Audit(AuditType.Unpublish, userId, content.Id);
                        result = UnpublishResultType.Success;
                    }
                    else
                    {
                        //unpublishing a specific culture
                        Audit(AuditType.UnpublishVariant, userId, content.Id, $"Culture \"{culture}\" unpublished", culture);
                        if (!content.Published)
                        {
                            //log that the whole content item has been unpublished due to mandatory culture unpublished
                            Audit(AuditType.Unpublish, userId, content.Id, $"Unpublished (culture \"{culture}\" is mandatory)");
                        }
                            
                        result = content.Published ? UnpublishResultType.SuccessCulture : UnpublishResultType.SuccessMandatoryCulture;
                    }
                    scope.Complete();
                    return new UnpublishResult(result, evtMsgs, content);
                }

                // failed - map result
                var r = saved.Result == PublishResultType.FailedCancelledByEvent
                    ? UnpublishResultType.FailedCancelledByEvent
                    : UnpublishResultType.Failed;
                return new UnpublishResult(r, evtMsgs, content);
            }
        }

        /// <inheritdoc />
        public PublishResult SavePublishing(IContent content, int userId = 0, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();
            PublishResult publishResult = null;
            UnpublishResult unpublishResult = null;

            // nothing set = republish it all
            if (content.PublishedState != PublishedState.Publishing && content.PublishedState != PublishedState.Unpublishing)
                ((Content) content).PublishedState = PublishedState.Publishing;

            // state here is either Publishing or Unpublishing
            var publishing = content.PublishedState == PublishedState.Publishing;
            var unpublishing = content.PublishedState == PublishedState.Unpublishing;

            string culturesChanging = null;

            using (var scope = ScopeProvider.CreateScope())
            {
                // is the content going to end up published, or unpublished?
                if (publishing && content.ContentType.VariesByCulture())
                {
                    var publishedCultures = content.PublishedCultures.ToList();
                    var cannotBePublished = publishedCultures.Count == 0; // no published cultures = cannot be published
                    if (!cannotBePublished)
                    {
                        var mandatoryCultures = _languageRepository.GetMany().Where(x => x.IsMandatory).Select(x => x.IsoCode);
                        cannotBePublished = mandatoryCultures.Any(x => !publishedCultures.Contains(x, StringComparer.OrdinalIgnoreCase)); // missing mandatory culture = cannot be published
                    }

                    if (cannotBePublished)
                    {
                        publishing = false;
                        unpublishing = content.Published; // if not published yet, nothing to do

                        // we may end up in a state where we won't publish nor unpublish
                        // keep going, though, as we want to save anways
                    }
                    else
                    {
                        culturesChanging = string.Join(",", content.PublishCultureInfos.Where(x => x.Value.IsDirty()).Select(x => x.Key));
                    }
                }

                var isNew = !content.HasIdentity;
                var changeType = isNew ? TreeChangeTypes.RefreshNode : TreeChangeTypes.RefreshBranch;
                var previouslyPublished = content.HasIdentity && content.Published;

                scope.WriteLock(Constants.Locks.ContentTree);

                // always save
                var saveEventArgs = new SaveEventArgs<IContent>(content, evtMsgs);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs, "Saving"))
                {
                    scope.Complete();
                    return new PublishResult(PublishResultType.FailedCancelledByEvent, evtMsgs, content);
                }

                if (publishing)
                {
                    // ensure that the document can be published, and publish
                    // handling events, business rules, etc
                    // note: StrategyPublish flips the PublishedState to Publishing!
                    publishResult = StrategyCanPublish(scope, content, userId, /*checkPath:*/ true, evtMsgs);
                    if (publishResult.Success)
                        publishResult = StrategyPublish(scope, content, /*canPublish:*/ true, userId, evtMsgs);
                    if (!publishResult.Success)
                        ((Content) content).Published = content.Published; // reset published state = save unchanged
                }

                if (unpublishing)
                {
                    var newest = GetById(content.Id); // ensure we have the newest version - in scope
                    if (content.VersionId != newest.VersionId) // but use the original object if it's already the newest version
                        content = newest;

                    if (content.Published)
                    {
                        // ensure that the document can be unpublished, and unpublish
                        // handling events, business rules, etc
                        // note: StrategyUnpublish flips the PublishedState to Unpublishing!
                        // note: This unpublishes the entire document (not different variants)
                        unpublishResult = StrategyCanUnpublish(scope, content, userId, evtMsgs);
                        if (unpublishResult.Success)
                            unpublishResult = StrategyUnpublish(scope, content, true, userId, evtMsgs);
                        if (!unpublishResult.Success)
                            ((Content) content).Published = content.Published; // reset published state = save unchanged
                    }
                    else
                    {
                        // already unpublished - optimistic concurrency collision, really,
                        // and I am not sure at all what we should do, better die fast, else
                        // we may end up corrupting the db
                        throw new InvalidOperationException("Concurrency collision.");
                    }
                }

                // save, always
                if (content.HasIdentity == false)
                    content.CreatorId = userId;
                content.WriterId = userId;

                // saving does NOT change the published version, unless PublishedState is Publishing or Unpublishing
                _documentRepository.Save(content);

                // raise the Saved event, always
                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Saved, this, saveEventArgs, "Saved");
                }

                if (unpublishing) // we have tried to unpublish
                {
                    if (unpublishResult.Success) // and succeeded, trigger events
                    {
                        // events and audit
                        scope.Events.Dispatch(Unpublished, this, new PublishEventArgs<IContent>(content, false, false), "Unpublished");
                        scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, TreeChangeTypes.RefreshBranch).ToEventArgs());
                        Audit(AuditType.Unpublish, userId, content.Id);
                        scope.Complete();
                        return new PublishResult(PublishResultType.Success, evtMsgs, content);
                    }

                    // or, failed
                    scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, changeType).ToEventArgs());
                    scope.Complete(); // compete the save
                    return new PublishResult(PublishResultType.FailedToUnpublish, evtMsgs, content); // bah
                }

                if (publishing) // we have tried to publish
                {
                    if (publishResult.Success) // and succeeded, trigger events
                    {
                        if (isNew == false && previouslyPublished == false)
                            changeType = TreeChangeTypes.RefreshBranch; // whole branch

                        // invalidate the node/branch
                        scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, changeType).ToEventArgs());
                        scope.Events.Dispatch(Published, this, new PublishEventArgs<IContent>(content, false, false), "Published");

                        // if was not published and now is... descendants that were 'published' (but
                        // had an unpublished ancestor) are 're-published' ie not explicitely published
                        // but back as 'published' nevertheless
                        if (isNew == false && previouslyPublished == false && HasChildren(content.Id))
                        {
                            var descendants = GetPublishedDescendantsLocked(content).ToArray();
                            scope.Events.Dispatch(Published, this, new PublishEventArgs<IContent>(descendants, false, false), "Published");
                        }

                        if (culturesChanging != null)
                            Audit(AuditType.PublishVariant, userId, content.Id, $"Published cultures: {culturesChanging}", culturesChanging);
                        else
                            Audit(AuditType.Publish, userId, content.Id);

                        scope.Complete();
                        return publishResult;
                    }

                    // or, failed
                    scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, changeType).ToEventArgs());
                    scope.Complete(); // compete the save
                    return publishResult;
                }

                // both publishing and unpublishing are false
                // this means that we wanted to publish, in a variant scenario, a document that
                // was not published yet, and we could not, due to cultures issues
                //
                // raise event (we saved), report

                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(content, changeType).ToEventArgs());
                scope.Complete(); // compete the save
                return new PublishResult(PublishResultType.FailedByCulture, evtMsgs, content);
            }
        }

        /// <inheritdoc />
        public IEnumerable<PublishResult> PerformScheduledPublish()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                foreach (var d in GetContentForRelease())
                {
                    PublishResult result;
                    try
                    {
                        d.ReleaseDate = null;
                        d.PublishCulture(); // fixme variants?
                        result = SaveAndPublish(d, userId: d.WriterId);
                        if (result.Success == false)
                            Logger.Error<ContentService>(null, "Failed to publish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);
                    }
                    catch (Exception e)
                    {
                        Logger.Error<ContentService>(e, "Failed to publish document id={DocumentId}, an exception was thrown.", d.Id);
                        throw;
                    }
                    yield return result;
                }
                foreach (var d in GetContentForExpiration())
                {
                    try
                    {
                        d.ExpireDate = null;
                        var result = Unpublish(d, userId: d.WriterId);
                        if (result.Success == false)
                            Logger.Error<ContentService>(null, "Failed to unpublish document id={DocumentId}, reason={Reason}.", d.Id, result.Result);
                    }
                    catch (Exception e)
                    {
                        Logger.Error<ContentService>(e, "Failed to unpublish document id={DocumentId}, an exception was thrown.", d.Id);
                        throw;
                    }
                }

                scope.Complete();
            }
        }

        /// <inheritdoc />
        public IEnumerable<PublishResult> SaveAndPublishBranch(IContent content, bool force, string culture = "*", int userId = 0)
        {
            // note: EditedValue and PublishedValue are objects here, so it is important to .Equals()
            // and not to == them, else we would be comparing references, and that is a bad thing

            bool IsEditing(IContent c, string l)
                => c.PublishName != c.Name ||
                   c.PublishedCultures.Where(x => x.InvariantEquals(l)).Any(x => c.GetCultureName(x) != c.GetPublishName(x)) ||
                   c.Properties.Any(x => x.Values.Where(y => culture == "*" || y.Culture.InvariantEquals(l)).Any(y => !y.EditedValue.Equals(y.PublishedValue)));

            return SaveAndPublishBranch(content, force, document => IsEditing(document, culture), document => document.PublishCulture(culture), userId);
        }

        // fixme - make this public once we know it works + document
        private IEnumerable<PublishResult> SaveAndPublishBranch(IContent content, bool force, string[] cultures, int userId = 0)
        {
            // note: EditedValue and PublishedValue are objects here, so it is important to .Equals()
            // and not to == them, else we would be comparing references, and that is a bad thing

            cultures = cultures ?? Array.Empty<string>();

            // determines whether the document is edited, and thus needs to be published,
            // for the specified cultures (it may be edited for other cultures and that
            // should not trigger a publish).
            bool IsEdited(IContent c)
            {
                if (cultures.Length == 0)
                {
                    // nothing = everything
                    return c.PublishName != c.Name ||
                           c.PublishedCultures.Any(x => c.GetCultureName(x) != c.GetPublishName(x)) ||
                           c.Properties.Any(x => x.Values.Any(y => !y.EditedValue.Equals(y.PublishedValue)));
                }

                return c.PublishName != c.Name ||
                       c.PublishedCultures.Where(x => cultures.Contains(x, StringComparer.InvariantCultureIgnoreCase)).Any(x => c.GetCultureName(x) != c.GetPublishName(x)) ||
                       c.Properties.Any(x => x.Values.Where(y => cultures.Contains(y.Culture, StringComparer.InvariantCultureIgnoreCase)).Any(y => !y.EditedValue.Equals(y.PublishedValue)));
            }

            // publish the specified cultures
            bool PublishCultures(IContent c)
            {
                return cultures.Length == 0
                    ? c.PublishCulture() // nothing = everything
                    : cultures.All(c.PublishCulture);
            }

            return SaveAndPublishBranch(content, force, IsEdited, PublishCultures, userId);
        }

        /// <inheritdoc />
        public IEnumerable<PublishResult> SaveAndPublishBranch(IContent document, bool force,
            Func<IContent, bool> editing, Func<IContent, bool> publishCultures, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var results = new List<PublishResult>();
            var publishedDocuments = new List<IContent>();

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                // fixme events?!

                if (!document.HasIdentity)
                    throw new InvalidOperationException("Do not branch-publish a new document.");

                var publishedState = ((Content) document).PublishedState;
                if (publishedState == PublishedState.Publishing)
                    throw new InvalidOperationException("Do not publish values when publishing branches.");

                // deal with the branch root - if it fails, abort
                var result = SaveAndPublishBranchOne(scope, document, editing, publishCultures, true, publishedDocuments, evtMsgs, userId);
                results.Add(result);
                if (!result.Success) return results;

                // deal with descendants
                // if one fails, abort its branch
                var exclude = new HashSet<int>();

                //fixme: should be paged to not overwhelm the database (timeouts)
                foreach (var d in GetDescendants(document))
                {
                    // if parent is excluded, exclude document and ignore
                    // if not forcing, and not publishing, exclude document and ignore
                    if (exclude.Contains(d.ParentId)  ||  !force && !d.Published)
                    {
                        exclude.Add(d.Id);
                        continue;
                    }

                    // no need to check path here,
                    // 1. because we know the parent is path-published (we just published it)
                    // 2. because it would not work as nothing's been written out to the db until the uow completes
                    result = SaveAndPublishBranchOne(scope, d, editing, publishCultures, false, publishedDocuments, evtMsgs, userId);
                    results.Add(result);
                    if (result.Success) continue;

                    // abort branch
                    exclude.Add(d.Id);
                }

                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IContent>(document, TreeChangeTypes.RefreshBranch).ToEventArgs());
                scope.Events.Dispatch(Published, this, new PublishEventArgs<IContent>(publishedDocuments, false, false), "Published");
                Audit(AuditType.Publish, userId, document.Id, "Branch published");

                scope.Complete();
            }

            return results;
        }

        private PublishResult SaveAndPublishBranchOne(IScope scope, IContent document,
            Func<IContent, bool> editing, Func<IContent, bool> publishValues,
            bool checkPath,
            List<IContent> publishedDocuments,
            EventMessages evtMsgs, int userId)
        {
            // if already published, and values haven't changed - i.e. not changing anything
            // nothing to do - fixme - unless we *want* to bump dates?
            if (document.Published && (editing == null || !editing(document)))
                return new PublishResult(PublishResultType.SuccessAlready, evtMsgs, document);

            // publish & check if values are valid
            if (publishValues != null && !publishValues(document))
                return new PublishResult(PublishResultType.FailedContentInvalid, evtMsgs, document);

            // check if we can publish
            var result = StrategyCanPublish(scope, document, userId, checkPath, evtMsgs);
            if (!result.Success)
                return result;

            // publish - should be successful
            var publishResult = StrategyPublish(scope, document, /*canPublish:*/ true, userId, evtMsgs);
            if (!publishResult.Success)
                throw new Exception("oops: failed to publish.");

            // save
            document.WriterId = userId;
            _documentRepository.Save(document);
            publishedDocuments.Add(document);
            return publishResult;
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
            // then recursively delete descendants, bottom-up
            // just repository.Delete + an event
            var stack = new Stack<IContent>();
            stack.Push(content);
            var level = 1;
            while (stack.Count > 0)
            {
                var c = stack.Peek();
                IContent[] cc;
                if (c.Level == level)
                    while ((cc = c.Children(this).ToArray()).Length > 0)
                    {
                        foreach (var ci in cc)
                            stack.Push(ci);
                        c = cc[cc.Length - 1];
                    }
                c = stack.Pop();
                level = c.Level;

                _documentRepository.Delete(c);
                var args = new DeleteEventArgs<IContent>(c, false); // raise event & get flagged files
                scope.Events.Dispatch(Deleted, this, args, nameof(Deleted));

                // fixme not going to work, do it differently
                _mediaFileSystem.DeleteFiles(args.MediaFilesToDelete, // remove flagged files
                    (file, e) => Logger.Error<ContentService>(e, "An error occurred while deleting file attached to nodes: {File}", file));
            }
        }

        //TODO:
        // both DeleteVersions methods below have an issue. Sort of. They do NOT take care of files the way
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
        public void DeleteVersions(int id, DateTime versionDate, int userId = 0)
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
        public void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = 0)
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
                    // fixme nesting uow?
                    DeleteVersions(id, content.UpdateDate, userId);
                }

                scope.WriteLock(Constants.Locks.ContentTree);
                var c = _documentRepository.Get(id);
                if (c.VersionId != versionId) // don't delete the current version
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
            var moves = new List<Tuple<IContent, string>>();

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
        public void Move(IContent content, int parentId, int userId = 0)
        {
            // if moving to the recycle bin then use the proper method
            if (parentId == Constants.System.RecycleBinContent)
            {
                MoveToRecycleBin(content, userId);
                return;
            }

            var moves = new List<Tuple<IContent, string>>();

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
                    ((Content) content).PublishedState = PublishedState.Unpublishing;
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
            ICollection<Tuple<IContent, string>> moves,
            bool? trash)
        {
            content.WriterId = userId;
            content.ParentId = parentId;

            // get the level delta (old pos to new pos)
            var levelDelta = parent == null
                ? 1 - content.Level + (parentId == Constants.System.RecycleBinContent ? 1 : 0)
                : parent.Level + 1 - content.Level;

            var paths = new Dictionary<int, string>();

            moves.Add(Tuple.Create(content, content.Path)); // capture original path

            // get before moving, in case uow is immediate
            var descendants = GetDescendants(content);

            // these will be updated by the repo because we changed parentId
            //content.Path = (parent == null ? "-1" : parent.Path) + "," + content.Id;
            //content.SortOrder = ((ContentRepository) repository).NextChildSortOrder(parentId);
            //content.Level += levelDelta;
            PerformMoveContentLocked(content, userId, trash);

            // if uow is not immediate, content.Path will be updated only when the UOW commits,
            // and because we want it now, we have to calculate it by ourselves
            //paths[content.Id] = content.Path;
            paths[content.Id] = (parent == null ? (parentId == Constants.System.RecycleBinContent ? "-1,-20" : "-1") : parent.Path) + "," + content.Id;

            foreach (var descendant in descendants)
            {
                moves.Add(Tuple.Create(descendant, descendant.Path)); // capture original path

                // update path and level since we do not update parentId
                if (paths.ContainsKey(descendant.ParentId) == false)
                    Console.WriteLine("oops on " + descendant.ParentId + " for " + content.Path + " " + parent?.Path);
                descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
                Console.WriteLine("path " + descendant.Id + " = " + paths[descendant.Id]);
                descendant.Level += levelDelta;
                PerformMoveContentLocked(descendant, userId, trash);
            }
        }

        private void PerformMoveContentLocked(IContent content, int userId, bool? trash)
        {
            if (trash.HasValue) ((ContentBase) content).Trashed = trash.Value;
            content.WriterId = userId;
            _documentRepository.Save(content);
        }

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IContent"/> that resides in the bin
        /// </summary>
        public OperationResult EmptyRecycleBin()
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

                // emptying the recycle bin means deleting whetever is in there - do it properly!
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
                Audit(AuditType.Delete, 0, Constants.System.RecycleBinContent, "Recycle bin emptied");

                scope.Complete();
            }

            return OperationResult.Succeed(evtMsgs);
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
        public IContent Copy(IContent content, int parentId, bool relateToOriginal, int userId = 0)
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
        public IContent Copy(IContent content, int parentId, bool relateToOriginal, bool recursive, int userId = 0)
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
                    ((Content) copy).Published = false;
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
                    foreach (var descendant in GetDescendants(content))
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
                            ((Content) descendantCopy).Published = false;
                        descendantCopy.CreatorId = userId;
                        descendantCopy.WriterId = userId;

                        // save and flush (see above)
                        _documentRepository.Save(descendantCopy);

                        copies.Add(Tuple.Create(descendant, descendantCopy));
                        idmap[descendant.Id] = descendantCopy.Id;
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
        /// <param name="userId">Optional Id of the User issueing the send to publication</param>
        /// <returns>True if sending publication was succesfull otherwise false</returns>
        public bool SendToPublication(IContent content, int userId = 0)
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
                    ? string.Join(",", content.CultureInfos.Where(x => x.Value.IsDirty()).Select(x => x.Key))
                    : null;
                //TODO: Currently there's no way to change track which variant properties have changed, we only have change
                // tracking enabled on all values on the Property which doesn't allow us to know which variants have changed.
                // in this particular case, determining which cultures have changed works with the above with names since it will
                // have always changed if it's been saved in the back office but that's not really fail safe.

                //Save before raising event
                // fixme - nesting uow?
                var saveResult = Save(content, userId);

                if (saveResult.Success)
                {
                    sendToPublishEventArgs.CanCancel = false;
                    scope.Events.Dispatch(SentToPublish, this, sendToPublishEventArgs);

                    if (culturesChanging != null)
                        Audit(AuditType.SendToPublishVariant, userId, content.Id, $"Send To Publish for cultures: {culturesChanging}", culturesChanging);
                    else
                        Audit(AuditType.SendToPublish, content.WriterId, content.Id);
                }

                // fixme here, on only on success?                
                scope.Complete();

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
        /// <returns>True if sorting succeeded, otherwise False</returns>
        public bool Sort(IEnumerable<IContent> items, int userId = 0, bool raiseEvents = true)
        {
            var itemsA = items.ToArray();
            if (itemsA.Length == 0) return true;

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                var ret = Sort(scope, itemsA, userId, raiseEvents);
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
        /// <returns>True if sorting succeeded, otherwise False</returns>
        public bool Sort(IEnumerable<int> ids, int userId = 0, bool raiseEvents = true)
        {
            var idsA = ids.ToArray();
            if (idsA.Length == 0) return true;

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);
                var itemsA = GetByIds(idsA).ToArray();

                var ret = Sort(scope, itemsA, userId, raiseEvents);
                scope.Complete();
                return ret;
            }
        }

        private bool Sort(IScope scope, IContent[] itemsA, int userId, bool raiseEvents)
        {
            var saveEventArgs = new SaveEventArgs<IContent>(itemsA);
            if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs, "Saving"))
                return false;

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
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs, "Saved");
            }

            scope.Events.Dispatch(TreeChanged, this, saved.Select(x => new TreeChange<IContent>(x, TreeChangeTypes.RefreshNode)).ToEventArgs());

            if (raiseEvents && published.Any())
                scope.Events.Dispatch(Published, this, new PublishEventArgs<IContent>(published, false, false), "Published");

            Audit(AuditType.Sort, userId, 0, "Sorting content performed by user");
            return true;
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
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IContentService, SaveEventArgs<IContent>> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IContentService, SaveEventArgs<IContent>> Saved;

        /// <summary>
        /// Occurs after Create
        /// </summary>
        /// <remarks>
        /// Please note that the Content object has been created, but might not have been saved
        /// so it does not have an identity yet (meaning no Id has been set).
        /// </remarks>
        public static event TypedEventHandler<IContentService, NewEventArgs<IContent>> Created;

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
        public static event TypedEventHandler<IContentService, PublishEventArgs<IContent>> Publishing;

        /// <summary>
        /// Occurs after publish
        /// </summary>
        public static event TypedEventHandler<IContentService, PublishEventArgs<IContent>> Published;

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
        internal static event TypedEventHandler<IContentService, TreeChange<IContent>.EventArgs> TreeChanged;

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

        // ensures that a document can be published
        internal PublishResult StrategyCanPublish(IScope scope, IContent content, int userId, bool checkPath, EventMessages evtMsgs)
        {
            // raise Publishing event
            if (scope.Events.DispatchCancelable(Publishing, this, new PublishEventArgs<IContent>(content, evtMsgs)))
            {
                Logger.Info<ContentService>("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "publishing was cancelled");
                return new PublishResult(PublishResultType.FailedCancelledByEvent, evtMsgs, content);
            }

            // ensure that the document has published values
            // either because it is 'publishing' or because it already has a published version
            if (((Content) content).PublishedState != PublishedState.Publishing && content.PublishedVersionId == 0)
            {
                Logger.Info<ContentService>("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "document does not have published values");
                return new PublishResult(PublishResultType.FailedNoPublishedValues, evtMsgs, content);
            }

            // ensure that the document status is correct
            switch (content.Status)
            {
                case ContentStatus.Expired:
                    Logger.Info<ContentService>("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "document has expired");
                    return new PublishResult(PublishResultType.FailedHasExpired, evtMsgs, content);

                case ContentStatus.AwaitingRelease:
                    Logger.Info<ContentService>("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "document is awaiting release");
                    return new PublishResult(PublishResultType.FailedAwaitingRelease, evtMsgs, content);

                case ContentStatus.Trashed:
                    Logger.Info<ContentService>("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "document is trashed");
                    return new PublishResult(PublishResultType.FailedIsTrashed, evtMsgs, content);
            }

            if (!checkPath) return new PublishResult(evtMsgs, content);

            // check if the content can be path-published
            // root content can be published
            // else check ancestors - we know we are not trashed
            var pathIsOk = content.ParentId == Constants.System.Root || IsPathPublished(GetParent(content));
            if (pathIsOk == false)
            {
                Logger.Info<ContentService>("Document {ContentName} (id={ContentId}) cannot be published: {Reason}", content.Name, content.Id, "parent is not published");
                return new PublishResult(PublishResultType.FailedPathNotPublished, evtMsgs, content);
            }

            return new PublishResult(evtMsgs, content);
        }

        // publishes a document
        internal PublishResult StrategyPublish(IScope scope, IContent content, bool canPublish, int userId, EventMessages evtMsgs)
        {
            // note: when used at top-level, StrategyCanPublish with checkPath=true should have run already
            // and alreadyCheckedCanPublish should be true, so not checking again. when used at nested level,
            // there is no need to check the path again. so, checkPath=false in StrategyCanPublish below

            var result = canPublish
                ? new PublishResult(evtMsgs, content) // already know we can
                : StrategyCanPublish(scope, content, userId, /*checkPath:*/ false, evtMsgs); // else check

            if (result.Success == false)
                return result;

            // change state to publishing
            ((Content) content).PublishedState = PublishedState.Publishing;

            Logger.Info<ContentService>("Document {ContentName} (id={ContentId}) has been published.", content.Name, content.Id);
            return result;
        }

        // ensures that a document can be unpublished
        internal UnpublishResult StrategyCanUnpublish(IScope scope, IContent content, int userId, EventMessages evtMsgs)
        {
            // raise Unpublishing event
            if (scope.Events.DispatchCancelable(Unpublishing, this, new PublishEventArgs<IContent>(content, evtMsgs)))
            {
                Logger.Info<ContentService>("Document {ContentName} (id={ContentId}) cannot be unpublished: unpublishing was cancelled.", content.Name, content.Id);
                return new UnpublishResult(UnpublishResultType.FailedCancelledByEvent, evtMsgs, content);
            }

            return new UnpublishResult(evtMsgs, content);
        }

        // unpublishes a document
        internal UnpublishResult StrategyUnpublish(IScope scope, IContent content, bool canUnpublish, int userId, EventMessages evtMsgs)
        {
            var attempt = canUnpublish
                ? new UnpublishResult(evtMsgs, content) // already know we can
                : StrategyCanUnpublish(scope, content, userId, evtMsgs); // else check

            if (attempt.Success == false)
                return attempt;

            // if the document has a release date set to before now,
            // it should be removed so it doesn't interrupt an unpublish
            // otherwise it would remain released == published
            if (content.ReleaseDate.HasValue && content.ReleaseDate.Value <= DateTime.Now)
            {
                content.ReleaseDate = null;
                Logger.Info<ContentService>("Document {ContentName} (id={ContentId}) had its release date removed, because it was unpublished.", content.Name, content.Id);
            }

            // change state to unpublishing
            ((Content) content).PublishedState = PublishedState.Unpublishing;

            Logger.Info<ContentService>("Document {ContentName} (id={ContentId}) has been unpublished.", content.Name, content.Id);
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
        /// <param name="contentTypeId">Id of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional Id of the user issueing the delete operation</param>
        public void DeleteOfTypes(IEnumerable<int> contentTypeIds, int userId = 0)
        {
            //TODO: This currently this is called from the ContentTypeService but that needs to change,
            // if we are deleting a content type, we should just delete the data and do this operation slightly differently.
            // This method will recursively go lookup every content item, check if any of it's descendants are
            // of a different type, move them to the recycle bin, then permanently delete the content items.
            // The main problem with this is that for every content item being deleted, events are raised...
            // which we need for many things like keeping caches in sync, but we can surely do this MUCH better.

            var changes = new List<TreeChange<IContent>>();
            var moves = new List<Tuple<IContent, string>>();
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
        public void DeleteOfType(int contentTypeId, int userId = 0)
        {
            DeleteOfTypes(new[] { contentTypeId }, userId);
        }

        private IContentType GetContentType(IScope scope, string contentTypeAlias)
        {
            if (string.IsNullOrWhiteSpace(contentTypeAlias)) throw new ArgumentNullOrEmptyException(nameof(contentTypeAlias));

            scope.ReadLock(Constants.Locks.ContentTypes);

            var query = Query<IContentType>().Where(x => x.Alias == contentTypeAlias);
            var contentType = _contentTypeRepository.Get(query).FirstOrDefault();

            if (contentType == null)
                throw new Exception($"No ContentType matching the passed in Alias: '{contentTypeAlias}' was found"); // causes rollback

            return contentType;
        }

        private IContentType GetContentType(string contentTypeAlias)
        {
            if (string.IsNullOrWhiteSpace(contentTypeAlias)) throw new ArgumentNullOrEmptyException(nameof(contentTypeAlias));

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
                    ((Content) blueprint).Blueprint = true;
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
                    ((Content) blueprint).Blueprint = true;
                return blueprint;
            }
        }

        public void SaveBlueprint(IContent content, int userId = 0)
        {
            //always ensure the blueprint is at the root
            if (content.ParentId != -1)
                content.ParentId = -1;

            ((Content) content).Blueprint = true;

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);

                if (string.IsNullOrWhiteSpace(content.Name))
                {
                    throw new ArgumentException("Cannot save content blueprint with empty name.");
                }

                if (content.HasIdentity == false)
                {
                    content.CreatorId = userId;
                }
                content.WriterId = userId;

                _documentBlueprintRepository.Save(content);

                scope.Events.Dispatch(SavedBlueprint, this, new SaveEventArgs<IContent>(content), "SavedBlueprint");

                scope.Complete();
            }
        }

        public void DeleteBlueprint(IContent content, int userId = 0)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);
                _documentBlueprintRepository.Delete(content);
                scope.Events.Dispatch(DeletedBlueprint, this, new DeleteEventArgs<IContent>(content), nameof(DeletedBlueprint));
                scope.Complete();
            }
        }

        public IContent CreateContentFromBlueprint(IContent blueprint, string name, int userId = 0)
        {
            if (blueprint == null) throw new ArgumentNullException(nameof(blueprint));

            var contentType = blueprint.ContentType;
            var content = new Content(name, -1, contentType);
            content.Path = string.Concat(content.ParentId.ToString(), ",", content.Id);

            content.CreatorId = userId;
            content.WriterId = userId;

            foreach (var property in blueprint.Properties)
                content.SetValue(property.Alias, property.GetValue()); //fixme doesn't take into account variants

            return content;
        }

        public IEnumerable<IContent> GetBlueprintsForContentTypes(params int[] contentTypeId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IContent>();
                if (contentTypeId.Length > 0)
                {
                    query.Where(x => contentTypeId.Contains(x.ContentTypeId));
                }
                return _documentBlueprintRepository.Get(query).Select(x =>
                {
                    ((Content) x).Blueprint = true;
                    return x;
                });
            }
        }

        public void DeleteBlueprintsOfTypes(IEnumerable<int> contentTypeIds, int userId = 0)
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
                    ((Content) x).Blueprint = true;
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

        public void DeleteBlueprintsOfType(int contentTypeId, int userId = 0)
        {
            DeleteBlueprintsOfTypes(new[] { contentTypeId }, userId);
        }

        #endregion

        #region Rollback

        public OperationResult Rollback(int id, int versionId, string culture = "*", int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            
            //Get the current copy of the node
            var content = GetById(id);

            //Get the version
            var version = GetVersion(versionId);

            //Good ole null checks
            if (content == null || version == null)
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
                    Logger.Error<ContentService>("User '{UserId}' was unable to rollback content '{ContentId}' to version '{VersionId}'", userId, id, versionId);
                }
                else
                {
                    //Emit RolledBack event aka after
                    rollbackEventArgs.CanCancel = false;
                    scope.Events.Dispatch(RolledBack, this, rollbackEventArgs);

                    //Logging & Audit message
                    Logger.Info<ContentService>("User '{UserId}' rolled back content '{ContentId}' to version '{VersionId}'", userId, id, versionId);
                    Audit(AuditType.RollBack, userId, id, $"Content '{content.Name}' was rolled back to version '{versionId}'");
                }
                
                scope.Complete();
            }

            return rollbackSaveResult;
        }

        #endregion
    }
}
