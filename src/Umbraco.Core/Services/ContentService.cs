using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Content Service, which is an easy access to operations involving <see cref="IContent"/>
    /// </summary>
    public class ContentService : RepositoryService, IContentService, IContentServiceOperations
    {
        private readonly EntityXmlSerializer _entitySerializer = new EntityXmlSerializer();
        private readonly IDataTypeService _dataTypeService;
        private readonly IUserService _userService;
        private readonly IEnumerable<IUrlSegmentProvider> _urlSegmentProviders;

        #region Constructors

        public ContentService(
            IDatabaseUnitOfWorkProvider provider,
            ILogger logger,
            IEventMessagesFactory eventMessagesFactory,
            IDataTypeService dataTypeService,
            IUserService userService,
            IEnumerable<IUrlSegmentProvider> urlSegmentProviders)
            : base(provider, logger, eventMessagesFactory)
        {
            if (dataTypeService == null) throw new ArgumentNullException(nameof(dataTypeService));
            if (userService == null) throw new ArgumentNullException(nameof(userService));
            if (urlSegmentProviders == null) throw new ArgumentNullException(nameof(urlSegmentProviders));
            _dataTypeService = dataTypeService;
            _userService = userService;
            _urlSegmentProviders = urlSegmentProviders;
        }

        #endregion

        #region Count

        public int CountPublished(string contentTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repo = uow.CreateRepository<IContentRepository>();
                var count = repo.CountPublished();
                uow.Complete();
                return count;
            }
        }

        public int Count(string contentTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repo = uow.CreateRepository<IContentRepository>();
                var count = repo.Count(contentTypeAlias);
                uow.Complete();
                return count;
            }
        }

        public int CountChildren(int parentId, string contentTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repo = uow.CreateRepository<IContentRepository>();
                var count = repo.CountChildren(parentId, contentTypeAlias);
                uow.Complete();
                return count;
            }
        }

        public int CountDescendants(int parentId, string contentTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repo = uow.CreateRepository<IContentRepository>();
                var count = repo.CountDescendants(parentId, contentTypeAlias);
                uow.Complete();
                return count;
            }
        }

        #endregion

        #region Permissions

        /// <summary>
        /// Used to bulk update the permissions set for a content item. This will replace all permissions
        /// assigned to an entity with a list of user id & permission pairs.
        /// </summary>
        /// <param name="permissionSet"></param>
        public void ReplaceContentPermissions(EntityPermissionSet permissionSet)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repo = uow.CreateRepository<IContentRepository>();
                repo.ReplaceContentPermissions(permissionSet);
                uow.Complete();
            }
        }

        /// <summary>
        /// Assigns a single permission to the current content item for the specified user ids
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permission"></param>
        /// <param name="userIds"></param>
        public void AssignContentPermission(IContent entity, char permission, IEnumerable<int> userIds)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repo = uow.CreateRepository<IContentRepository>();
                repo.AssignEntityPermission(entity, permission, userIds);
                uow.Complete();
            }
        }

        /// <summary>
        /// Gets the list of permissions for the content item
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public IEnumerable<EntityPermission> GetPermissionsForEntity(IContent content)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repo = uow.CreateRepository<IContentRepository>();
                var perms = repo.GetPermissionsForEntity(content.Id);
                uow.Complete();
                return perms;
            }
        }

        #endregion

        #region Create

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
        public IContent CreateContent(string name, int parentId, string contentTypeAlias, int userId = 0)
        {
            var contentType = GetContentType(contentTypeAlias);
            if (contentType == null)
                throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias));
            var parent = parentId > 0 ? GetById(parentId) : null;
            if (parentId > 0 && parent == null)
                throw new ArgumentException("No content with that id.", nameof(parentId));

            var content = new Content(name, parentId, contentType);
            CreateContent(null, content, parent, userId, false);

            return content;
        }

        /// <summary>
        /// Creates an <see cref="IContent"/> object of a specified content type, at root.
        /// </summary>
        /// <remarks>This method simply returns a new, non-persisted, IContent without any identity. It
        /// is intended as a shortcut to creating new content objects that does not invoke a save
        /// operation against the database.
        /// </remarks>
        /// <param name="name">The name of the content object.</param>
        /// <param name="contentTypeAlias">The alias of the content type.</param>
        /// <param name="userId">The optional id of the user creating the content.</param>
        /// <returns>The content object.</returns>
        public IContent CreateContent(string name, string contentTypeAlias, int userId = 0)
        {
            // not locking since not saving anything

            var contentType = GetContentType(contentTypeAlias);
            if (contentType == null)
                throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias));

            var content = new Content(name, -1, contentType);
            CreateContent(null, content, null, userId, false);

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
        public IContent CreateContent(string name, IContent parent, string contentTypeAlias, int userId = 0)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                // not locking since not saving anything

                var contentType = GetContentType(contentTypeAlias);
                if (contentType == null)
                    throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias)); // causes rollback

                var content = new Content(name, parent, contentType);
                CreateContent(uow, content, parent, userId, false);

                uow.Complete();
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
        public IContent CreateContentWithIdentity(string name, int parentId, string contentTypeAlias, int userId = 0)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                // locking the content tree secures content types too
                uow.WriteLock(Constants.Locks.ContentTree);

                var contentType = GetContentType(contentTypeAlias); // + locks
                if (contentType == null)
                    throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias)); // causes rollback

                var parent = parentId > 0 ? GetById(parentId) : null; // + locks
                if (parentId > 0 && parent == null)
                    throw new ArgumentException("No content with that id.", nameof(parentId)); // causes rollback

                var content = parentId > 0 ? new Content(name, parent, contentType) : new Content(name, parentId, contentType);
                CreateContent(uow, content, parent, userId, true);

                uow.Complete();
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
        public IContent CreateContentWithIdentity(string name, IContent parent, string contentTypeAlias, int userId = 0)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                // locking the content tree secures content types too
                uow.WriteLock(Constants.Locks.ContentTree);

                var contentType = GetContentType(contentTypeAlias); // + locks
                if (contentType == null)
                    throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias)); // causes rollback

                var content = new Content(name, parent, contentType);
                CreateContent(uow, content, parent, userId, true);

                uow.Complete();
                return content;
            }
        }

        private void CreateContent(IDatabaseUnitOfWork uow, Content content, IContent parent, int userId, bool withIdentity)
        {
            // NOTE: I really hate the notion of these Creating/Created events - they are so inconsistent, I've only just found
            // out that in these 'WithIdentity' methods, the Saving/Saved events were not fired, wtf. Anyways, they're added now.
            var newArgs = parent != null
                ? new NewEventArgs<IContent>(content, content.ContentType.Alias, parent)
                : new NewEventArgs<IContent>(content, content.ContentType.Alias, -1);

            if (Creating.IsRaisedEventCancelled(newArgs, this))
            {
                content.WasCancelled = true;
                return;
            }

            content.CreatorId = userId;
            content.WriterId = userId;

            if (withIdentity)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IContent>(content), this))
                {
                    content.WasCancelled = true;
                    return;
                }

                var repo = uow.CreateRepository<IContentRepository>();
                repo.AddOrUpdate(content);

                uow.Flush(); // need everything so we can serialize
                repo.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));

                Saved.RaiseEvent(new SaveEventArgs<IContent>(content, false), this);
            }

            Created.RaiseEvent(new NewEventArgs<IContent>(content, false, content.ContentType.Alias, parent), this);

            var msg = withIdentity
                ? "Content '{0}' was created with Id {1}"
                : "Content '{0}' was created";
            Audit(AuditType.New, string.Format(msg, content.Name, content.Id), content.CreatorId, content.Id);
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var content = repository.Get(id);
                uow.Complete();
                return content;
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var items = repository.GetAll(idsA);
                uow.Complete();
                return items;
            }
        }

        /// <summary>
        /// Gets an <see cref="IContent"/> object by its 'UniqueId'
        /// </summary>
        /// <param name="key">Guid key of the Content to retrieve</param>
        /// <returns><see cref="IContent"/></returns>
        public IContent GetById(Guid key)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.Key == key);
                var content = repository.GetByQuery(query).SingleOrDefault();
                uow.Complete();
                return content;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by the Id of the <see cref="IContentType"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentOfContentType(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.ContentTypeId == id);
                var items = repository.GetByQuery(query);
                uow.Complete();
                return items;
            }
        }

        internal IEnumerable<IContent> GetPublishedContentOfContentType(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.ContentTypeId == id);
                var content = repository.GetByPublishedVersion(query);
                uow.Complete();
                return content;
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.Level == level && x.Trashed == false);
                var items = repository.GetByQuery(query);
                uow.Complete();
                return items;
            }
        }

        /// <summary>
        /// Gets a specific version of an <see cref="IContent"/> item.
        /// </summary>
        /// <param name="versionId">Id of the version to retrieve</param>
        /// <returns>An <see cref="IContent"/> item</returns>
        public IContent GetByVersion(Guid versionId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var content = repository.GetByVersion(versionId);
                uow.Complete();
                return content;
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects versions by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetVersions(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var versions = repository.GetAllVersions(id);
                uow.Complete();
                return versions;
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var ancestors = repository.GetAll(ids);
                uow.Complete();
                return ancestors;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetChildren(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.ParentId == id);
                var children = repository.GetByQuery(query).OrderBy(x => x.SortOrder);
                uow.Complete();
                return children;
            }
        }

        /// <summary>
        /// Gets a collection of published <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <returns>An Enumerable list of published <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetPublishedChildren(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.ParentId == id && x.Published);
                var children = repository.GetByQuery(query).OrderBy(x => x.SortOrder);
                uow.Complete();
                return children;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <param name="pageIndex">Page index (zero based)</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalChildren">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren,
            string orderBy, Direction orderDirection, string filter = "")
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var filterQuery = filter.IsNullOrWhiteSpace() 
                    ? null
                    : repository.QueryFactory.Create<IContent>().Where(x => x.Name.Contains(filter));
                return GetPagedChildren(id, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, true, filterQuery);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <param name="pageIndex">Page index (zero based)</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalChildren">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
        /// <param name="filter"></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IContent> filter)
        {
            Mandate.ParameterCondition(pageIndex >= 0, "pageIndex");
            Mandate.ParameterCondition(pageSize > 0, "pageSize");

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();

                var query = repository.Query;
                //if the id is System Root, then just get all - NO! does not make sense!
                //if (id != Constants.System.Root)
                query.Where(x => x.ParentId == id);
                var children = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, orderBySystemField, filter);
                uow.Complete();
                return children;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Descendants from</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalChildren">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, string orderBy = "Path", Direction orderDirection = Direction.Ascending, string filter = "")
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var filterQuery = filter.IsNullOrWhiteSpace()
                    ? null
                    : repository.QueryFactory.Create<IContent>().Where(x => x.Name.Contains(filter));
                return GetPagedDescendants(id, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, true, filterQuery);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Descendants from</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalChildren">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
        /// <param name="filter">Search filter</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IContent> filter)
        {
            Mandate.ParameterCondition(pageIndex >= 0, nameof(pageIndex));
            Mandate.ParameterCondition(pageSize > 0, nameof(pageSize));

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();

                var query = repository.Query;
                //if the id is System Root, then just get all
                if (id != Constants.System.Root)
                    query.Where(x => x.Path.SqlContains($",{id},", TextColumnType.NVarchar));
                var descendants = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, orderBySystemField, filter);
                uow.Complete();
                return descendants;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by its name or partial name
        /// </summary>
        /// <param name="parentId">Id of the Parent to retrieve Children from</param>
        /// <param name="name">Full or partial name of the children</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetChildrenByName(int parentId, string name)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.ParentId == parentId && x.Name.Contains(name));
                var children = repository.GetByQuery(query);
                uow.Complete();
                return children;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Descendants from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetDescendants(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var content = GetById(id);
                if (content == null)
                {
                    uow.Complete(); // else causes rollback
                    return Enumerable.Empty<IContent>();
                }
                var pathMatch = content.Path + ",";
                var query = repository.Query.Where(x => x.Id != content.Id && x.Path.StartsWith(pathMatch));
                var descendants = repository.GetByQuery(query);
                uow.Complete();
                return descendants;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="content"><see cref="IContent"/> item to retrieve Descendants from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetDescendants(IContent content)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var pathMatch = content.Path + ",";
                var query = repository.Query.Where(x => x.Id != content.Id && x.Path.StartsWith(pathMatch));
                var descendants = repository.GetByQuery(query);
                uow.Complete();
                return descendants;
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
        /// Gets the published version of an <see cref="IContent"/> item
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> to retrieve version from</param>
        /// <returns>An <see cref="IContent"/> item</returns>
        public IContent GetPublishedVersion(int id)
        {
            var version = GetVersions(id);
            return version.FirstOrDefault(x => x.Published);
        }

        /// <summary>
        /// Gets the published version of a <see cref="IContent"/> item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <returns>The published version, if any; otherwise, null.</returns>
        public IContent GetPublishedVersion(IContent content)
        {
            if (content.Published) return content;
            return content.HasPublishedVersion
                ? GetByVersion(content.PublishedVersionGuid)
                : null;
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetRootContent()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.ParentId == Constants.System.Root);
                var items = repository.GetByQuery(query);
                uow.Complete();
                return items;
            }
        }

        /// <summary>
        /// Gets all published content items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<IContent> GetAllPublished()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.Trashed == false);
                var content = repository.GetByPublishedVersion(query);
                uow.Complete();
                return content;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has an expiration date less than or equal to today.
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentForExpiration()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var content = GetContentForExpiration(uow);
                uow.Complete();
                return content;
            }
        }

        private IEnumerable<IContent> GetContentForExpiration(IDatabaseUnitOfWork uow)
        {
            var repository = uow.CreateRepository<IContentRepository>();
            var query = repository.Query.Where(x => x.Published && x.ExpireDate <= DateTime.Now);
            return repository.GetByQuery(query);
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has a release date less than or equal to today.
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentForRelease()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var content = GetContentForRelease(uow);
                uow.Complete();
                return content;
            }
        }

        private IEnumerable<IContent> GetContentForRelease(IDatabaseUnitOfWork uow)
        {
            var repository = uow.CreateRepository<IContentRepository>();
            var query = repository.Query.Where(x => x.Published == false && x.ReleaseDate <= DateTime.Now);
            return repository.GetByQuery(query);
        }

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentInRecycleBin()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var bin = $"{Constants.System.Root},{Constants.System.RecycleBinContent},";
                var query = repository.Query.Where(x => x.Path.StartsWith(bin));
                var content = repository.GetByQuery(query);
                uow.Complete();
                return content;
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
        /// Checks whether an <see cref="IContent"/> item has any published versions
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/></param>
        /// <returns>True if the content has any published version otherwise False</returns>
        public bool HasPublishedVersion(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.Published && x.Id == id && x.Trashed == false);
                var count = repository.Count(query);
                uow.Complete();
                return count > 0;
            }
        }

        /// <summary>
        /// Checks if the passed in <see cref="IContent"/> can be published based on the anscestors publish state.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to check if anscestors are published</param>
        /// <returns>True if the Content can be published, otherwise False</returns>
        public bool IsPublishable(IContent content)
        {
            // fast
            if (content.ParentId == Constants.System.Root) return true; // root content is always publishable
            if (content.Trashed) return false; // trashed content is never publishable

            // not trashed and has a parent: publishable if the parent is path-published
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repo = uow.CreateRepository<IContentRepository>();
                var parent = repo.Get(content.ParentId);
                if (parent == null)
                    throw new Exception("Out of sync."); // causes rollback
                var isPublishable = repo.IsPathPublished(parent);
                uow.Complete();
                return isPublishable;
            }
        }

        public bool IsPathPublished(IContent content)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repo = uow.CreateRepository<IContentRepository>();
                var isPathPublished = repo.IsPathPublished(content);
                uow.Complete();
                return isPathPublished;
            }
        }

        #endregion

        #region Save, Publish, Unpublish

        /// <summary>
        /// This will rebuild the xml structures for content in the database.
        /// </summary>
        /// <param name="userId">This is not used for anything</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        /// <remarks>
        /// This is used for when a document type alias or a document type property is changed, the xml will need to
        /// be regenerated.
        /// </remarks>
        public bool RePublishAll(int userId = 0)
        {
            try
            {
                RebuildXmlStructures();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error<ContentService>("An error occurred executing RePublishAll", ex);
                return false;
            }
        }

        /// <summary>
        /// This will rebuild the xml structures for content in the database.
        /// </summary>
        /// <param name="contentTypeIds">
        /// If specified will only rebuild the xml for the content type's specified, otherwise will update the structure
        /// for all published content.
        /// </param>
        internal void RePublishAll(params int[] contentTypeIds)
        {
            try
            {
                RebuildXmlStructures(contentTypeIds);
            }
            catch (Exception ex)
            {
                Logger.Error<ContentService>("An error occurred executing RePublishAll", ex);
            }
        }

        /// <summary>
        /// Saves a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public void Save(IContent content, int userId = 0, bool raiseEvents = true)
        {
            ((IContentServiceOperations) this).Save(content, userId, raiseEvents);
        }

        /// <summary>
        /// Saves a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        Attempt<OperationStatus> IContentServiceOperations.Save(IContent content, int userId, bool raiseEvents)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (raiseEvents && Saving.IsRaisedEventCancelled(new SaveEventArgs<IContent>(content, evtMsgs), this))
                return OperationStatus.Attempt.Cancel(evtMsgs);

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);

                var repository = uow.CreateRepository<IContentRepository>();
                if (content.HasIdentity == false)
                    content.CreatorId = userId;
                content.WriterId = userId;

                // saving the Published version => indicate we are .Saving
                // saving the Unpublished version => remains .Unpublished
                if (content.Published)
                    content.ChangePublishedState(PublishedState.Saving);

                repository.AddOrUpdate(content);
                repository.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));

                uow.Complete();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IContent>(content, false, evtMsgs), this);

            Audit(AuditType.Save, "Save Content performed by user", userId, content.Id);

            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        /// <summary>
        /// Saves a collection of <see cref="IContent"/> objects.
        /// </summary>
        /// <remarks>
        /// If the collection of content contains new objects that references eachother by Id or ParentId,
        /// then use the overload Save method with a collection of Lazy <see cref="IContent"/>.
        /// </remarks>
        /// <param name="contents">Collection of <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public void Save(IEnumerable<IContent> contents, int userId = 0, bool raiseEvents = true)
        {
            ((IContentServiceOperations) this).Save(contents, userId, raiseEvents);
        }

        /// <summary>
        /// Saves a collection of <see cref="IContent"/> objects.
        /// </summary>
        /// <param name="contents">Collection of <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        Attempt<OperationStatus> IContentServiceOperations.Save(IEnumerable<IContent> contents, int userId, bool raiseEvents)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var contentsA = contents.ToArray();

            if (raiseEvents && Saving.IsRaisedEventCancelled(new SaveEventArgs<IContent>(contentsA, evtMsgs), this))
                return OperationStatus.Attempt.Cancel(evtMsgs);

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                foreach (var content in contentsA)
                {
                    if (content.HasIdentity == false)
                        content.CreatorId = userId;
                    content.WriterId = userId;

                    // saving the Published version => indicate we are .Saving
                    // saving the Unpublished version => remains .Unpublished
                    if (content.Published)
                        content.ChangePublishedState(PublishedState.Saving);

                    repository.AddOrUpdate(content);
                    repository.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                }

                uow.Complete();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IContent>(contentsA, false, evtMsgs), this);
            Audit(AuditType.Save, "Bulk Save content performed by user", userId == -1 ? 0 : userId, Constants.System.Root);

            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        /// <summary>
        /// Saves and Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save and publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise save events.</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        [Obsolete("Use SaveAndPublishWithStatus instead, that method will provide more detailed information on the outcome")]
        public bool SaveAndPublish(IContent content, int userId = 0, bool raiseEvents = true)
        {
            var result = SaveAndPublishDo(content, userId, raiseEvents);
            return result.Success;
        }

        /// <summary>
        /// Saves and Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save and publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise save events.</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        Attempt<PublishStatus> IContentServiceOperations.SaveAndPublish(IContent content, int userId, bool raiseEvents)
        {
            return SaveAndPublishDo(content, userId, raiseEvents);
        }

        /// <summary>
        /// Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public bool Publish(IContent content, int userId = 0)
        {
            var result = SaveAndPublishDo(content, userId);
            Logger.Info<ContentService>("Call was made to ContentService.Publish, use PublishWithStatus instead since that method will provide more detailed information on the outcome");
            return result.Success;
        }

        /// <summary>
        /// Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>The published status attempt</returns>
        Attempt<PublishStatus> IContentServiceOperations.Publish(IContent content, int userId)
        {
            return SaveAndPublishDo(content, userId);
        }

        /// <summary>
        /// UnPublishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if unpublishing succeeded, otherwise False</returns>
        public bool UnPublish(IContent content, int userId = 0)
        {
            return ((IContentServiceOperations)this).UnPublish(content, userId).Success;
        }

        /// <summary>
        /// UnPublishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if unpublishing succeeded, otherwise False</returns>
        Attempt<UnPublishStatus> IContentServiceOperations.UnPublish(IContent content, int userId)
        {
            return UnPublishDo(content, false, userId);
        }

        /// <summary>
        /// Saves and Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save and publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise save events.</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public Attempt<PublishStatus> SaveAndPublishWithStatus(IContent content, int userId = 0, bool raiseEvents = true)
        {
            return ((IContentServiceOperations)this).SaveAndPublish(content, userId, raiseEvents);
        }

        /// <summary>
        /// Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public Attempt<PublishStatus> PublishWithStatus(IContent content, int userId = 0)
        {
            return ((IContentServiceOperations) this).Publish(content, userId);
        }

        /// <summary>
        /// Publishes a <see cref="IContent"/> object and all its children
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish along with its children</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        [Obsolete("Use PublishWithChildrenWithStatus instead, that method will provide more detailed information on the outcome and also allows the includeUnpublished flag")]
        public bool PublishWithChildren(IContent content, int userId = 0)
        {
            // this used to just return false only when the parent content failed, otherwise would
            // always return true so we'll do the same thing for the moment

            var result = PublishWithChildrenDo(content, userId, true);

            // FirstOrDefault() is a pain to use with structs and result contain Attempt structs
            // so use this code, which is fast and works - and please ReSharper do NOT suggest otherwise
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var r in result)
                if (r.Result.ContentItem.Id == content.Id) return r.Success;
            return false;
        }

        /// <summary>
        /// Publishes a <see cref="IContent"/> object and all its children
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish along with its children</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="includeUnpublished">set to true if you want to also publish children that are currently unpublished</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public IEnumerable<Attempt<PublishStatus>> PublishWithChildrenWithStatus(IContent content, int userId = 0, bool includeUnpublished = false)
        {
            return ((IContentServiceOperations)this).PublishWithChildren(content, userId, includeUnpublished);
        }

        /// <summary>
        /// Used to perform scheduled publishing/unpublishing
        /// </summary>
        public IEnumerable<Attempt<PublishStatus>> PerformScheduledPublish()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);

                foreach (var d in GetContentForRelease(uow))
                {
                    d.ReleaseDate = null;
                    var result = ((IContentServiceOperations) this).SaveAndPublish(d, d.WriterId);
                    if (result.Success == false)
                    {
                        if (result.Exception != null)
                        {
                            Logger.Error<ContentService>("Could not published the document (" + d.Id + ") based on it's scheduled release, status result: " + result.Result.StatusType, result.Exception);
                        }
                        else
                        {
                            Logger.Warn<ContentService>("Could not published the document (" + d.Id + ") based on it's scheduled release. Status result: " + result.Result.StatusType);
                        }
                    }
                    yield return result;
                }
                foreach (var d in GetContentForExpiration(uow))
                {
                    try
                    {
                        d.ExpireDate = null;
                        ((IContentServiceOperations) this).UnPublish(d, d.WriterId);
                    }
                    catch (Exception ee)
                    {
                        Logger.Error<ContentService>($"Error unpublishing node {d.Id}", ee);
                        throw;
                    }
                }

                uow.Complete();
            }
        }

        /// <summary>
        /// Publishes a <see cref="IContent"/> object and all its children
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish along with its children</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="includeUnpublished"></param>
        /// <returns>The list of statuses for all published items</returns>
        IEnumerable<Attempt<PublishStatus>> IContentServiceOperations.PublishWithChildren(IContent content, int userId, bool includeUnpublished)
        {
            return PublishWithChildrenDo(content, userId, includeUnpublished);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Permanently deletes an <see cref="IContent"/> object as well as all of its Children.
        /// </summary>
        /// <remarks>
        /// This method will also delete associated media files, child content and possibly associated domains.
        /// </remarks>
        /// <remarks>Please note that this method will completely remove the Content from the database</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
        public void Delete(IContent content, int userId = 0)
        {
            ((IContentServiceOperations) this).Delete(content, userId);
        }

        /// <summary>
        /// Permanently deletes an <see cref="IContent"/> object.
        /// </summary>
        /// <remarks>
        /// This method will also delete associated media files, child content and possibly associated domains.
        /// </remarks>
        /// <remarks>Please note that this method will completely remove the Content from the database</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
        Attempt<OperationStatus> IContentServiceOperations.Delete(IContent content, int userId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IContent>(content, evtMsgs), this))
                return OperationStatus.Attempt.Cancel(evtMsgs);

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();

                // if it's not trashed yet, and published, we should unpublish
                // but... UnPublishing event makes no sense (not going to cancel?) and no need to save
                // just raise the event
                if (content.Trashed == false && content.HasPublishedVersion)
                    UnPublished.RaiseEvent(new PublishEventArgs<IContent>(content, false, false), this);

                DeleteLocked(repository, content);
                uow.Complete();
            }

            Audit(AuditType.Delete, "Delete Content performed by user", userId, content.Id);

            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        private void DeleteLocked(IContentRepository repository, IContent content)
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

                repository.Delete(c);
                var args = new DeleteEventArgs<IContent>(c, false); // raise event & get flagged files
                Deleted.RaiseEvent(args, this);

                IOHelper.DeleteFiles(args.MediaFilesToDelete, // remove flagged files
                    (file, e) => Logger.Error<ContentService>("An error occurred while deleting file attached to nodes: " + file, e));
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
            if (DeletingVersions.IsRaisedEventCancelled(new DeleteRevisionsEventArgs(id, dateToRetain: versionDate), this))
                return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                repository.DeleteVersions(id, versionDate);
                uow.Complete();
            }

            DeletedVersions.RaiseEvent(new DeleteRevisionsEventArgs(id, false, dateToRetain: versionDate), this);

            Audit(AuditType.Delete, "Delete Content by version date performed by user", userId, Constants.System.Root);
        }

        /// <summary>
        /// Permanently deletes specific version(s) from an <see cref="IContent"/> object.
        /// This method will never delete the latest version of a content item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> object to delete a version from</param>
        /// <param name="versionId">Id of the version to delete</param>
        /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        public void DeleteVersion(int id, Guid versionId, bool deletePriorVersions, int userId = 0)
        {
            if (DeletingVersions.IsRaisedEventCancelled(new DeleteRevisionsEventArgs(id, /*specificVersion:*/ versionId), this))
                return;

            if (deletePriorVersions)
            {
                var content = GetByVersion(versionId);
                DeleteVersions(id, content.UpdateDate, userId);
            }

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                repository.DeleteVersion(versionId);
                uow.Complete();
            }

            DeletedVersions.RaiseEvent(new DeleteRevisionsEventArgs(id, false,/* specificVersion:*/ versionId), this);

            Audit(AuditType.Delete, "Delete Content by version performed by user", userId, Constants.System.Root);
        }

        #endregion

        #region Move, RecycleBin

        /// <summary>
        /// Deletes an <see cref="IContent"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <remarks>Move an item to the Recycle Bin will result in the item being unpublished</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
        public void MoveToRecycleBin(IContent content, int userId = 0)
        {
            ((IContentServiceOperations) this).MoveToRecycleBin(content, userId);
        }

        /// <summary>
        /// Deletes an <see cref="IContent"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <remarks>Move an item to the Recycle Bin will result in the item being unpublished</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
        Attempt<OperationStatus> IContentServiceOperations.MoveToRecycleBin(IContent content, int userId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var moves = new List<Tuple<IContent, string>>();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();

                var originalPath = content.Path;
                if (Trashing.IsRaisedEventCancelled(new MoveEventArgs<IContent>(new MoveEventInfo<IContent>(content, originalPath, Constants.System.RecycleBinContent)), this))
                    return OperationStatus.Attempt.Cancel(evtMsgs); // causes rollback

                // if it's published we may want to force-unpublish it - that would be backward-compatible... but...
                // making a radical decision here: trashing is equivalent to moving under an unpublished node so
                // it's NOT unpublishing, only the content is now masked - allowing us to restore it if wanted
                //if (content.HasPublishedVersion)
                //{ }

                PerformMoveLocked(repository, content, Constants.System.RecycleBinContent, null, userId, moves, true);
                uow.Complete();
            }

            var moveInfo = moves
                .Select(x => new MoveEventInfo<IContent>(x.Item1, x.Item2, x.Item1.ParentId))
                .ToArray();

            Trashed.RaiseEvent(new MoveEventArgs<IContent>(false, evtMsgs, moveInfo), this);
            Audit(AuditType.Move, "Move Content to Recycle Bin performed by user", userId, content.Id);

            return OperationStatus.Attempt.Succeed(evtMsgs);
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();

                var parent = parentId == Constants.System.Root ? null : GetById(parentId);
                if (parentId != Constants.System.Root && (parent == null || parent.Trashed))
                    throw new InvalidOperationException("Parent does not exist or is trashed."); // causes rollback

                if (Moving.IsRaisedEventCancelled(new MoveEventArgs<IContent>(new MoveEventInfo<IContent>(content, content.Path, parentId)), this))
                    return; // causes rollback

                // if content was trashed, and since we're not moving to the recycle bin,
                // indicate that the trashed status should be changed to false, else just
                // leave it unchanged
                var trashed = content.Trashed ? false : (bool?)null;

                // if the content was trashed under another content, and so has a published version,
                // it cannot move back as published but has to be unpublished first - that's for the
                // root content, everything underneath will retain its published status
                if (content.Trashed && content.HasPublishedVersion)
                {
                    // however, it had been masked when being trashed, so there's no need for
                    // any special event here - just change its state
                    content.ChangePublishedState(PublishedState.Unpublishing);
                }

                PerformMoveLocked(repository, content, parentId, parent, userId, moves, trashed);

                uow.Complete();
            }

            var moveInfo = moves //changes
                .Select(x => new MoveEventInfo<IContent>(x.Item1, x.Item2, x.Item1.ParentId))
                .ToArray();

            Moved.RaiseEvent(new MoveEventArgs<IContent>(false, moveInfo), this);

            Audit(AuditType.Move, "Move Content performed by user", userId, content.Id);
        }

        // MUST be called from within WriteLock
        // trash indicates whether we are trashing, un-trashing, or not changing anything
        private void PerformMoveLocked(IContentRepository repository,
            IContent content, int parentId, IContent parent, int userId,
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

            // these will be updated by the repo because we changed parentId
            //content.Path = (parent == null ? "-1" : parent.Path) + "," + content.Id;
            //content.SortOrder = ((ContentRepository) repository).NextChildSortOrder(parentId);
            //content.Level += levelDelta;
            PerformMoveContentLocked(repository, content, userId, trash);

            // BUT content.Path will be updated only when the UOW commits, and
            //  because we want it now, we have to calculate it by ourselves
            //paths[content.Id] = content.Path;
            paths[content.Id] = (parent == null ? (parentId == Constants.System.RecycleBinContent ? "-1,-20" : "-1") : parent.Path) + "," + content.Id;

            var descendants = GetDescendants(content);
            foreach (var descendant in descendants)
            {
                moves.Add(Tuple.Create(descendant, descendant.Path)); // capture original path

                // update path and level since we do not update parentId
                descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
                descendant.Level += levelDelta;
                PerformMoveContentLocked(repository, descendant, userId, trash);
            }
        }

        private static void PerformMoveContentLocked(IContentRepository repository, IContent content, int userId,
            bool? trash)
        {
            if (trash.HasValue) ((ContentBase) content).Trashed = trash.Value;
            content.WriterId = userId;
            repository.AddOrUpdate(content);
        }

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IContent"/> that resides in the bin
        /// </summary>
        public void EmptyRecycleBin()
        {
            var nodeObjectType = new Guid(Constants.ObjectTypes.Document);
            var deleted = new List<IContent>();
            var evtMsgs = EventMessagesFactory.Get(); // todo - and then?

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();

                // v7 EmptyingRecycleBin and EmptiedRecycleBin events are greatly simplified since
                // each deleted items will have its own deleting/deleted events. so, files and such
                // are managed by Delete, and not here.

                // no idea what those events are for, keep a simplified version
                if (EmptyingRecycleBin.IsRaisedEventCancelled(new RecycleBinEventArgs(nodeObjectType), this))
                    return; // causes rollback

                // emptying the recycle bin means deleting whetever is in there - do it properly!
                var query = repository.Query.Where(x => x.ParentId == Constants.System.RecycleBinContent);
                var contents = repository.GetByQuery(query).ToArray();
                foreach (var content in contents)
                {
                    DeleteLocked(repository, content);
                    deleted.Add(content);
                }

                EmptiedRecycleBin.RaiseEvent(new RecycleBinEventArgs(nodeObjectType, true), this);
                uow.Complete();
            }

            Audit(AuditType.Delete, "Empty Content Recycle Bin performed by user", 0, Constants.System.RecycleBinContent);
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

            if (Copying.IsRaisedEventCancelled(new CopyEventArgs<IContent>(content, copy, parentId), this))
                return null;

            // fixme - relateToOriginal is ignored?!

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();

                // a copy is .Saving and will be .Unpublished
                if (copy.Published)
                    copy.ChangePublishedState(PublishedState.Saving);

                // update the create author and last edit author
                copy.CreatorId = userId;
                copy.WriterId = userId;

                // save
                repository.AddOrUpdate(copy);
                repository.AddOrUpdatePreviewXml(copy, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));

                uow.Flush(); // ensure copy has an ID - fixme why?

                if (recursive)
                {
                    // process descendants
                    var copyIds = new Dictionary<int, IContent>();
                    copyIds[content.Id] = copy;
                    foreach (var descendant in GetDescendants(content))
                    {
                        var dcopy = descendant.DeepCloneWithResetIdentities();
                        //dcopy.ParentId = copyIds[descendant.ParentId];
                        var descendantParentId = descendant.ParentId;
                        ((Content) dcopy).SetLazyParentId(new Lazy<int>(() => copyIds[descendantParentId].Id));

                        if (dcopy.Published)
                            dcopy.ChangePublishedState(PublishedState.Saving);
                        dcopy.CreatorId = userId;
                        dcopy.WriterId = userId;

                        repository.AddOrUpdate(dcopy);
                        repository.AddOrUpdatePreviewXml(dcopy, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));

                        copyIds[descendant.Id] = dcopy;
                    }
                }

                // fixme tag & tree issue
                // tags code handling has been removed here
                // - tags should be handled by the content repository
                // - a copy is unpublished and therefore has no impact on tags in DB

                uow.Complete();
            }

            Copied.RaiseEvent(new CopyEventArgs<IContent>(content, copy, false, parentId, relateToOriginal), this);
            Audit(AuditType.Copy, "Copy Content performed by user", content.WriterId, content.Id);
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
            if (SendingToPublish.IsRaisedEventCancelled(new SendToPublishEventArgs<IContent>(content), this))
                return false;

            //Save before raising event
            Save(content, userId);

            SentToPublish.RaiseEvent(new SendToPublishEventArgs<IContent>(content, false), this);

            Audit(AuditType.SendToPublish, "Send to Publish performed by user", content.WriterId, content.Id);

            return true;
        }

        /// <summary>
        /// Rollback an <see cref="IContent"/> object to a previous version.
        /// This will create a new version, which is a copy of all the old data.
        /// </summary>
        /// <remarks>
        /// The way data is stored actually only allows us to rollback on properties
        /// and not data like Name and Alias of the Content.
        /// </remarks>
        /// <param name="id">Id of the <see cref="IContent"/>being rolled back</param>
        /// <param name="versionId">Id of the version to rollback to</param>
        /// <param name="userId">Optional Id of the User issueing the rollback of the Content</param>
        /// <returns>The newly created <see cref="IContent"/> object</returns>
        public IContent Rollback(int id, Guid versionId, int userId = 0)
        {
            var content = GetByVersion(versionId);

            if (RollingBack.IsRaisedEventCancelled(new RollbackEventArgs<IContent>(content), this))
                return content;

            content.CreatorId = userId;

            // need to make sure that the repository is going to save a new version
            // but if we're not changing anything, the repository would not save anything
            // so - make sure the property IS dirty, doing a flip-flop with an impossible value
            content.WriterId = -1;
            content.WriterId = userId;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();

                // a rolled back version is .Saving and will be .Unpublished
                content.ChangePublishedState(PublishedState.Saving);

                repository.AddOrUpdate(content);
                repository.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                uow.Complete();
            }

            RolledBack.RaiseEvent(new RollbackEventArgs<IContent>(content, false), this);
            Audit(AuditType.RollBack, "Content rollback performed by user", content.WriterId, content.Id);

            return content;
        }

        /// <summary>
        /// Sorts a collection of <see cref="IContent"/> objects by updating the SortOrder according
        /// to the ordering of items in the passed in <see cref="IEnumerable{T}"/>.
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

            if (raiseEvents && Saving.IsRaisedEventCancelled(new SaveEventArgs<IContent>(itemsA), this))
                    return false;

            var published = new List<IContent>();
            var saved = new List<IContent>();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
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
                    else if (content.HasPublishedVersion)
                        published.Add(GetByVersion(content.PublishedVersionGuid));

                    // save
                    saved.Add(content);
                    repository.AddOrUpdate(content);
                    repository.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                }

                foreach (var content in published)
                    repository.AddOrUpdateContentXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));

                uow.Complete();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IContent>(saved, false), this);

            if (raiseEvents && published.Any())
                Published.RaiseEvent(new PublishEventArgs<IContent>(published, false, false), this);

            Audit(AuditType.Sort, "Sorting content performed by user", userId, 0);

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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var descendants = GetPublishedDescendantsLocked(repository, content);
                uow.Complete();
                return descendants;
            }
        }

        internal IEnumerable<IContent> GetPublishedDescendantsLocked(IContentRepository repository, IContent content)
        {
            var pathMatch = content.Path + ",";
            var query = repository.Query.Where(x => x.Id != content.Id && x.Path.StartsWith(pathMatch) /*&& x.Trashed == false*/);
            var contents = repository.GetByPublishedVersion(query);

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

        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                repo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
                uow.Complete();
            }
        }

        /// <summary>
        /// Publishes a <see cref="IContent"/> object and all its children
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish along with its children</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="includeUnpublished">If set to true, this will also publish descendants that are completely unpublished, normally this will only publish children that have previously been published</param>
        /// <returns>
        /// A list of publish statues. If the parent document is not valid or cannot be published because it's parent(s) is not published
        /// then the list will only contain one status item, otherwise it will contain status items for it and all of it's descendants that
        /// are to be published.
        /// </returns>
        private IEnumerable<Attempt<PublishStatus>> PublishWithChildrenDo(IContent content, int userId = 0, bool includeUnpublished = false)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var evtMsgs = EventMessagesFactory.Get();
            var publishedItems = new List<IContent>(); // this is for events
            Attempt<PublishStatus>[] attempts;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();

                // fail fast + use in alreadyChecked below to avoid duplicate checks
                var attempt = EnsurePublishable(content, evtMsgs);
                if (attempt.Success)
                    attempt = StrategyCanPublish(content, userId, evtMsgs);
                if (attempt.Success == false)
                    return new[] { attempt }; // causes rollback

                var contents = new List<IContent> { content }; //include parent item
                contents.AddRange(GetDescendants(content));

                // publish using the strategy - for descendants,
                // - published w/out changes: nothing to do
                // - published w/changes: publish those changes
                // - unpublished: publish if includeUnpublished, otherwise ignore
                var alreadyChecked = new[] { content };
                attempts = StrategyPublishWithChildren(contents, alreadyChecked, userId, evtMsgs, includeUnpublished).ToArray();

                foreach (var status in attempts.Where(x => x.Success).Select(x => x.Result))
                {
                    // save them all, even those that are .Success because of (.StatusType == PublishStatusType.SuccessAlreadyPublished)
                    // so we bump the date etc
                    var publishedItem = status.ContentItem;
                    publishedItem.WriterId = userId;
                    repository.AddOrUpdate(publishedItem);
                    repository.AddOrUpdatePreviewXml(publishedItem, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                    repository.AddOrUpdateContentXml(publishedItem, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                    publishedItems.Add(publishedItem);
                }

                uow.Complete();
            }

            Published.RaiseEvent(new PublishEventArgs<IContent>(publishedItems, false, false), this);
            Audit(AuditType.Publish, "Publish with Children performed by user", userId, content.Id);
            return attempts;
        }


        /// <summary>
        /// UnPublishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="omitCacheRefresh">Optional boolean to avoid having the cache refreshed when calling this Unpublish method. By default this method will update the cache.</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if unpublishing succeeded, otherwise False</returns>
        private Attempt<UnPublishStatus> UnPublishDo(IContent content, bool omitCacheRefresh = false, int userId = 0)
        {
            // fixme kill omitCacheRefresh!

            var evtMsgs = EventMessagesFactory.Get();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();

                var newest = GetById(content.Id); // ensure we have the newest version
                if (content.Version != newest.Version) // but use the original object if it's already the newest version
                    content = newest;
                if (content.Published == false && content.HasPublishedVersion == false)
                {
                    uow.Complete();
                    return Attempt.Succeed(new UnPublishStatus(UnPublishedStatusType.SuccessAlreadyUnPublished, evtMsgs, content)); // already unpublished
                }

                // strategy
                var attempt = StrategyCanUnPublish(content, userId, evtMsgs);
                if (attempt == false) return attempt; // causes rollback
                attempt = StrategyUnPublish(content, true, userId, evtMsgs);
                if (attempt == false) return attempt; // causes rollback

                content.WriterId = userId;
                repository.AddOrUpdate(content);
                // fixme delete xml from database! was in  _publishingStrategy.UnPublishingFinalized(content);
                repository.DeleteContentXml(content);

                uow.Complete();
            }

            UnPublished.RaiseEvent(new PublishEventArgs<IContent>(content, false, false), this);
            return Attempt.Succeed(new UnPublishStatus(UnPublishedStatusType.Success, evtMsgs, content));
        }

        /// <summary>
        /// Saves and Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save and publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise save events.</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        private Attempt<PublishStatus> SaveAndPublishDo(IContent content, int userId = 0, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (raiseEvents && Saving.IsRaisedEventCancelled(new SaveEventArgs<IContent>(content), this))
                return Attempt.Fail(new PublishStatus(PublishStatusType.FailedCancelledByEvent, evtMsgs, content));

            var isNew = content.IsNewEntity();
            var previouslyPublished = content.HasIdentity && content.HasPublishedVersion;
            var status = default(Attempt<PublishStatus>);

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();

                // fixme - EnsurePublishable vs StrategyCanPublish?
                // EnsurePublishable ensures that path published is ok
                // StrategyCanPublish ensures other things including valid properties
                // should we merge or?!

                // ensure content is publishable, and try to publish
                status = EnsurePublishable(content, evtMsgs);
                if (status.Success)
                {
                    // strategy handles events, and various business rules eg release & expire
                    // dates, trashed status...
                    status = StrategyPublish(content, false, userId, evtMsgs);
                }

                // save - always, even if not publishing (this is SaveAndPublish)
                if (content.HasIdentity == false)
                    content.CreatorId = userId;
                content.WriterId = userId;

                repository.AddOrUpdate(content);
                repository.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                if (content.Published)
                    repository.AddOrUpdateContentXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));

                uow.Complete();
            }

            if (status.Success == false)
            {
                // fixme what about the saved event?
                return status;
            }

            Published.RaiseEvent(new PublishEventArgs<IContent>(content, false, false), this);

            // if was not published and now is... descendants that were 'published' (but
            // had an unpublished ancestor) are 're-published' ie not explicitely published
            // but back as 'published' nevertheless
            if (isNew == false && previouslyPublished == false)
            {
                if (HasChildren(content.Id))
                {
                    var descendants = GetPublishedDescendants(content).ToArray();
                    Published.RaiseEvent(new PublishEventArgs<IContent>(descendants, false, false), this);
                }
            }

            Audit(AuditType.Publish, "Save and Publish performed by user", userId, content.Id);
            return status;
        }

        private Attempt<PublishStatus> EnsurePublishable(IContent content, EventMessages evtMsgs)
        {
            // root content can be published
            var checkParents = content.ParentId == Constants.System.Root;

            // trashed content cannot be published
            if (checkParents == false && content.ParentId != Constants.System.RecycleBinContent)
            {
                // ensure all ancestors are published
                // because content may be new its Path may be null - start with parent
                var path = content.Path ?? content.Parent(this).Path;
                if (path != null) // if parent is also null, give up
                {
                    var ancestorIds = path.Split(',')
                        .Skip(1) // remove leading "-1"
                        .Reverse()
                        .Select(int.Parse);
                    if (content.Path != null)
                        ancestorIds = ancestorIds.Skip(1); // remove trailing content.Id

                    if (ancestorIds.All(HasPublishedVersion))
                        checkParents = true;
                }
            }

            if (checkParents == false)
            {
                Logger.Info<ContentService>($"Content '{content.Name}' with Id '{content.Id}' could not be published because its parent is not published.");
                return Attempt.Fail(new PublishStatus(PublishStatusType.FailedPathNotPublished, evtMsgs, content));
            }

            // fixme - should we do it - are we doing it for descendants too?
            if (content.IsValid() == false)
            {
                Logger.Info<ContentService>($"Content '{content.Name}' with Id '{content.Id}' could not be published because of invalid properties.");
                return Attempt.Fail(new PublishStatus(PublishStatusType.FailedContentInvalid, evtMsgs, content)
                {
                    InvalidProperties = ((ContentBase)content).LastInvalidProperties
                });
            }

            return Attempt.Succeed(new PublishStatus(PublishStatusType.Success, evtMsgs, content));
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
        /// Occurs before Create
        /// </summary>
        [Obsolete("Use the Created event instead, the Creating and Created events both offer the same functionality, Creating event has been deprecated.")]
        public static event TypedEventHandler<IContentService, NewEventArgs<IContent>> Creating;

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
        public static event TypedEventHandler<IContentService, PublishEventArgs<IContent>> UnPublishing;

        /// <summary>
        /// Occurs after unpublish
        /// </summary>
        public static event TypedEventHandler<IContentService, PublishEventArgs<IContent>> UnPublished;

        #endregion

        #region Publishing Strategies

        // prob. want to find nicer names?

        internal Attempt<PublishStatus> StrategyCanPublish(IContent content, int userId, EventMessages evtMsgs)
        {
            if (Publishing.IsRaisedEventCancelled(new PublishEventArgs<IContent>(content, evtMsgs), this))
            {
                Logger.Info<ContentService>($"Content '{content.Name}' with Id '{content.Id}' will not be published, the event was cancelled.");
                return Attempt.Fail(new PublishStatus(PublishStatusType.FailedCancelledByEvent, evtMsgs, content));
            }

            // check if the content is valid
            if (content.IsValid() == false)
            {
                Logger.Info<ContentService>($"Content '{content.Name}' with Id '{content.Id}' could not be published because of invalid properties.");
                return Attempt.Fail(new PublishStatus(PublishStatusType.FailedContentInvalid, evtMsgs, content)
                {
                    InvalidProperties = ((ContentBase)content).LastInvalidProperties
                });
            }

            // check if the Content is Expired
            if (content.Status == ContentStatus.Expired)
            {
                Logger.Info<ContentService>($"Content '{content.Name}' with Id '{content.Id}' has expired and could not be published.");
                return Attempt.Fail(new PublishStatus(PublishStatusType.FailedHasExpired, evtMsgs, content));
            }

            // check if the Content is Awaiting Release
            if (content.Status == ContentStatus.AwaitingRelease)
            {
                Logger.Info<ContentService>($"Content '{content.Name}' with Id '{content.Id}' is awaiting release and could not be published.");
                return Attempt.Fail(new PublishStatus(PublishStatusType.FailedAwaitingRelease, evtMsgs, content));
            }

            // check if the Content is Trashed
            if (content.Status == ContentStatus.Trashed)
            {
                Logger.Info<ContentService>($"Content '{content.Name}' with Id '{content.Id}' is trashed and could not be published.");
                return Attempt.Fail(new PublishStatus(PublishStatusType.FailedIsTrashed, evtMsgs, content));
            }

            return Attempt.Succeed(new PublishStatus(content, evtMsgs));
        }

        internal Attempt<PublishStatus> StrategyPublish(IContent content, bool alreadyCheckedCanPublish, int userId, EventMessages evtMsgs)
        {
            var attempt = alreadyCheckedCanPublish
                ? Attempt.Succeed(new PublishStatus(content, evtMsgs)) // already know we can
                : StrategyCanPublish(content, userId, evtMsgs); // else check
            if (attempt.Success == false)
                return attempt;

            // change state to publishing
            content.ChangePublishedState(PublishedState.Publishing);

            Logger.Info<ContentService>($"Content '{content.Name}' with Id '{content.Id}' has been published.");

            return attempt;
        }

        ///  <summary>
        ///  Publishes a list of content items
        ///  </summary>
        ///  <param name="contents">Contents, ordered by level ASC</param>
        /// <param name="alreadyChecked">Contents for which we've already checked CanPublish</param>
        /// <param name="userId"></param>
        /// <param name="evtMsgs"></param>
        /// <param name="includeUnpublished">Indicates whether to publish content that is completely unpublished (has no published
        ///  version). If false, will only publish already published content with changes. Also impacts what happens if publishing
        ///  fails (see remarks).</param>
        ///  <returns></returns>
        ///  <remarks>
        ///  Navigate content & descendants top-down and for each,
        ///  - if it is published
        ///    - and unchanged, do nothing
        ///    - else (has changes), publish those changes
        ///  - if it is not published
        ///    - and at top-level, publish
        ///    - or includeUnpublished is true, publish
        ///    - else do nothing & skip the underlying branch
        ///
        ///  When publishing fails
        ///  - if content has no published version, skip the underlying branch
        ///  - else (has published version),
        ///    - if includeUnpublished is true, process the underlying branch
        ///    - else, do not process the underlying branch
        ///  </remarks>
        internal IEnumerable<Attempt<PublishStatus>> StrategyPublishWithChildren(IEnumerable<IContent> contents, IEnumerable<IContent> alreadyChecked, int userId, EventMessages evtMsgs, bool includeUnpublished = true)
        {
            var statuses = new List<Attempt<PublishStatus>>();
            var alreadyCheckedA = (alreadyChecked ?? Enumerable.Empty<IContent>()).ToArray();

            // list of ids that we exclude because they could not be published
            var excude = new List<int>();

            var topLevel = -1;
            foreach (var content in contents)
            {
                // initialize - content is ordered by level ASC
                if (topLevel < 0)
                    topLevel = content.Level;

                if (excude.Contains(content.ParentId))
                {
                    // parent is excluded, so exclude content too
                    Logger.Info<ContentService>($"Content '{content.Name}' with Id '{content.Id}' will not be published because it's parent's publishing action failed or was cancelled.");
                    excude.Add(content.Id);
                    // status has been reported for an ancestor and that one is excluded => no status
                    continue;
                }

                if (content.Published && content.Level > topLevel) // topLevel we DO want to (re)publish
                {
                    // newest is published already
                    statuses.Add(Attempt.Succeed(new PublishStatus(PublishStatusType.SuccessAlreadyPublished, evtMsgs, content)));
                    continue;
                }

                if (content.HasPublishedVersion)
                {
                    // newest is published already but we are topLevel, or
                    // newest is not published, but another version is - publish newest
                    var r = StrategyPublish(content, alreadyCheckedA.Contains(content), userId, evtMsgs);
                    if (r.Success == false)
                    {
                        // we tried to publish and it failed, but it already had / still has a published version,
                        // the rule in remarks says that we should skip the underlying branch if includeUnpublished
                        // is false, else process it - not that it makes much sense, but keep it like that for now
                        if (includeUnpublished == false)
                            excude.Add(content.Id);
                    }

                    statuses.Add(r);
                    continue;
                }

                if (content.Level == topLevel || includeUnpublished)
                {
                    // content has no published version, and we want to publish it, either
                    // because it is top-level or because we include unpublished.
                    // if publishing fails, and because content does not have a published
                    // version at all, ensure we do not process its descendants
                    var r = StrategyPublish(content, alreadyCheckedA.Contains(content), userId, evtMsgs);
                    if (r.Success == false)
                        excude.Add(content.Id);

                    statuses.Add(r);
                    continue;
                }

                // content has no published version, and we don't want to publish it
                excude.Add(content.Id); // ignore everything below it
                // content is not even considered, really => no status
            }

            return statuses;
        }

        internal Attempt<UnPublishStatus> StrategyCanUnPublish(IContent content, int userId, EventMessages evtMsgs)
        {
            // fire UnPublishing event
            if (UnPublishing.IsRaisedEventCancelled(new PublishEventArgs<IContent>(content, evtMsgs), this))
            {
                Logger.Info<ContentService>($"Content '{content.Name}' with Id '{content.Id}' will not be unpublished, the event was cancelled.");
                return Attempt.Fail(new UnPublishStatus(UnPublishedStatusType.FailedCancelledByEvent, evtMsgs, content));
            }

            return Attempt.Succeed(new UnPublishStatus(content, evtMsgs));
        }

        internal Attempt<UnPublishStatus> StrategyUnPublish(IContent content, bool alreadyCheckedCanUnPublish, int userId, EventMessages evtMsgs)
        {
            // content should (is assumed to) be the newest version, which may not be published,
            // don't know how to test this, so it's not verified

            var attempt = alreadyCheckedCanUnPublish
                ? Attempt.Succeed(new UnPublishStatus(content, evtMsgs)) // already know we can
                : StrategyCanUnPublish(content, userId, evtMsgs);
            if (attempt.Success == false)
                return attempt;

            // if Content has a release date set to before now, it should be removed so it doesn't interrupt an unpublish
            // otherwise it would remain released == published
            if (content.ReleaseDate.HasValue && content.ReleaseDate.Value <= DateTime.Now)
            {
                content.ReleaseDate = null;
                Logger.Info<ContentService>($"Content '{content.Name}' with Id '{content.Id}' had its release date removed, because it was unpublished.");
            }

            // version is published or unpublished, but content is published
            // change state to unpublishing
            content.ChangePublishedState(PublishedState.Unpublishing);

            Logger.Info<ContentService>($"Content '{content.Name}' with Id '{content.Id}' has been unpublished.");

            return attempt;
        }

        internal IEnumerable<Attempt<UnPublishStatus>> StrategyUnPublish(IEnumerable<IContent> content, int userId, EventMessages evtMsgs)
        {
            return content.Select(x => StrategyUnPublish(x, false, userId, evtMsgs));
        }

        #endregion

        #region Content Types

        /// <summary>
        /// Deletes all content of specified type. All children of deleted content is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="contentTypeId">Id of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional Id of the user issueing the delete operation</param>
        public void DeleteContentOfType(int contentTypeId, int userId = 0)
        {
            //TODO: This currently this is called from the ContentTypeService but that needs to change,
            // if we are deleting a content type, we should just delete the data and do this operation slightly differently.
            // This method will recursively go lookup every content item, check if any of it's descendants are
            // of a different type, move them to the recycle bin, then permanently delete the content items.
            // The main problem with this is that for every content item being deleted, events are raised...
            // which we need for many things like keeping caches in sync, but we can surely do this MUCH better.

            var moves = new List<Tuple<IContent, string>>();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();

                // fixme what about content that has the contenttype as part of its composition?
                var query = repository.Query.Where(x => x.ContentTypeId == contentTypeId);
                var contents = repository.GetByQuery(query).ToArray();

                if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IContent>(contents), this))
                    return; // causes rollback

                // order by level, descending, so deepest first - that way, we cannot move
                // a content of the deleted type, to the recycle bin (and then delete it...)
                foreach (var content in contents.OrderByDescending(x => x.ParentId))
                {
                    // if it's not trashed yet, and published, we should unpublish
                    // but... UnPublishing event makes no sense (not going to cancel?) and no need to save
                    // just raise the event
                    if (content.Trashed == false && content.HasPublishedVersion)
                        UnPublished.RaiseEvent(new PublishEventArgs<IContent>(content, false, false), this);

                    // if current content has children, move them to trash
                    var c = content;
                    var childQuery = repository.Query.Where(x => x.Path.StartsWith(c.Path));
                    var children = repository.GetByQuery(childQuery);
                    foreach (var child in children.Where(x => x.ContentTypeId != contentTypeId))
                    {
                        // see MoveToRecycleBin
                        PerformMoveLocked(repository, child, Constants.System.RecycleBinContent, null, userId, moves, true);
                    }

                    // delete content
                    // triggers the deleted event (and handles the files)
                    DeleteLocked(repository, content);
                }

                uow.Complete();
            }

            var moveInfos = moves
                .Select(x => new MoveEventInfo<IContent>(x.Item1, x.Item2, x.Item1.ParentId))
                .ToArray();
            if (moveInfos.Length > 0)
                Trashed.RaiseEvent(new MoveEventArgs<IContent>(false, moveInfos), this);

            Audit(AuditType.Delete, $"Delete Content of Type {contentTypeId} performed by user", userId, Constants.System.Root);
        }

        private IContentType GetContentType(string contentTypeAlias)
        {
            Mandate.ParameterNotNullOrEmpty(contentTypeAlias, nameof(contentTypeAlias));

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTypes);

                var repository = uow.CreateRepository<IContentTypeRepository>();
                var query = repository.Query.Where(x => x.Alias == contentTypeAlias);
                var contentType = repository.GetByQuery(query).FirstOrDefault();

                if (contentType == null)
                    throw new Exception($"No ContentType matching the passed in Alias: '{contentTypeAlias}' was found"); // causes rollback

                uow.Complete();
                return contentType;
            }
        }

        #endregion

        #region Xml - Should Move!

        /// <summary>
        /// Returns the persisted content's XML structure
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public XElement GetContentXml(int contentId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var elt = repository.GetContentXml(contentId);
                uow.Complete();
                return elt;
            }
        }

        /// <summary>
        /// Returns the persisted content's preview XML structure
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public XElement GetContentPreviewXml(int contentId, Guid version)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var elt = repository.GetContentPreviewXml(contentId, version);
                uow.Complete();
                return elt;
            }
        }

        /// <summary>
        /// Rebuilds all xml content in the cmsContentXml table for all documents
        /// </summary>
        /// <param name="contentTypeIds">
        /// Only rebuild the xml structures for the content type ids passed in, if none then rebuilds the structures
        /// for all content
        /// </param>
        public void RebuildXmlStructures(params int[] contentTypeIds)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.ContentTree);
                var repository = uow.CreateRepository<IContentRepository>();
                repository.RebuildXmlStructures(
                    content => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, content),
                    contentTypeIds: contentTypeIds.Length == 0 ? null : contentTypeIds);
                uow.Complete();
            }

            Audit(AuditType.Publish, "ContentService.RebuildXmlStructures completed, the xml has been regenerated in the database", 0, Constants.System.Root);
        }

        #endregion
    }
}