using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Umbraco.Core.Auditing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;

using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Content Service, which is an easy access to operations involving <see cref="IContent"/>
    /// </summary>
    public class ContentService : RepositoryService, IContentService, IContentServiceOperations
    {
        private readonly IPublishingStrategy _publishingStrategy;
        private readonly EntityXmlSerializer _entitySerializer = new EntityXmlSerializer();
        private readonly IDataTypeService _dataTypeService;
        private readonly IUserService _userService;
        private readonly IEnumerable<IUrlSegmentProvider> _urlSegmentProviders;

        //Support recursive locks because some of the methods that require locking call other methods that require locking.
        //for example, the Move method needs to be locked but this calls the Save method which also needs to be locked.
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public ContentService(
            IDatabaseUnitOfWorkProvider provider,
            ILogger logger,
            IEventMessagesFactory eventMessagesFactory,
            IPublishingStrategy publishingStrategy,
            IDataTypeService dataTypeService,
            IUserService userService,
            IEnumerable<IUrlSegmentProvider> urlSegmentProviders)
            : base(provider, logger, eventMessagesFactory)
        {
            if (publishingStrategy == null) throw new ArgumentNullException("publishingStrategy");
            if (dataTypeService == null) throw new ArgumentNullException("dataTypeService");
            if (userService == null) throw new ArgumentNullException("userService");
            if (urlSegmentProviders == null) throw new ArgumentNullException("urlSegmentProviders");
            _publishingStrategy = publishingStrategy;
            _dataTypeService = dataTypeService;
            _userService = userService;
            _urlSegmentProviders = urlSegmentProviders;
        }

        public int CountPublished(string contentTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                return repository.CountPublished();
            }
        }

        public int Count(string contentTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                return repository.Count(contentTypeAlias);
            }
        }

        public int CountChildren(int parentId, string contentTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                return repository.CountChildren(parentId, contentTypeAlias);
            }
        }

        public int CountDescendants(int parentId, string contentTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                return repository.CountDescendants(parentId, contentTypeAlias);
            }
        }

        /// <summary>
        /// Used to bulk update the permissions set for a content item. This will replace all permissions
        /// assigned to an entity with a list of user id & permission pairs.
        /// </summary>
        /// <param name="permissionSet"></param>
        public void ReplaceContentPermissions(EntityPermissionSet permissionSet)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                repository.ReplaceContentPermissions(permissionSet);
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
                var repository = uow.CreateRepository<IContentRepository>();
                repository.AssignEntityPermission(entity, permission, userIds);
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
                var repository = uow.CreateRepository<IContentRepository>();
                return repository.GetPermissionsForEntity(content.Id);
            }
        }

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
        public IContent CreateContent(string name, int parentId, string contentTypeAlias, int userId = 0)
        {
            var contentType = FindContentTypeByAlias(contentTypeAlias);
            var content = new Content(name, parentId, contentType);
            var parent = GetById(content.ParentId);
            content.Path = string.Concat(parent.IfNotNull(x => x.Path, content.ParentId.ToString()), ",", content.Id);


            if (Creating.IsRaisedEventCancelled(new NewEventArgs<IContent>(content, contentTypeAlias, parentId), this))
            {
                content.WasCancelled = true;
                return content;
            }

            content.CreatorId = userId;
            content.WriterId = userId;

            Created.RaiseEvent(new NewEventArgs<IContent>(content, false, contentTypeAlias, parentId), this);

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                repo.AddOrUpdate(new AuditItem(content.Id, $"Content '{name}' was created", AuditType.New, content.CreatorId));
                uow.Complete();
            }

            return content;
        }

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
        /// <param name="parent">Parent <see cref="IContent"/> object for the new Content</param>
        /// <param name="contentTypeAlias">Alias of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional id of the user creating the content</param>
        /// <returns><see cref="IContent"/></returns>
        public IContent CreateContent(string name, IContent parent, string contentTypeAlias, int userId = 0)
        {
            if (parent == null) throw new ArgumentNullException("parent");

            var contentType = FindContentTypeByAlias(contentTypeAlias);
            var content = new Content(name, parent, contentType);
            content.Path = string.Concat(parent.Path, ",", content.Id);

            if (Creating.IsRaisedEventCancelled(new NewEventArgs<IContent>(content, contentTypeAlias, parent), this))
            {
                content.WasCancelled = true;
                return content;
            }

            content.CreatorId = userId;
            content.WriterId = userId;

            Created.RaiseEvent(new NewEventArgs<IContent>(content, false, contentTypeAlias, parent), this);

            Audit(AuditType.New, string.Format("Content '{0}' was created", name), content.CreatorId, content.Id);

            return content;
        }

        /// <summary>
        /// Creates and saves an <see cref="IContent"/> object using the alias of the <see cref="IContentType"/>
        /// that this Content should based on.
        /// </summary>
        /// <remarks>
        /// This method returns an <see cref="IContent"/> object that has been persisted to the database
        /// and therefor has an identity.
        /// </remarks>
        /// <param name="name">Name of the Content object</param>
        /// <param name="parentId">Id of Parent for the new Content</param>
        /// <param name="contentTypeAlias">Alias of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional id of the user creating the content</param>
        /// <returns><see cref="IContent"/></returns>
        public IContent CreateContentWithIdentity(string name, int parentId, string contentTypeAlias, int userId = 0)
        {
            var contentType = FindContentTypeByAlias(contentTypeAlias);
            var content = new Content(name, parentId, contentType);

            //NOTE: I really hate the notion of these Creating/Created events - they are so inconsistent, I've only just found
            // out that in these 'WithIdentity' methods, the Saving/Saved events were not fired, wtf. Anyways, they're added now.
            if (Creating.IsRaisedEventCancelled(new NewEventArgs<IContent>(content, contentTypeAlias, parentId), this))
            {
                content.WasCancelled = true;
                return content;
            }

            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IContent>(content), this))
            {
                content.WasCancelled = true;
                return content;
            }

            content.CreatorId = userId;
            content.WriterId = userId;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                repository.AddOrUpdate(content);
                repository.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                uow.Complete();
            }

            Saved.RaiseEvent(new SaveEventArgs<IContent>(content, false), this);
            Created.RaiseEvent(new NewEventArgs<IContent>(content, false, contentTypeAlias, parentId), this);
            Audit(AuditType.New, $"Content '{name}' was created with Id {content.Id}", content.CreatorId, content.Id);

            return content;
        }

        /// <summary>
        /// Creates and saves an <see cref="IContent"/> object using the alias of the <see cref="IContentType"/>
        /// that this Content should based on.
        /// </summary>
        /// <remarks>
        /// This method returns an <see cref="IContent"/> object that has been persisted to the database
        /// and therefor has an identity.
        /// </remarks>
        /// <param name="name">Name of the Content object</param>
        /// <param name="parent">Parent <see cref="IContent"/> object for the new Content</param>
        /// <param name="contentTypeAlias">Alias of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional id of the user creating the content</param>
        /// <returns><see cref="IContent"/></returns>
        public IContent CreateContentWithIdentity(string name, IContent parent, string contentTypeAlias, int userId = 0)
        {
            if (parent == null) throw new ArgumentNullException("parent");

            var contentType = FindContentTypeByAlias(contentTypeAlias);
            var content = new Content(name, parent, contentType);

            //NOTE: I really hate the notion of these Creating/Created events - they are so inconsistent, I've only just found
            // out that in these 'WithIdentity' methods, the Saving/Saved events were not fired, wtf. Anyways, they're added now.
            if (Creating.IsRaisedEventCancelled(new NewEventArgs<IContent>(content, contentTypeAlias, parent), this))
            {
                content.WasCancelled = true;
                return content;
            }

            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IContent>(content), this))
            {
                content.WasCancelled = true;
                return content;
            }

            content.CreatorId = userId;
            content.WriterId = userId;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                repository.AddOrUpdate(content);
                repository.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                uow.Complete();
            }

            Saved.RaiseEvent(new SaveEventArgs<IContent>(content, false), this);
            Created.RaiseEvent(new NewEventArgs<IContent>(content, false, contentTypeAlias, parent), this);
            Audit(AuditType.New, $"Content '{name}' was created with Id {content.Id}", content.CreatorId, content.Id);

            return content;
        }

        /// <summary>
        /// Gets an <see cref="IContent"/> object by Id
        /// </summary>
        /// <param name="id">Id of the Content to retrieve</param>
        /// <returns><see cref="IContent"/></returns>
        public IContent GetById(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets an <see cref="IContent"/> object by Id
        /// </summary>
        /// <param name="ids">Ids of the Content to retrieve</param>
        /// <returns><see cref="IContent"/></returns>
        public IEnumerable<IContent> GetByIds(IEnumerable<int> ids)
        {
            if (ids.Any() == false) return Enumerable.Empty<IContent>();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                return repository.GetAll(ids.ToArray());
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
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.Key == key);
                var contents = repository.GetByQuery(query);
                return contents.SingleOrDefault();
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
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.ContentTypeId == id);
                var contents = repository.GetByQuery(query);

                return contents;
            }
        }

        internal IEnumerable<IContent> GetPublishedContentOfContentType(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.ContentTypeId == id);
                var contents = repository.GetByPublishedVersion(query);

                return contents;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Level
        /// </summary>
        /// <param name="level">The level to retrieve Content from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetByLevel(int level)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.Level == level && !x.Path.StartsWith(Constants.System.RecycleBinContent.ToInvariantString()));
                var contents = repository.GetByQuery(query);

                return contents;
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
                var repository = uow.CreateRepository<IContentRepository>();
                return repository.GetByVersion(versionId);
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
                var repository = uow.CreateRepository<IContentRepository>();
                var versions = repository.GetAllVersions(id);
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

            var ids = content.Path.Split(',').Where(x => x != Constants.System.Root.ToInvariantString() && x != content.Id.ToString(CultureInfo.InvariantCulture)).Select(int.Parse).ToArray();
            if (ids.Any() == false)
                return new List<IContent>();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                return repository.GetAll(ids);
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
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.ParentId == id);
                var contents = repository.GetByQuery(query).OrderBy(x => x.SortOrder);

                return contents;
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
            return GetPagedChildren(id, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, true, filter);
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
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren,
            string orderBy, Direction orderDirection, bool orderBySystemField, string filter)
        {
            Mandate.ParameterCondition(pageIndex >= 0, "pageIndex");
            Mandate.ParameterCondition(pageSize > 0, "pageSize");
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();

                var query = repository.Query;
                //if the id is System Root, then just get all
                if (id != Constants.System.Root)
                {
                    query.Where(x => x.ParentId == id);
                }
                var contents = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, orderBySystemField, filter);

                return contents;
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
            return GetPagedDescendants(id, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, true, filter);
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
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, string orderBy, Direction orderDirection, bool orderBySystemField, string filter)
        {
            Mandate.ParameterCondition(pageIndex >= 0, "pageIndex");
            Mandate.ParameterCondition(pageSize > 0, "pageSize");
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();

                var query = repository.Query;
                //if the id is System Root, then just get all
                if (id != Constants.System.Root)
                {
                    query.Where(x => x.Path.SqlContains(string.Format(",{0},", id), TextColumnType.NVarchar));
                }
                var contents = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, orderBySystemField, filter);

                return contents;
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
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.ParentId == parentId && x.Name.Contains(name));
                var contents = repository.GetByQuery(query);

                return contents;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Descendants from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetDescendants(int id)
        {
            var content = GetById(id);
            if (content == null)
            {
                return Enumerable.Empty<IContent>();
            }
            return GetDescendants(content);
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
                var repository = uow.CreateRepository<IContentRepository>();
                var pathMatch = content.Path + ",";
                var query = repository.Query.Where(x => x.Path.StartsWith(pathMatch) && x.Id != content.Id);
                var contents = repository.GetByQuery(query);

                return contents;
            }
        }

        /// <summary>
        /// Gets the parent of the current content as an <see cref="IContent"/> item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> to retrieve the parent from</param>
        /// <returns>Parent <see cref="IContent"/> object</returns>
        public IContent GetParent(int id)
        {
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
            return version.FirstOrDefault(x => x.Published == true);
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
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.ParentId == Constants.System.Root);
                var contents = repository.GetByQuery(query);

                return contents;
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
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.Trashed == false);
                return repository.GetByPublishedVersion(query);
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
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.Published == true && x.ExpireDate <= DateTime.Now);
                var contents = repository.GetByQuery(query);

                return contents;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has a release date less than or equal to today.
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentForRelease()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.Published == false && x.ReleaseDate <= DateTime.Now);
                var contents = repository.GetByQuery(query);

                return contents;
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentInRecycleBin()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.Path.Contains(Constants.System.RecycleBinContent.ToInvariantString()));
                var contents = repository.GetByQuery(query);

                return contents;
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

        internal int CountChildren(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.ParentId == id);
                var count = repository.Count(query);
                return count;
            }
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
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.Published == true && x.Id == id && x.Trashed == false);
                int count = repository.Count(query);
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
            //If the passed in content has yet to be saved we "fallback" to checking the Parent
            //because if the Parent is publishable then the current content can be Saved and Published
            if (content.HasIdentity == false)
            {
                IContent parent = GetById(content.ParentId);
                return IsPublishable(parent, true);
            }

            return IsPublishable(content, false);
        }

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
        /// Deletes an <see cref="IContent"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <remarks>Move an item to the Recycle Bin will result in the item being unpublished</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
        Attempt<OperationStatus> IContentServiceOperations.MoveToRecycleBin(IContent content, int userId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (new WriteLock(Locker))
            {
                var originalPath = content.Path;

                if (Trashing.IsRaisedEventCancelled(
                  new MoveEventArgs<IContent>(evtMsgs, new MoveEventInfo<IContent>(content, originalPath, Constants.System.RecycleBinContent)),
                  this))
                {
                    return OperationStatus.Attempt.Cancel(evtMsgs);
                }

                var moveInfo = new List<MoveEventInfo<IContent>>
                {
                    new MoveEventInfo<IContent>(content, originalPath, Constants.System.RecycleBinContent)
                };

                //Make sure that published content is unpublished before being moved to the Recycle Bin
                if (HasPublishedVersion(content.Id))
                {
                    //TODO: this shouldn't be a 'sub operation', and if it needs to be it cannot raise events and cannot be cancelled!
                    UnPublish(content, userId);
                }

                //Unpublish descendents of the content item that is being moved to trash
                var descendants = GetDescendants(content).OrderBy(x => x.Level).ToList();
                foreach (var descendant in descendants)
                {
                    //TODO: this shouldn't be a 'sub operation', and if it needs to be it cannot raise events and cannot be cancelled!
                    UnPublish(descendant, userId);
                }

                content.WriterId = userId;
                content.ChangeTrashedState(true);

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentRepository>();
                    repository.AddOrUpdate(content);

                    //Loop through descendants to update their trash state, but ensuring structure by keeping the ParentId
                    foreach (var descendant in descendants)
                    {
                        moveInfo.Add(new MoveEventInfo<IContent>(descendant, descendant.Path, descendant.ParentId));
                        descendant.WriterId = userId;
                        descendant.ChangeTrashedState(true, descendant.ParentId);
                        repository.AddOrUpdate(descendant);
                    }

                    uow.Complete();
                }

                Trashed.RaiseEvent(new MoveEventArgs<IContent>(false, evtMsgs, moveInfo.ToArray()), this);
                Audit(AuditType.Move, "Move Content to Recycle Bin performed by user", userId, content.Id);

                return OperationStatus.Attempt.Succeed(evtMsgs);
            }
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
        /// Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public Attempt<PublishStatus> PublishWithStatus(IContent content, int userId = 0)
        {
            return ((IContentServiceOperations)this).Publish(content, userId);
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
            var result = PublishWithChildrenDo(content, userId, true);

            //This used to just return false only when the parent content failed, otherwise would always return true so we'll
            // do the same thing for the moment
            if (result.All(x => x.Result.ContentItem.Id != content.Id))
                return false;

            return result.Single(x => x.Result.ContentItem.Id == content.Id).Success;
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
        public Attempt<PublishStatus> SaveAndPublishWithStatus(IContent content, int userId = 0, bool raiseEvents = true)
        {
            return ((IContentServiceOperations)this).SaveAndPublish(content, userId, raiseEvents);
        }

        /// <summary>
        /// Saves a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public void Save(IContent content, int userId = 0, bool raiseEvents = true)
        {
            ((IContentServiceOperations)this).Save(content, userId, raiseEvents);
        }

        /// <summary>
        /// Saves a collection of <see cref="IContent"/> objects.
        /// </summary>
        /// <param name="contents">Collection of <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        Attempt<OperationStatus> IContentServiceOperations.Save(IEnumerable<IContent> contents, int userId, bool raiseEvents)
        {
            var asArray = contents.ToArray();

            var evtMsgs = EventMessagesFactory.Get();

            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(
                    new SaveEventArgs<IContent>(asArray, evtMsgs),
                    this))
                {
                    return OperationStatus.Attempt.Cancel(evtMsgs);
                }
            }
            using (new WriteLock(Locker))
            {
                var containsNew = asArray.Any(x => x.HasIdentity == false);

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentRepository>();
                    if (containsNew)
                    {
                        foreach (var content in asArray)
                        {
                            content.WriterId = userId;

                            //Only change the publish state if the "previous" version was actually published
                            if (content.Published)
                                content.ChangePublishedState(PublishedState.Saved);

                            repository.AddOrUpdate(content);
                            //add or update preview
                            repository.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                        }
                    }
                    else
                    {
                        foreach (var content in asArray)
                        {
                            content.WriterId = userId;
                            repository.AddOrUpdate(content);
                            //add or update preview
                            repository.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                        }
                    }

                    uow.Complete();
                }

                if (raiseEvents)
                    Saved.RaiseEvent(new SaveEventArgs<IContent>(asArray, false, evtMsgs), this);
                Audit(AuditType.Save, "Bulk Save content performed by user", userId == -1 ? 0 : userId, Constants.System.Root);

                return OperationStatus.Attempt.Succeed(evtMsgs);
            }
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

            using (new WriteLock(Locker))
            {
                if (Deleting.IsRaisedEventCancelled(
                  new DeleteEventArgs<IContent>(content, evtMsgs),
                  this))
                {
                    return OperationStatus.Attempt.Cancel(evtMsgs);
                }

                //Make sure that published content is unpublished before being deleted
                if (HasPublishedVersion(content.Id))
                {
                    UnPublish(content, userId);
                }

                //Delete children before deleting the 'possible parent'
                var children = GetChildren(content.Id);
                foreach (var child in children)
                {
                    Delete(child, userId);
                }

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentRepository>();
                    repository.Delete(content);
                    uow.Complete();

                    var args = new DeleteEventArgs<IContent>(content, false, evtMsgs);
                    Deleted.RaiseEvent(args, this);

                    //remove any flagged media files
                    repository.DeleteMediaFiles(args.MediaFilesToDelete);
                }

                Audit(AuditType.Delete, "Delete Content performed by user", userId, content.Id);

                return OperationStatus.Attempt.Succeed(evtMsgs);
            }
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
        /// Saves a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        Attempt<OperationStatus> IContentServiceOperations.Save(IContent content, int userId, bool raiseEvents)
        {
            return Save(content, true, userId, raiseEvents);
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
            ((IContentServiceOperations)this).Save(contents, userId, raiseEvents);
        }

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

            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentRepository>();
                    //NOTE What about content that has the contenttype as part of its composition?
                    var query = repository.Query.Where(x => x.ContentTypeId == contentTypeId);
                    var contents = repository.GetByQuery(query).ToArray();

                    if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IContent>(contents), this))
                        return;

                    foreach (var content in contents.OrderByDescending(x => x.ParentId))
                    {
                        //Look for children of current content and move that to trash before the current content is deleted
                        var c = content;
                        var childQuery = repository.Query.Where(x => x.Path.StartsWith(c.Path));
                        var children = repository.GetByQuery(childQuery);

                        foreach (var child in children)
                        {
                            if (child.ContentType.Id != contentTypeId)
                                MoveToRecycleBin(child, userId);
                        }

                        //Permantly delete the content
                        Delete(content, userId);
                    }
                }

                Audit(AuditType.Delete,
                          string.Format("Delete Content of Type {0} performed by user", contentTypeId),
                          userId, Constants.System.Root);
            }
        }

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
            ((IContentServiceOperations)this).Delete(content, userId);
        }

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
            using (new WriteLock(Locker))
            {
                if (DeletingVersions.IsRaisedEventCancelled(new DeleteRevisionsEventArgs(id, specificVersion: versionId), this))
                    return;

                if (deletePriorVersions)
                {
                    var content = GetByVersion(versionId);
                    DeleteVersions(id, content.UpdateDate, userId);
                }

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentRepository>();
                    repository.DeleteVersion(versionId);
                    uow.Complete();
                }

                DeletedVersions.RaiseEvent(new DeleteRevisionsEventArgs(id, false, specificVersion: versionId), this);

                Audit(AuditType.Delete, "Delete Content by version performed by user", userId, Constants.System.Root);
            }
        }

        /// <summary>
        /// Deletes an <see cref="IContent"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <remarks>Move an item to the Recycle Bin will result in the item being unpublished</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
        public void MoveToRecycleBin(IContent content, int userId = 0)
        {
            ((IContentServiceOperations)this).MoveToRecycleBin(content, userId);
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
            using (new WriteLock(Locker))
            {
                //This ensures that the correct method is called if this method is used to Move to recycle bin.
                if (parentId == Constants.System.RecycleBinContent)
                {
                    MoveToRecycleBin(content, userId);
                    return;
                }

                if (Moving.IsRaisedEventCancelled(
                    new MoveEventArgs<IContent>(
                        new MoveEventInfo<IContent>(content, content.Path, parentId)), this))
                {
                    return;
                }

                //used to track all the moved entities to be given to the event
                var moveInfo = new List<MoveEventInfo<IContent>>();

                //call private method that does the recursive moving
                PerformMove(content, parentId, userId, moveInfo);

                Moved.RaiseEvent(new MoveEventArgs<IContent>(false, moveInfo.ToArray()), this);

                Audit(AuditType.Move, "Move Content performed by user", userId, content.Id);
            }
        }

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IContent"/> that resides in the bin
        /// </summary>
        public void EmptyRecycleBin()
        {
            using (new WriteLock(Locker))
            {
                Dictionary<int, IEnumerable<Property>> entities;
                List<string> files;
                bool success;
                var nodeObjectType = new Guid(Constants.ObjectTypes.Document);

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentRepository>();
                    //Create a dictionary of ids -> dictionary of property aliases + values
                    entities = repository.GetEntitiesInRecycleBin()
                        .ToDictionary(
                            key => key.Id,
                            val => (IEnumerable<Property>)val.Properties);

                    files = ((ContentRepository)repository).GetFilesInRecycleBinForUploadField();

                    if (EmptyingRecycleBin.IsRaisedEventCancelled(new RecycleBinEventArgs(nodeObjectType, entities, files), this))
                        return;

                    success = repository.EmptyRecycleBin();

                    EmptiedRecycleBin.RaiseEvent(new RecycleBinEventArgs(nodeObjectType, entities, files, success), this);

                    if (success)
                        repository.DeleteMediaFiles(files);
                }
            }
            Audit(AuditType.Delete, "Empty Content Recycle Bin performed by user", 0, Constants.System.RecycleBinContent);
        }

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
            //TODO: This all needs to be managed correctly so that the logic is submitted in one
            // transaction, the CRUD needs to be moved to the repo

            using (new WriteLock(Locker))
            {
                var copy = content.DeepCloneWithResetIdentities();
                copy.ParentId = parentId;

                // A copy should never be set to published automatically even if the original was.
                copy.ChangePublishedState(PublishedState.Unpublished);

                if (Copying.IsRaisedEventCancelled(new CopyEventArgs<IContent>(content, copy, parentId), this))
                    return null;

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentRepository>();
                    // Update the create author and last edit author
                    copy.CreatorId = userId;
                    copy.WriterId = userId;

                    repository.AddOrUpdate(copy);
                    //add or update a preview
                    repository.AddOrUpdatePreviewXml(copy, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                    uow.Flush(); // ensure copy has an ID


                    //Special case for the associated tags
                    //TODO: Move this to the repository layer in a single transaction!
                    //don't copy tags data in tags table if the item is in the recycle bin
                    if (parentId != Constants.System.RecycleBinContent)
                    {

                        var tags = uow.Database.Fetch<TagRelationshipDto>("WHERE nodeId = @Id", new { Id = content.Id });
                        foreach (var tag in tags)
                        {
                            uow.Database.Insert(new TagRelationshipDto { NodeId = copy.Id, TagId = tag.TagId, PropertyTypeId = tag.PropertyTypeId });
                        }
                    }

                    uow.Complete();
                }

                if (recursive)
                {
                    //Look for children and copy those as well
                    var children = GetChildren(content.Id);
                    foreach (var child in children)
                    {
                        //TODO: This shouldn't recurse back to this method, it should be done in a private method
                        // that doesn't have a nested lock and so we can perform the entire operation in one commit.
                        Copy(child, copy.Id, relateToOriginal, true, userId);
                    }
                }

                Copied.RaiseEvent(new CopyEventArgs<IContent>(content, copy, false, parentId, relateToOriginal), this);

                Audit(AuditType.Copy, "Copy Content performed by user", content.WriterId, content.Id);
                return copy;
            }
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

            content.WriterId = userId;
            content.CreatorId = userId;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                content.ChangePublishedState(PublishedState.Unpublished);
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
            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IContent>(items), this))
                    return false;
            }

            var shouldBePublished = new List<IContent>();
            var shouldBeSaved = new List<IContent>();

            var asArray = items.ToArray();
            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentRepository>();
                    int i = 0;
                    foreach (var content in asArray)
                    {
                        //If the current sort order equals that of the content
                        //we don't need to update it, so just increment the sort order
                        //and continue.
                        if (content.SortOrder == i)
                        {
                            i++;
                            continue;
                        }

                        content.SortOrder = i;
                        content.WriterId = userId;
                        i++;

                        if (content.Published)
                        {
                            //TODO: This should not be an inner operation, but if we do this, it cannot raise events and cannot be cancellable!
                            var published = _publishingStrategy.Publish(content, userId);
                            shouldBePublished.Add(content);
                        }
                        else
                            shouldBeSaved.Add(content);

                        repository.AddOrUpdate(content);
                        //add or update a preview
                        repository.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                    }

                    foreach (var content in shouldBePublished)
                    {
                        //Create and Save ContentXml DTO
                        repository.AddOrUpdateContentXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                    }

                    uow.Complete();
                }
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IContent>(asArray, false), this);

            if (shouldBePublished.Any())
            {
                //TODO: This should not be an inner operation, but if we do this, it cannot raise events and cannot be cancellable!
                _publishingStrategy.PublishingFinalized(shouldBePublished, false);
            }


            Audit(AuditType.Sort, "Sorting content performed by user", userId, 0);

            return true;
        }

        /// <summary>
        /// Returns the persisted content's XML structure
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public XElement GetContentXml(int contentId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                return repository.GetContentXml(contentId);
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
                var repository = uow.CreateRepository<IContentRepository>();
                return repository.GetContentPreviewXml(contentId, version);
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
                var repository = uow.CreateRepository<IContentRepository>();
                repository.RebuildXmlStructures(
                    content => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, content),
                    contentTypeIds: contentTypeIds.Length == 0 ? null : contentTypeIds);
                uow.Complete();
            }

            Audit(AuditType.Publish, "ContentService.RebuildXmlStructures completed, the xml has been regenerated in the database", 0, Constants.System.Root);

        }

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
                var repository = uow.CreateRepository<IContentRepository>();
                var query = repository.Query.Where(x => x.Id != content.Id && x.Path.StartsWith(content.Path) && x.Trashed == false);
                var contents = repository.GetByPublishedVersion(query);

                return contents;
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

        //TODO: All of this needs to be moved to the repository
        private void PerformMove(IContent content, int parentId, int userId, ICollection<MoveEventInfo<IContent>> moveInfo)
        {
            //add a tracking item to use in the Moved event
            moveInfo.Add(new MoveEventInfo<IContent>(content, content.Path, parentId));

            content.WriterId = userId;
            if (parentId == Constants.System.Root)
            {
                content.Path = string.Concat(Constants.System.Root, ",", content.Id);
                content.Level = 1;
            }
            else
            {
                var parent = GetById(parentId);
                content.Path = string.Concat(parent.Path, ",", content.Id);
                content.Level = parent.Level + 1;
            }

            //If Content is being moved away from Recycle Bin, its state should be un-trashed
            if (content.Trashed && parentId != Constants.System.RecycleBinContent)
            {
                content.ChangeTrashedState(false, parentId);
            }
            else
            {
                content.ParentId = parentId;
            }

            //If Content is published, it should be (re)published from its new location
            if (content.Published)
            {
                //If Content is Publishable its saved and published
                //otherwise we save the content without changing the publish state, and generate new xml because the Path, Level and Parent has changed.
                if (IsPublishable(content))
                {
                    //TODO: This is raising events, probably not desirable as this costs performance for event listeners like Examine
                    SaveAndPublish(content, userId);
                }
                else
                {
                    //TODO: This is raising events, probably not desirable as this costs performance for event listeners like Examine
                    Save(content, false, userId);

                    //TODO: This shouldn't be here! This needs to be part of the repository logic but in order to fix this we need to
                    // change how this method calls "Save" as it needs to save using an internal method
                    using (var uow = UowProvider.CreateUnitOfWork())
                    {
                        var xml = _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, content);

                        var poco = new ContentXmlDto { NodeId = content.Id, Xml = xml.ToDataString() };
                        var exists =
                            uow.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = content.Id }) !=
                            null;
                        int result = exists
                                         ? uow.Database.Update(poco)
                                         : Convert.ToInt32(uow.Database.Insert(poco));
                    }
                }
            }
            else
            {
                //TODO: This is raising events, probably not desirable as this costs performance for event listeners like Examine
                Save(content, userId);
            }

            //Ensure that Path and Level is updated on children
            var children = GetChildren(content.Id).ToArray();
            if (children.Any())
            {
                foreach (var child in children)
                {
                    PerformMove(child, content.Id, userId, moveInfo);
                }
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
        private IEnumerable<Attempt<PublishStatus>> PublishWithChildrenDo(
            IContent content, int userId = 0, bool includeUnpublished = false)
        {
            if (content == null) throw new ArgumentNullException("content");

            var evtMsgs = EventMessagesFactory.Get();

            using (new WriteLock(Locker))
            {
                var result = new List<Attempt<PublishStatus>>();

                //Check if parent is published (although not if its a root node) - if parent isn't published this Content cannot be published
                if (content.ParentId != Constants.System.Root && content.ParentId != Constants.System.RecycleBinContent && IsPublishable(content) == false)
                {
                    Logger.Info<ContentService>(
                        string.Format(
                            "Content '{0}' with Id '{1}' could not be published because its parent or one of its ancestors is not published.",
                            content.Name, content.Id));
                    result.Add(Attempt.Fail(new PublishStatus(PublishStatusType.FailedPathNotPublished, evtMsgs, content)));
                    return result;
                }

                //Content contains invalid property values and can therefore not be published - fire event?
                if (!content.IsValid())
                {
                    Logger.Info<ContentService>(
                        string.Format("Content '{0}' with Id '{1}' could not be published because of invalid properties.",
                                      content.Name, content.Id));
                    result.Add(
                        Attempt.Fail(
                            new PublishStatus(PublishStatusType.FailedContentInvalid, evtMsgs, content)
                            {
                                InvalidProperties = ((ContentBase)content).LastInvalidProperties
                            }));
                    return result;
                }

                //Consider creating a Path query instead of recursive method:
                //var query = repository.Query.Where(x => x.Path.StartsWith(content.Path));

                var updated = new List<IContent>();
                var list = new List<IContent>();
                list.Add(content); //include parent item
                list.AddRange(GetDescendants(content));

                var internalStrategy = (PublishingStrategy)_publishingStrategy;

                //Publish and then update the database with new status
                var publishedOutcome = internalStrategy.PublishWithChildrenInternal(list, userId, includeUnpublished).ToArray();
                var published = publishedOutcome
                    .Where(x => x.Success || x.Result.StatusType == PublishStatusType.SuccessAlreadyPublished)
                    // ensure proper order (for events) - cannot publish a child before its parent!
                    .OrderBy(x => x.Result.ContentItem.Level)
                    .ThenBy(x => x.Result.ContentItem.SortOrder);

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentRepository>();
                    //NOTE The Publish with subpages-dialog was used more as a republish-type-thing, so we'll have to include PublishStatusType.SuccessAlreadyPublished
                    //in the updated-list, so the Published event is triggered with the expected set of pages and the xml is updated.
                    foreach (var item in published)
                    {
                        item.Result.ContentItem.WriterId = userId;
                        repository.AddOrUpdate(item.Result.ContentItem);
                        //add or update a preview
                        repository.AddOrUpdatePreviewXml(item.Result.ContentItem, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                        //add or update the published xml
                        repository.AddOrUpdateContentXml(item.Result.ContentItem, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                        updated.Add(item.Result.ContentItem);
                    }

                    uow.Complete();

                }
                //Save xml to db and call following method to fire event:
                _publishingStrategy.PublishingFinalized(updated, false);

                Audit(AuditType.Publish, "Publish with Children performed by user", userId, content.Id);


                return publishedOutcome;
            }
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
            var newest = GetById(content.Id); // ensure we have the newest version
            if (content.Version != newest.Version) // but use the original object if it's already the newest version
                content = newest;

            var evtMsgs = EventMessagesFactory.Get();

            var published = content.Published ? content : GetPublishedVersion(content.Id); // get the published version
            if (published == null)
            {
                return Attempt.Succeed(new UnPublishStatus(UnPublishedStatusType.SuccessAlreadyUnPublished, evtMsgs, content)); // already unpublished
            }

            var unpublished = _publishingStrategy.UnPublish(content, userId);
            if (unpublished == false) return Attempt.Fail(new UnPublishStatus(UnPublishedStatusType.FailedCancelledByEvent, evtMsgs, content));

            content.WriterId = userId;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentRepository>();
                repository.AddOrUpdate(content);
                // is published is not newest, reset the published flag on published version
                if (published.Version != content.Version)
                    repository.ClearPublishedFlag(published);
                repository.DeleteContentXml(content);
                uow.Complete();
            }
            //Delete xml from db? and call following method to fire event through PublishingStrategy to update cache
            if (omitCacheRefresh == false)
                _publishingStrategy.UnPublishingFinalized(content);

            Audit(AuditType.UnPublish, "UnPublish performed by user", userId, content.Id);

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

            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(
                    new SaveEventArgs<IContent>(content, evtMsgs), this))
                {
                    return Attempt.Fail(new PublishStatus(PublishStatusType.FailedCancelledByEvent, evtMsgs, content));
                }
            }

            using (new WriteLock(Locker))
            {
                //Has this content item previously been published? If so, we don't need to refresh the children
                var previouslyPublished = content.HasIdentity && HasPublishedVersion(content.Id); //content might not have an id
                var publishStatus = new PublishStatus(PublishStatusType.Success, evtMsgs, content); //initially set to success

                //Check if parent is published (although not if its a root node) - if parent isn't published this Content cannot be published
                publishStatus.StatusType = CheckAndLogIsPublishable(content);
                //if it is not successful, then check if the props are valid
                if (publishStatus.StatusType.IsSuccess())
                {
                    //Content contains invalid property values and can therefore not be published - fire event?
                    publishStatus.StatusType = CheckAndLogIsValid(content);
                    //set the invalid properties (if there are any)
                    publishStatus.InvalidProperties = ((ContentBase)content).LastInvalidProperties;
                }
                //if we're still successful, then publish using the strategy
                if (publishStatus.StatusType == PublishStatusType.Success)
                {
                    var internalStrategy = (PublishingStrategy)_publishingStrategy;
                    //Publish and then update the database with new status
                    var publishResult = internalStrategy.PublishInternal(content, userId);
                    //set the status type to the publish result
                    publishStatus.StatusType = publishResult.Result.StatusType;
                }

                //we are successfully published if our publishStatus is still Successful
                bool published = publishStatus.StatusType == PublishStatusType.Success;

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentRepository>();
                    if (published == false)
                    {
                        content.ChangePublishedState(PublishedState.Saved);
                    }
                    //Since this is the Save and Publish method, the content should be saved even though the publish fails or isn't allowed
                    if (content.HasIdentity == false)
                    {
                        content.CreatorId = userId;
                    }
                    content.WriterId = userId;

                    repository.AddOrUpdate(content);

                    //Generate a new preview
                    repository.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));

                    if (published)
                    {
                        //Content Xml
                        repository.AddOrUpdateContentXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));
                    }

                    uow.Complete();
                }

                if (raiseEvents)
                    Saved.RaiseEvent(new SaveEventArgs<IContent>(content, false, evtMsgs), this);

                //Save xml to db and call following method to fire event through PublishingStrategy to update cache
                if (published)
                {
                    _publishingStrategy.PublishingFinalized(content);
                }

                //We need to check if children and their publish state to ensure that we 'republish' content that was previously published
                if (published && previouslyPublished == false && HasChildren(content.Id))
                {
                    var descendants = GetPublishedDescendants(content);

                    _publishingStrategy.PublishingFinalized(descendants, false);
                }

                Audit(AuditType.Publish, "Save and Publish performed by user", userId, content.Id);

                return Attempt.If(publishStatus.StatusType == PublishStatusType.Success, publishStatus);
            }
        }

        /// <summary>
        /// Saves a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save</param>
        /// <param name="changeState">Boolean indicating whether or not to change the Published state upon saving</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        private Attempt<OperationStatus> Save(IContent content, bool changeState, int userId = 0, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(
                  new SaveEventArgs<IContent>(content, evtMsgs),
                  this))
                {
                    return OperationStatus.Attempt.Cancel(evtMsgs);
                }
            }

            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentRepository>();
                    if (content.HasIdentity == false)
                    {
                        content.CreatorId = userId;
                    }
                    content.WriterId = userId;

                    //Only change the publish state if the "previous" version was actually published or marked as unpublished
                    if (changeState && (content.Published || ((Content)content).PublishedState == PublishedState.Unpublished))
                        content.ChangePublishedState(PublishedState.Saved);

                    repository.AddOrUpdate(content);

                    //Generate a new preview
                    repository.AddOrUpdatePreviewXml(content, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));

                    uow.Complete();
                }

                if (raiseEvents)
                    Saved.RaiseEvent(new SaveEventArgs<IContent>(content, false, evtMsgs), this);

                Audit(AuditType.Save, "Save Content performed by user", userId, content.Id);

                return OperationStatus.Attempt.Succeed(evtMsgs);
            }
        }

        /// <summary>
        /// Checks if the passed in <see cref="IContent"/> can be published based on the anscestors publish state.
        /// </summary>
        /// <remarks>
        /// Check current is only used when falling back to checking the Parent of non-saved content, as
        /// non-saved content doesn't have a valid path yet.
        /// </remarks>
        /// <param name="content"><see cref="IContent"/> to check if anscestors are published</param>
        /// <param name="checkCurrent">Boolean indicating whether the passed in content should also be checked for published versions</param>
        /// <returns>True if the Content can be published, otherwise False</returns>
        private bool IsPublishable(IContent content, bool checkCurrent)
        {
            var ids = content.Path.Split(',').Select(int.Parse).ToList();
            foreach (var id in ids)
            {
                //If Id equals that of the recycle bin we return false because nothing in the bin can be published
                if (id == Constants.System.RecycleBinContent)
                    return false;

                //We don't check the System Root, so just continue
                if (id == Constants.System.Root) continue;

                //If the current id equals that of the passed in content and if current shouldn't be checked we skip it.
                if (checkCurrent == false && id == content.Id) continue;

                //Check if the content for the current id is published - escape the loop if we encounter content that isn't published
                var hasPublishedVersion = HasPublishedVersion(id);
                if (hasPublishedVersion == false)
                    return false;
            }

            return true;
        }

        private PublishStatusType CheckAndLogIsPublishable(IContent content)
        {
            //Check if parent is published (although not if its a root node) - if parent isn't published this Content cannot be published
            if (content.ParentId != Constants.System.Root && content.ParentId != Constants.System.RecycleBinContent && IsPublishable(content) == false)
            {
                Logger.Info<ContentService>(
                    string.Format(
                        "Content '{0}' with Id '{1}' could not be published because its parent is not published.",
                        content.Name, content.Id));
                return PublishStatusType.FailedPathNotPublished;
            }
            else if (content.ExpireDate.HasValue && content.ExpireDate.Value > DateTime.MinValue && DateTime.Now > content.ExpireDate.Value)
            {
                Logger.Info<ContentService>(
                    string.Format(
                        "Content '{0}' with Id '{1}' has expired and could not be published.",
                        content.Name, content.Id));
                return PublishStatusType.FailedHasExpired;
            }
            else if (content.ReleaseDate.HasValue && content.ReleaseDate.Value > DateTime.MinValue && content.ReleaseDate.Value > DateTime.Now)
            {
                Logger.Info<ContentService>(
                    string.Format(
                        "Content '{0}' with Id '{1}' is awaiting release and could not be published.",
                        content.Name, content.Id));
                return PublishStatusType.FailedAwaitingRelease;
            }

            return PublishStatusType.Success;
        }

        private PublishStatusType CheckAndLogIsValid(IContent content)
        {
            //Content contains invalid property values and can therefore not be published - fire event?
            if (content.IsValid() == false)
            {
                Logger.Info<ContentService>(
                    string.Format(
                        "Content '{0}' with Id '{1}' could not be published because of invalid properties.",
                        content.Name, content.Id));
                return PublishStatusType.FailedContentInvalid;
            }

            return PublishStatusType.Success;
        }

        private IContentType FindContentTypeByAlias(string contentTypeAlias)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
                var query = repository.Query.Where(x => x.Alias == contentTypeAlias);
                var types = repository.GetByQuery(query);

                if (types.Any() == false)
                    throw new Exception(
                        string.Format("No ContentType matching the passed in Alias: '{0}' was found",
                                      contentTypeAlias));

                var contentType = types.First();

                if (contentType == null)
                    throw new Exception(string.Format("ContentType matching the passed in Alias: '{0}' was null",
                                                      contentTypeAlias));

                return contentType;
            }
        }

        #endregion

        #region Proxy Event Handlers
        /// <summary>
        /// Occurs before publish.
        /// </summary>
        /// <remarks>Proxy to the real event on the <see cref="PublishingStrategy"/></remarks>
        public static event TypedEventHandler<IPublishingStrategy, PublishEventArgs<IContent>> Publishing
        {
            add { PublishingStrategy.Publishing += value; }
            remove { PublishingStrategy.Publishing -= value; }
        }

        /// <summary>
        /// Occurs after publish.
        /// </summary>
        /// <remarks>Proxy to the real event on the <see cref="PublishingStrategy"/></remarks>
        public static event TypedEventHandler<IPublishingStrategy, PublishEventArgs<IContent>> Published
        {
            add { PublishingStrategy.Published += value; }
            remove { PublishingStrategy.Published -= value; }
        }
        /// <summary>
        /// Occurs before unpublish.
        /// </summary>
        /// <remarks>Proxy to the real event on the <see cref="PublishingStrategy"/></remarks>
        public static event TypedEventHandler<IPublishingStrategy, PublishEventArgs<IContent>> UnPublishing
        {
            add { PublishingStrategy.UnPublishing += value; }
            remove { PublishingStrategy.UnPublishing -= value; }
        }

        /// <summary>
        /// Occurs after unpublish.
        /// </summary>
        /// <remarks>Proxy to the real event on the <see cref="PublishingStrategy"/></remarks>
        public static event TypedEventHandler<IPublishingStrategy, PublishEventArgs<IContent>> UnPublished
        {
            add { PublishingStrategy.UnPublished += value; }
            remove { PublishingStrategy.UnPublished -= value; }
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
        #endregion
    }
}