using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Media Service, which is an easy access to operations involving <see cref="IMedia"/>
    /// </summary>
    public class MediaService : ScopeRepositoryService, IMediaService, IMediaServiceOperations
    {

        //Support recursive locks because some of the methods that require locking call other methods that require locking.
        //for example, the Move method needs to be locked but this calls the Save method which also needs to be locked.
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private readonly EntityXmlSerializer _entitySerializer = new EntityXmlSerializer();
        private readonly IDataTypeService _dataTypeService;
        private readonly IUserService _userService;
        private readonly MediaFileSystem _mediaFileSystem = FileSystemProviderManager.Current.MediaFileSystem;

        public MediaService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory, IDataTypeService dataTypeService, IUserService userService)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {
            if (dataTypeService == null) throw new ArgumentNullException("dataTypeService");
            if (userService == null) throw new ArgumentNullException("userService");
            _dataTypeService = dataTypeService;
            _userService = userService;
        }

        /// <summary>
        /// Creates an <see cref="IMedia"/> object using the alias of the <see cref="IMediaType"/>
        /// that this Media should based on.
        /// </summary>
        /// <remarks>
        /// Note that using this method will simply return a new IMedia without any identity
        /// as it has not yet been persisted. It is intended as a shortcut to creating new media objects
        /// that does not invoke a save operation against the database.
        /// </remarks>
        /// <param name="name">Name of the Media object</param>
        /// <param name="parentId">Id of Parent for the new Media item</param>
        /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user creating the media item</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia CreateMedia(string name, Guid parentId, string mediaTypeAlias, int userId = 0)
        {
            var parent = GetById(parentId);
            return CreateMedia(name, parent, mediaTypeAlias, userId);
        }

        /// <summary>
        /// Creates an <see cref="IMedia"/> object using the alias of the <see cref="IMediaType"/>
        /// that this Media should based on.
        /// </summary>
        /// <remarks>
        /// Note that using this method will simply return a new IMedia without any identity
        /// as it has not yet been persisted. It is intended as a shortcut to creating new media objects
        /// that does not invoke a save operation against the database.
        /// </remarks>
        /// <param name="name">Name of the Media object</param>
        /// <param name="parentId">Id of Parent for the new Media item</param>
        /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user creating the media item</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia CreateMedia(string name, int parentId, string mediaTypeAlias, int userId = 0)
        {
            var mediaType = FindMediaTypeByAlias(mediaTypeAlias);
            var media = new Models.Media(name, parentId, mediaType);
            var parent = GetById(media.ParentId);
            media.Path = string.Concat(parent.IfNotNull(x => x.Path, media.ParentId.ToString()), ",", media.Id);

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var newEventArgs = new NewEventArgs<IMedia>(media, mediaTypeAlias, parentId);
                if (uow.Events.DispatchCancelable(Creating, this, newEventArgs))
                {
                    uow.Commit();
                    media.WasCancelled = true;
                    return media;
                }

                media.CreatorId = userId;
                newEventArgs.CanCancel = false;
                uow.Events.Dispatch(Created, this, newEventArgs);
                Audit(uow, AuditType.New, string.Format("Media '{0}' was created", name), media.CreatorId, media.Id);
                uow.Commit();

                return media;
            }
        }

        /// <summary>
        /// Creates an <see cref="IMedia"/> object using the alias of the <see cref="IMediaType"/>
        /// that this Media should based on.
        /// </summary>
        /// <remarks>
        /// Note that using this method will simply return a new IMedia without any identity
        /// as it has not yet been persisted. It is intended as a shortcut to creating new media objects
        /// that does not invoke a save operation against the database.
        /// </remarks>
        /// <param name="name">Name of the Media object</param>
        /// <param name="parent">Parent <see cref="IMedia"/> for the new Media item</param>
        /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user creating the media item</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia CreateMedia(string name, IMedia parent, string mediaTypeAlias, int userId = 0)
        {
            if (parent == null) throw new ArgumentNullException("parent");

            var mediaType = FindMediaTypeByAlias(mediaTypeAlias);
            var media = new Models.Media(name, parent, mediaType);
            media.Path = string.Concat(parent.Path, ",", media.Id);

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var newEventArgs = new NewEventArgs<IMedia>(media, mediaTypeAlias, parent);
                if (uow.Events.DispatchCancelable(Creating, this, newEventArgs))
                {
                    uow.Commit();
                    media.WasCancelled = true;
                    return media;
                }

                media.CreatorId = userId;
                newEventArgs.CanCancel = false;
                uow.Events.Dispatch(Created, this, newEventArgs);

                Audit(uow, AuditType.New, string.Format("Media '{0}' was created", name), media.CreatorId, media.Id);
                uow.Commit();

                return media;
            }
        }

        /// <summary>
        /// Creates an <see cref="IMedia"/> object using the alias of the <see cref="IMediaType"/>
        /// that this Media should based on.
        /// </summary>
        /// <remarks>
        /// This method returns an <see cref="IMedia"/> object that has been persisted to the database
        /// and therefor has an identity.
        /// </remarks>
        /// <param name="name">Name of the Media object</param>
        /// <param name="parentId">Id of Parent for the new Media item</param>
        /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user creating the media item</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia CreateMediaWithIdentity(string name, int parentId, string mediaTypeAlias, int userId = 0)
        {
            var mediaType = FindMediaTypeByAlias(mediaTypeAlias);
            var media = new Models.Media(name, parentId, mediaType);

            using (var uow = UowProvider.GetUnitOfWork())
            {
                //NOTE: I really hate the notion of these Creating/Created events - they are so inconsistent, I've only just found
                // out that in these 'WithIdentity' methods, the Saving/Saved events were not fired, wtf. Anyways, they're added now.
                var newEventArgs = new NewEventArgs<IMedia>(media, mediaTypeAlias, parentId);
                if (uow.Events.DispatchCancelable(Creating, this, newEventArgs))
                {
                    uow.Commit();
                    media.WasCancelled = true;
                    return media;
                }

                var saveEventArgs = new SaveEventArgs<IMedia>(media);
                if (uow.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    uow.Commit();
                    media.WasCancelled = true;
                    return media;
                }

                var repository = RepositoryFactory.CreateMediaRepository(uow);
                media.CreatorId = userId;
                repository.AddOrUpdate(media);
                repository.AddOrUpdateContentXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));

                if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                {
                    repository.AddOrUpdatePreviewXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                }

                saveEventArgs.CanCancel = false;
                uow.Events.Dispatch(Saved, this, saveEventArgs);
                newEventArgs.CanCancel = false;
                uow.Events.Dispatch(Created, this, newEventArgs);

                Audit(uow, AuditType.New, string.Format("Media '{0}' was created with Id {1}", name, media.Id), media.CreatorId, media.Id);
                uow.Commit();
            }

            return media;
        }

        /// <summary>
        /// Creates an <see cref="IMedia"/> object using the alias of the <see cref="IMediaType"/>
        /// that this Media should based on.
        /// </summary>
        /// <remarks>
        /// This method returns an <see cref="IMedia"/> object that has been persisted to the database
        /// and therefor has an identity.
        /// </remarks>
        /// <param name="name">Name of the Media object</param>
        /// <param name="parent">Parent <see cref="IMedia"/> for the new Media item</param>
        /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user creating the media item</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia CreateMediaWithIdentity(string name, IMedia parent, string mediaTypeAlias, int userId = 0)
        {
            if (parent == null) throw new ArgumentNullException("parent");

            var mediaType = FindMediaTypeByAlias(mediaTypeAlias);
            var media = new Models.Media(name, parent, mediaType);

            using (var uow = UowProvider.GetUnitOfWork())
            {
                //NOTE: I really hate the notion of these Creating/Created events - they are so inconsistent, I've only just found
                // out that in these 'WithIdentity' methods, the Saving/Saved events were not fired, wtf. Anyways, they're added now.
                var newEventArgs = new NewEventArgs<IMedia>(media, mediaTypeAlias, parent);
                if (uow.Events.DispatchCancelable(Creating, this, newEventArgs))
                {
                    uow.Commit();
                    media.WasCancelled = true;
                    return media;
                }

                var saveEventArgs = new SaveEventArgs<IMedia>(media);
                if (uow.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    uow.Commit();
                    media.WasCancelled = true;
                    return media;
                }

                var repository = RepositoryFactory.CreateMediaRepository(uow);
                media.CreatorId = userId;
                repository.AddOrUpdate(media);
                repository.AddOrUpdateContentXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                // generate preview for blame history?
                if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                {
                    repository.AddOrUpdatePreviewXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                }

                saveEventArgs.CanCancel = false;
                uow.Events.Dispatch(Saved, this, saveEventArgs);
                newEventArgs.CanCancel = false;
                uow.Events.Dispatch(Created, this, newEventArgs);

                Audit(uow, AuditType.New, string.Format("Media '{0}' was created with Id {1}", name, media.Id), media.CreatorId, media.Id);
                uow.Commit();
            }

            return media;
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by Id
        /// </summary>
        /// <param name="id">Id of the Content to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetById(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                return repository.Get(id);
            }
        }

        public int CountNotTrashed(string contentTypeAlias = null)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            using (var mediaTypeRepository = RepositoryFactory.CreateMediaTypeRepository(uow))
            {
                var mediaTypeId = 0;
                if (contentTypeAlias.IsNullOrWhiteSpace() == false)
                {
                    var mediaType = mediaTypeRepository.Get(contentTypeAlias);
                    if (mediaType == null) return 0;
                    mediaTypeId = mediaType.Id;
                }

                var query = Query<IMedia>.Builder.Where(media => media.Trashed == false);
                if (mediaTypeId > 0)
                {
                    query.Where(media => media.ContentTypeId == mediaTypeId);
                }
                return repository.Count(query);
            }
        }

        public int Count(string contentTypeAlias = null)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                return repository.Count(contentTypeAlias);
            }
        }

        public int CountChildren(int parentId, string contentTypeAlias = null)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                return repository.CountChildren(parentId, contentTypeAlias);
            }
        }

        public int CountDescendants(int parentId, string contentTypeAlias = null)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                return repository.CountDescendants(parentId, contentTypeAlias);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> objects by Ids
        /// </summary>
        /// <param name="ids">Ids of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IEnumerable<IMedia> GetByIds(IEnumerable<int> ids)
        {
            if (ids.Any() == false) return Enumerable.Empty<IMedia>();

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                return repository.GetAll(ids.ToArray());
            }
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> objects by Ids
        /// </summary>
        /// <param name="ids">Ids of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IEnumerable<IMedia> GetByIds(IEnumerable<Guid> ids)
        {
            if (ids.Any() == false) return Enumerable.Empty<IMedia>();

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                return repository.GetAll(ids.ToArray());
            }
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by its 'UniqueId'
        /// </summary>
        /// <param name="key">Guid key of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetById(Guid key)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                return repository.Get(key);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Level
        /// </summary>
        /// <param name="level">The level to retrieve Media from</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetByLevel(int level)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                var query = Query<IMedia>.Builder.Where(x => x.Level == level && x.Path.StartsWith(Constants.System.RecycleBinMediaPathPrefix) == false);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets a specific version of an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="versionId">Id of the version to retrieve</param>
        /// <returns>An <see cref="IMedia"/> item</returns>
        public IMedia GetByVersion(Guid versionId)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                return repository.GetByVersion(versionId);
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IMedia"/> objects versions by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetVersions(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                return repository.GetAllVersions(id);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which are ancestors of the current media.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> to retrieve ancestors for</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetAncestors(int id)
        {
            var media = GetById(id);
            return GetAncestors(media);
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which are ancestors of the current media.
        /// </summary>
        /// <param name="media"><see cref="IMedia"/> to retrieve ancestors for</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetAncestors(IMedia media)
        {
            var ids = media.Path.Split(',').Where(x => x != Constants.System.RootString && x != media.Id.ToString(CultureInfo.InvariantCulture)).Select(int.Parse).ToArray();
            if (ids.Any() == false)
                return new List<IMedia>();

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                return repository.GetAll(ids);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetChildren(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                var query = Query<IMedia>.Builder.Where(x => x.ParentId == id);
                return repository.GetByQuery(query);
            }
        }
        
        [Obsolete("Use the overload with 'long' parameter types instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<IMedia> GetPagedChildren(int id, int pageIndex, int pageSize, out int totalChildren,
            string orderBy, Direction orderDirection, string filter = "")
        {
            long total;
            var result = GetPagedChildren(id, Convert.ToInt64(pageIndex), pageSize, out total, orderBy, orderDirection, true, filter);
            totalChildren = Convert.ToInt32(total);
            return result;
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <param name="pageIndex">Page index (zero based)</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalChildren">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IMedia> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren,
            string orderBy, Direction orderDirection, string filter = "")
        {
            return GetPagedChildren(id, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, true, filter);
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Parent Id
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
        public IEnumerable<IMedia> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren,
            string orderBy, Direction orderDirection, bool orderBySystemField, string filter)
        {
            return GetPagedChildren(id, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, true, filter, null);
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalChildren">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
        /// <param name="filter">Search text filter</param>
        /// <param name="contentTypeFilter">A list of content type Ids to filter the list by</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IMedia> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren,
            string orderBy, Direction orderDirection, bool orderBySystemField, string filter, int[] contentTypeFilter)
        {
            Mandate.ParameterCondition(pageIndex >= 0, "pageIndex");
            Mandate.ParameterCondition(pageSize > 0, "pageSize");

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);

                var query = Query<IMedia>.Builder;
                // always check for a parent - else it will also get decendants (and then you should use the GetPagedDescendants method)
                query.Where(x => x.ParentId == id);

                if (contentTypeFilter != null && contentTypeFilter.Length > 0)
                {
                    query.Where(x => contentTypeFilter.Contains(x.ContentTypeId));
                }

                IQuery<IMedia> filterQuery = null;
                if (filter.IsNullOrWhiteSpace() == false)
                {
                    filterQuery = Query<IMedia>.Builder.Where(x => x.Name.Contains(filter));
                }
                return repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, orderBySystemField, filterQuery);
            }
        }

        [Obsolete("Use the overload with 'long' parameter types instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<IMedia> GetPagedDescendants(int id, int pageIndex, int pageSize, out int totalChildren, string orderBy = "path", Direction orderDirection = Direction.Ascending, string filter = "")
        {
            long total;
            var result = GetPagedDescendants(id, Convert.ToInt64(pageIndex), pageSize, out total, orderBy, orderDirection, true, filter);
            totalChildren = Convert.ToInt32(total);
            return result;
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
        public IEnumerable<IMedia> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, string orderBy = "path", Direction orderDirection = Direction.Ascending, string filter = "")
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
        public IEnumerable<IMedia> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, string orderBy, Direction orderDirection, bool orderBySystemField, string filter)
        {
            Mandate.ParameterCondition(pageIndex >= 0, "pageIndex");
            Mandate.ParameterCondition(pageSize > 0, "pageSize");
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                var query = Query<IMedia>.Builder;
                //if the id is System Root, then just get all
                if (id != Constants.System.Root)
                {
                    var entityRepository = RepositoryFactory.CreateEntityRepository(uow);
                    var mediaPath = entityRepository.GetAllPaths(Constants.ObjectTypes.MediaGuid, id).ToArray();
                    if (mediaPath.Length == 0)
                    {
                        totalChildren = 0;
                        return Enumerable.Empty<IMedia>();
                    }
                    query.Where(x => x.Path.SqlStartsWith(string.Format("{0},", mediaPath[0].Path), TextColumnType.NVarchar));
                }
                IQuery<IMedia> filterQuery = null;
                if (filter.IsNullOrWhiteSpace() == false)
                {
                    filterQuery = Query<IMedia>.Builder.Where(x => x.Name.Contains(filter));
                }
                return repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, orderBySystemField, filterQuery);
            }
        }

        /// <summary>
        /// Gets descendants of a <see cref="IMedia"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve descendants from</param>
        /// <returns>An Enumerable flat list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetDescendants(int id)
        {
            var media = GetById(id);
            if (media == null)
            {
                return Enumerable.Empty<IMedia>();
            }
            return GetDescendants(media);
        }

        /// <summary>
        /// Gets descendants of a <see cref="IMedia"/> object by its Id
        /// </summary>
        /// <param name="media">The Parent <see cref="IMedia"/> object to retrieve descendants from</param>
        /// <returns>An Enumerable flat list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetDescendants(IMedia media)
        {
            //This is a check to ensure that the path is correct for this entity to avoid problems like: http://issues.umbraco.org/issue/U4-9336 due to data corruption
            if (media.ValidatePath() == false)
                throw new InvalidDataException(string.Format("The content item {0} has an invalid path: {1} with parentID: {2}", media.Id, media.Path, media.ParentId));

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                var pathMatch = media.Path + ",";
                var query = Query<IMedia>.Builder.Where(x => x.Path.StartsWith(pathMatch) && x.Id != media.Id);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets the parent of the current media as an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> to retrieve the parent from</param>
        /// <returns>Parent <see cref="IMedia"/> object</returns>
        public IMedia GetParent(int id)
        {
            var media = GetById(id);
            return GetParent(media);
        }

        /// <summary>
        /// Gets the parent of the current media as an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="media"><see cref="IMedia"/> to retrieve the parent from</param>
        /// <returns>Parent <see cref="IMedia"/> object</returns>
        public IMedia GetParent(IMedia media)
        {
            if (media.ParentId == Constants.System.Root || media.ParentId == Constants.System.RecycleBinMedia)
                return null;

            return GetById(media.ParentId);
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by the Id of the <see cref="IContentType"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetMediaOfMediaType(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                var query = Query<IMedia>.Builder.Where(x => x.ContentTypeId == id);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetRootMedia()
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                var query = Query<IMedia>.Builder.Where(x => x.ParentId == Constants.System.Root);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IMedia"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetMediaInRecycleBin()
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                var query = Query<IMedia>.Builder.Where(x => x.Path.StartsWith(Constants.System.RecycleBinMediaPathPrefix));
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object from the path stored in the 'umbracoFile' property.
        /// </summary>
        /// <param name="mediaPath">Path of the media item to retrieve (for example: /media/1024/koala_403x328.jpg)</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetMediaByPath(string mediaPath)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                return repository.GetMediaByPath(mediaPath);
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IMedia"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/></param>
        /// <returns>True if the media has any children otherwise False</returns>
        public bool HasChildren(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                var query = Query<IMedia>.Builder.Where(x => x.ParentId == id);
                int count = repository.Count(query);
                return count > 0;
            }
        }

        /// <summary>
        /// Moves an <see cref="IMedia"/> object to a new location
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to move</param>
        /// <param name="parentId">Id of the Media's new Parent</param>
        /// <param name="userId">Id of the User moving the Media</param>
        /// <returns>True if moving succeeded, otherwise False</returns>
        public Attempt<OperationStatus> Move(IMedia media, int parentId, int userId = 0)
        {
            //TODO: This all needs to be on the repo layer in one transaction!

            if (media == null) throw new ArgumentNullException("media");
            var evtMsgs = EventMessagesFactory.Get();
            using (new WriteLock(Locker))
            {
                //This ensures that the correct method is called if this method is used to Move to recycle bin.
                if (parentId == Constants.System.RecycleBinMedia)
                {
                    MoveToRecycleBin(media, userId);
                    return OperationStatus.Success(evtMsgs);
                }

                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var originalPath = media.Path;

                    var moveEventInfo = new MoveEventInfo<IMedia>(media, originalPath, parentId);
                    var moveEventArgs = new MoveEventArgs<IMedia>(true, evtMsgs, moveEventInfo);
                    if (uow.Events.DispatchCancelable(Moving, this, moveEventArgs, "Moving"))
                    {
                        uow.Commit();
                        return OperationStatus.Cancelled(evtMsgs); ;
                    }

                    media.ParentId = parentId;
                    if (media.Trashed)
                    {
                        media.ChangeTrashedState(false, parentId);
                    }
                    Save(media, userId, false); //no events!

                    //used to track all the moved entities to be given to the event
                    var moveInfo = new List<MoveEventInfo<IMedia>>
                    {
                        moveEventInfo
                    };

                    //Ensure that relevant properties are updated on children
                    var children = GetChildren(media.Id).ToArray();
                    if (children.Any())
                    {
                        var parentPath = media.Path;
                        var parentLevel = media.Level;
                        var parentTrashed = media.Trashed;
                        var updatedDescendants = UpdatePropertiesOnChildren(children, parentPath, parentLevel, parentTrashed, moveInfo);
                        Save(updatedDescendants, userId, false); //no events!
                    }

                    moveEventArgs.MoveInfoCollection = moveInfo;
                    moveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(Moved, this, moveEventArgs, "Moved");

                    Audit(uow, AuditType.Move, "Move Media performed by user", userId, media.Id);
                    uow.Commit();
                }

                return OperationStatus.Success(evtMsgs);
            }
        }

        /// <summary>
        /// Deletes an <see cref="IMedia"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        public void MoveToRecycleBin(IMedia media, int userId = 0)
        {
            ((IMediaServiceOperations)this).MoveToRecycleBin(media, userId);
        }

        /// <summary>
        /// Permanently deletes an <see cref="IMedia"/> object
        /// </summary>
        /// <remarks>
        /// Please note that this method will completely remove the Media from the database,
        /// but current not from the file system.
        /// </remarks>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        Attempt<OperationStatus> IMediaServiceOperations.Delete(IMedia media, int userId)
        {
            //TODO: IT would be much nicer to mass delete all in one trans in the repo level!
            var evtMsgs = EventMessagesFactory.Get();

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var ret = DeleteUow(uow, media, userId, evtMsgs);
                uow.Commit();
                return ret;
            }
        }

        private Attempt<OperationStatus> DeleteUow(IScopeUnitOfWork uow, IMedia media, int userId, EventMessages evtMsgs)
        {
            var deleteEventArgs = new DeleteEventArgs<IMedia>(media, evtMsgs);
            if (uow.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
            {
                return OperationStatus.Cancelled(evtMsgs);
            }

            // delete children first
            var children = GetChildren(media.Id);
            foreach (var child in children)
            {
                DeleteUow(uow, child, userId, evtMsgs);
            }

            var repository = RepositoryFactory.CreateMediaRepository(uow);
            repository.Delete(media);
            deleteEventArgs.CanCancel = false;
            uow.Events.Dispatch(Deleted, this, deleteEventArgs);

            Audit(uow, AuditType.Delete, "Delete Media performed by user", userId, media.Id);

            return OperationStatus.Success(evtMsgs);
        }

        /// <summary>
        /// Saves a single <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Media</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        Attempt<OperationStatus> IMediaServiceOperations.Save(IMedia media, int userId, bool raiseEvents)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var saveEventArgs = new SaveEventArgs<IMedia>(media, evtMsgs);
                if (raiseEvents && uow.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    uow.Commit();
                    return OperationStatus.Cancelled(evtMsgs);
                }

                if (string.IsNullOrWhiteSpace(media.Name))
                {
                    throw new ArgumentException("Cannot save media with empty name.");
                }

                var repository = RepositoryFactory.CreateMediaRepository(uow);

                //set the creator id if it's new
                if (media.HasIdentity == false)
                {
                    media.CreatorId = userId;
                }

                repository.AddOrUpdate(media);
                repository.AddOrUpdateContentXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                // generate preview for blame history?
                if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                {
                    repository.AddOrUpdatePreviewXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                }

                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(Saved, this, saveEventArgs);
                }

                Audit(uow, AuditType.Save, "Save Media performed by user", userId, media.Id);
                uow.Commit();
            }

            return OperationStatus.Success(evtMsgs);
        }

        /// <summary>
        /// Saves a collection of <see cref="IMedia"/> objects
        /// </summary>
        /// <param name="medias">Collection of <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Media</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        Attempt<OperationStatus> IMediaServiceOperations.Save(IEnumerable<IMedia> medias, int userId, bool raiseEvents)
        {
            var asArray = medias.ToArray();
            var evtMsgs = EventMessagesFactory.Get();

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var saveEventArgs = new SaveEventArgs<IMedia>(asArray, evtMsgs);
                if (raiseEvents && uow.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    uow.Commit();
                    return OperationStatus.Cancelled(evtMsgs);
                }

                var repository = RepositoryFactory.CreateMediaRepository(uow);
                foreach (var media in asArray)
                {
                    media.CreatorId = userId;
                    repository.AddOrUpdate(media);
                    repository.AddOrUpdateContentXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                    // generate preview for blame history?
                    if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                    {
                        repository.AddOrUpdatePreviewXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                    }
                }

                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(Saved, this, saveEventArgs);
                }

                Audit(uow, AuditType.Save, "Save Media items performed by user", userId, -1);
                uow.Commit();
            }

            return OperationStatus.Success(evtMsgs);
        }

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IMedia"/> that resides in the bin
        /// </summary>
        public void EmptyRecycleBin()
        {
            using (new WriteLock(Locker))
            {
                var nodeObjectType = new Guid(Constants.ObjectTypes.Media);

                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var repository = RepositoryFactory.CreateMediaRepository(uow);
                    var entities = repository.GetEntitiesInRecycleBin().ToDictionary(key => key.Id, val => (IEnumerable<Property>)val.Properties);

                    var files = ((MediaRepository)repository).GetFilesInRecycleBinForUploadField();

                    var recycleBinEventArgs = new RecycleBinEventArgs(nodeObjectType, entities, files);
                    if (uow.Events.DispatchCancelable(EmptyingRecycleBin, this, recycleBinEventArgs))
                    {
                        uow.Commit();
                        return;
                    }

                    var success = repository.EmptyRecycleBin();
                    recycleBinEventArgs.CanCancel = false;
                    recycleBinEventArgs.RecycleBinEmptiedSuccessfully = success;
                    uow.Events.Dispatch(EmptiedRecycleBin, this, recycleBinEventArgs);

                    Audit(uow, AuditType.Delete, "Empty Media Recycle Bin performed by user", 0, Constants.System.RecycleBinMedia);
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// Deletes all content of the specified types. All Descendants of deleted content that is not of these types is moved to Recycle Bin.
        /// </summary>
        /// <param name="mediaTypeIds">Id of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional Id of the user issueing the delete operation</param>
        public void DeleteMediaOfTypes(IEnumerable<int> mediaTypeIds, int userId = 0)
        {
            using (new WriteLock(Locker))
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);

                //track the 'root' items of the collection of nodes discovered to delete, we need to use
                //these items to lookup descendants that are not of this doc type so they can be transfered
                //to the recycle bin
                IDictionary<string, IMedia> rootItems;
                var mediaToDelete = this.TrackDeletionsForDeleteContentOfTypes(mediaTypeIds, repository, out rootItems).ToArray();

                var deleteEventArgs = new DeleteEventArgs<IMedia>(mediaToDelete);
                if (uow.Events.DispatchCancelable(Deleting, this, deleteEventArgs, "Deleting"))
                {
                    uow.Commit();
                    return;
                }

                //Determine the items that will need to be recycled (that are children of these content items but not of these content types)
                var mediaToRecycle = this.TrackTrashedForDeleteContentOfTypes(mediaTypeIds, rootItems, repository);

                // move each item to the bin starting with the deepest items
                // nesting uow - technically supported though not ideal
                foreach (var child in mediaToRecycle.OrderByDescending(x => x.Level))
                {
                    MoveToRecycleBinDo(child, userId, true);
                }

                foreach (var content in mediaToDelete)
                {
                    Delete(content, userId);
                }

                Audit(uow, AuditType.Delete,
                    string.Format("Delete Media of Types {0} performed by user", string.Join(",", mediaTypeIds)),
                    userId, Constants.System.Root);
                uow.Commit();
            }
        }


        /// <summary>
        /// Deletes all media of specified type. All children of deleted media is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="mediaTypeId">Id of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user deleting the media</param>
        public void DeleteMediaOfType(int mediaTypeId, int userId = 0)
        {
            DeleteMediaOfTypes(new[] { mediaTypeId }, userId);
        }

        /// <summary>
        /// Deletes an <see cref="IMedia"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        Attempt<OperationStatus> IMediaServiceOperations.MoveToRecycleBin(IMedia media, int userId)
        {
            return MoveToRecycleBinDo(media, userId, false);
        }

        /// <summary>
        /// Deletes an <see cref="IMedia"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        /// <param name="ignoreDescendants">
        /// A boolean indicating to ignore this item's descendant list from also being moved to the recycle bin. This is required for the DeleteContentOfTypes method
        /// because it has already looked up all descendant nodes that will need to be recycled
        /// TODO: Fix all of this, it will require a reasonable refactor and most of this stuff should be done at the repo level instead of service sub operations
        /// </param>
        private Attempt<OperationStatus> MoveToRecycleBinDo(IMedia media, int userId, bool ignoreDescendants)
        {
            if (media == null) throw new ArgumentNullException("media");
            var evtMsgs = EventMessagesFactory.Get();
            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    //Hack: this ensures that the entity's path is valid and if not it fixes/persists it
                    //see: http://issues.umbraco.org/issue/U4-9336
                    media.EnsureValidPath(Logger, entity => GetById(entity.ParentId), QuickUpdate);
                    var originalPath = media.Path;
                    var moveEventInfo = new MoveEventInfo<IMedia>(media, originalPath, Constants.System.RecycleBinMedia);
                    var moveEventArgs = new MoveEventArgs<IMedia>(true, evtMsgs, moveEventInfo);
                    if (uow.Events.DispatchCancelable(Trashing, this, moveEventArgs, "Trashing"))
                    {
                        uow.Commit();
                        return OperationStatus.Cancelled(evtMsgs);
                    }
                    var moveInfo = new List<MoveEventInfo<IMedia>>
                    {
                        moveEventInfo
                    };

                    //get descendents to process of the content item that is being moved to trash - must be done before changing the state below,
                    //must be processed with shallowest levels first
                    var descendants = ignoreDescendants ? Enumerable.Empty<IMedia>() : GetDescendants(media).OrderBy(x => x.Level);

                    //Do the updates for this item
                    var repository = RepositoryFactory.CreateMediaRepository(uow);
                    repository.DeleteContentXml(media);
                    media.ChangeTrashedState(true, Constants.System.RecycleBinMedia);
                    repository.AddOrUpdate(media);

                    //Loop through descendants to update their trash state, but ensuring structure by keeping the ParentId
                    foreach (var descendant in descendants)
                    {
                        repository.DeleteContentXml(descendant);
                        descendant.ChangeTrashedState(true, descendant.ParentId);
                        repository.AddOrUpdate(descendant);

                        moveInfo.Add(new MoveEventInfo<IMedia>(descendant, descendant.Path, descendant.ParentId));
                    }

                    moveEventArgs.MoveInfoCollection = moveInfo;
                    moveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(Trashed, this, moveEventArgs, "Trashed");

                    Audit(uow, AuditType.Move, "Move Media to Recycle Bin performed by user", userId, media.Id);
                    uow.Commit();
                }

                return OperationStatus.Success(evtMsgs);
            }
        }


        /// <summary>
        /// Permanently deletes an <see cref="IMedia"/> object as well as all of its Children.
        /// </summary>
        /// <remarks>
        /// Please note that this method will completely remove the Media from the database,
        /// as well as associated media files from the file system.
        /// </remarks>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        public void Delete(IMedia media, int userId = 0)
        {
            ((IMediaServiceOperations)this).Delete(media, userId);
        }

        /// <summary>
        /// Permanently deletes versions from an <see cref="IMedia"/> object prior to a specific date.
        /// This method will never delete the latest version of a content item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> object to delete versions from</param>
        /// <param name="versionDate">Latest version date</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        public void DeleteVersions(int id, DateTime versionDate, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                DeleteVersions(uow, id, versionDate, userId);
                uow.Commit();
            }
        }

        private void DeleteVersions(IScopeUnitOfWork uow, int id, DateTime versionDate, int userId = 0)
        {
            var deleteRevisionsEventArgs = new DeleteRevisionsEventArgs(id, dateToRetain: versionDate);
            if (uow.Events.DispatchCancelable(DeletingVersions, this, deleteRevisionsEventArgs))
                return;

            var repository = RepositoryFactory.CreateMediaRepository(uow);
            repository.DeleteVersions(id, versionDate);
            deleteRevisionsEventArgs.CanCancel = false;
            uow.Events.Dispatch(DeletedVersions, this, deleteRevisionsEventArgs);
            Audit(uow, AuditType.Delete, "Delete Media by version date performed by user", userId, -1);
        }

        /// <summary>
        /// Permanently deletes specific version(s) from an <see cref="IMedia"/> object.
        /// This method will never delete the latest version of a content item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> object to delete a version from</param>
        /// <param name="versionId">Id of the version to delete</param>
        /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        public void DeleteVersion(int id, Guid versionId, bool deletePriorVersions, int userId = 0)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var deleteRevisionsEventArgs = new DeleteRevisionsEventArgs(id, specificVersion: versionId);
                if (uow.Events.DispatchCancelable(DeletingVersions, this, deleteRevisionsEventArgs))
                {
                    uow.Commit();
                    return;
                }

                if (deletePriorVersions)
                {
                    var content = GetByVersion(versionId);
                    DeleteVersions(uow, id, content.UpdateDate, userId);
                }

                var repository = RepositoryFactory.CreateMediaRepository(uow);
                repository.DeleteVersion(versionId);
                deleteRevisionsEventArgs.CanCancel = false;
                uow.Events.Dispatch(DeletedVersions, this, deleteRevisionsEventArgs);

                Audit(uow, AuditType.Delete, "Delete Media by version performed by user", userId, -1);
                uow.Commit();
            }
        }

        /// <summary>
        /// Saves a single <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public void Save(IMedia media, int userId = 0, bool raiseEvents = true)
        {
            ((IMediaServiceOperations)this).Save(media, userId, raiseEvents);
        }

        /// <summary>
        /// Saves a collection of <see cref="IMedia"/> objects
        /// </summary>
        /// <param name="medias">Collection of <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public void Save(IEnumerable<IMedia> medias, int userId = 0, bool raiseEvents = true)
        {
            ((IMediaServiceOperations)this).Save(medias, userId, raiseEvents);
        }

        /// <summary>
        /// Sorts a collection of <see cref="IMedia"/> objects by updating the SortOrder according
        /// to the ordering of items in the passed in <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="userId"></param>
        /// <param name="raiseEvents"></param>
        /// <returns>True if sorting succeeded, otherwise False</returns>
        public bool Sort(IEnumerable<IMedia> items, int userId = 0, bool raiseEvents = true)
        {
            var asArray = items.ToArray();

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var saveEventArgs = new SaveEventArgs<IMedia>(asArray);
                if (raiseEvents && uow.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    uow.Commit();
                    return false;
                }

                var repository = RepositoryFactory.CreateMediaRepository(uow);
                int i = 0;
                foreach (var media in asArray)
                {
                    //If the current sort order equals that of the media
                    //we don't need to update it, so just increment the sort order
                    //and continue.
                    if (media.SortOrder == i)
                    {
                        i++;
                        continue;
                    }

                    media.SortOrder = i;
                    i++;

                    repository.AddOrUpdate(media);
                    repository.AddOrUpdateContentXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                    // generate preview for blame history?
                    if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                    {
                        repository.AddOrUpdatePreviewXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                    }
                }

                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(Saved, this, saveEventArgs);
                }

                Audit(uow, AuditType.Sort, "Sorting Media performed by user", userId, 0);
                uow.Commit();
            }

            return true;
        }

        /// <summary>
        /// Gets paged media descendants as XML by path
        /// </summary>
        /// <param name="path">Path starts with</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records the query would return without paging</param>
        /// <returns>A paged enumerable of XML entries of media items</returns>
        public IEnumerable<XElement> GetPagedXmlEntries(string path, long pageIndex, int pageSize, out long totalRecords)
        {
            Mandate.ParameterCondition(pageIndex >= 0, "pageIndex");
            Mandate.ParameterCondition(pageSize > 0, "pageSize");

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                return repository.GetPagedXmlEntriesByPath(path, pageIndex, pageSize, null, out totalRecords);
            }
        }

        /// <summary>
        /// Rebuilds all xml content in the cmsContentXml table for all media
        /// </summary>
        /// <param name="contentTypeIds">
        /// Only rebuild the xml structures for the content type ids passed in, if none then rebuilds the structures
        /// for all media
        /// </param>
        public void RebuildXmlStructures(params int[] contentTypeIds)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                repository.RebuildXmlStructures(media => _entitySerializer.Serialize(this, _dataTypeService, _userService, media), contentTypeIds: contentTypeIds.Length == 0 ? null : contentTypeIds);

                Audit(uow, AuditType.Publish, "MediaService.RebuildXmlStructures completed, the xml has been regenerated in the database", 0, -1);
                uow.Commit();
            }
        }

        /// <summary>
        /// Updates the Path and Level on a collection of <see cref="IMedia"/> objects
        /// based on the Parent's Path and Level. Also change the trashed state if relevant.
        /// </summary>
        /// <param name="children">Collection of <see cref="IMedia"/> objects to update</param>
        /// <param name="parentPath">Path of the Parent media</param>
        /// <param name="parentLevel">Level of the Parent media</param>
        /// <param name="parentTrashed">Indicates whether the Parent is trashed or not</param>
        /// <param name="eventInfo">Used to track the objects to be used in the move event</param>
        /// <returns>Collection of updated <see cref="IMedia"/> objects</returns>
        private IEnumerable<IMedia> UpdatePropertiesOnChildren(IEnumerable<IMedia> children, string parentPath, int parentLevel, bool parentTrashed, ICollection<MoveEventInfo<IMedia>> eventInfo)
        {
            var list = new List<IMedia>();
            foreach (var child in children)
            {
                var originalPath = child.Path;
                child.Path = string.Concat(parentPath, ",", child.Id);
                child.Level = parentLevel + 1;
                if (parentTrashed != child.Trashed)
                {
                    child.ChangeTrashedState(parentTrashed, child.ParentId);
                }

                eventInfo.Add(new MoveEventInfo<IMedia>(child, originalPath, child.ParentId));
                list.Add(child);

                var grandkids = GetChildren(child.Id).ToArray();
                if (grandkids.Any())
                {
                    list.AddRange(UpdatePropertiesOnChildren(grandkids, child.Path, child.Level, child.Trashed, eventInfo));
                }
            }
            return list;
        }

        //private void CreateAndSaveMediaXml(XElement xml, int id, UmbracoDatabase db)
        //{
        //    var poco = new ContentXmlDto { NodeId = id, Xml = xml.ToDataString() };
        //    var exists = db.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = id }) != null;
        //    int result = exists ? db.Update(poco) : Convert.ToInt32(db.Insert(poco));
        //}

        private IMediaType FindMediaTypeByAlias(string mediaTypeAlias)
        {
            Mandate.ParameterNotNullOrEmpty(mediaTypeAlias, "mediaTypeAlias");

            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);
                var query = Query<IMediaType>.Builder.Where(x => x.Alias == mediaTypeAlias);
                var mediaTypes = repository.GetByQuery(query);

                if (mediaTypes.Any() == false)
                    throw new Exception(string.Format("No MediaType matching the passed in Alias: '{0}' was found", mediaTypeAlias));

                var mediaType = mediaTypes.First();

                if (mediaType == null)
                    throw new Exception(string.Format("MediaType matching the passed in Alias: '{0}' was null", mediaTypeAlias));

                return mediaType;
            }
        }

        private void Audit(IScopeUnitOfWork uow, AuditType type, string message, int userId, int objectId)
        {
            var repository = RepositoryFactory.CreateAuditRepository(uow);
            repository.AddOrUpdate(new AuditItem(objectId, message, type, userId));
        }

        /// <summary>
        /// Hack: This is used to fix some data if an entity's properties are invalid/corrupt
        /// </summary>
        /// <param name="media"></param>
        private void QuickUpdate(IMedia media)
        {
            if (media == null) throw new ArgumentNullException("media");
            if (media.HasIdentity == false) throw new InvalidOperationException("Cannot update an entity without an Identity");

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repository = RepositoryFactory.CreateMediaRepository(uow);
                repository.AddOrUpdate(media);
                uow.Commit();
            }
        }

        public Stream GetMediaFileContentStream(string filepath)
        {
            if (_mediaFileSystem.FileExists(filepath) == false)
                return null;
            try
            {
                return _mediaFileSystem.OpenFile(filepath);
            }
            catch
            {
                return null; // deal with race conds
            }
        }

        public void SetMediaFileContent(string filepath, Stream stream)
        {
            _mediaFileSystem.AddFile(filepath, stream, true);
        }

        public long GetMediaFileSize(string filepath)
        {
            return _mediaFileSystem.GetSize(filepath);
        }

        public void DeleteMediaFile(string filepath)
        {
            _mediaFileSystem.DeleteFile(filepath, true);
        }

        [Obsolete("This should no longer be used, thumbnail generation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public void GenerateThumbnails(string filepath, PropertyType propertyType)
        {
            using (var filestream = _mediaFileSystem.OpenFile(filepath))
            {
                _mediaFileSystem.GenerateThumbnails(filestream, filepath, propertyType);
            }
        }

        void IMediaService.Move(IMedia media, int parentId, int userId)
        {
            Move(media, parentId, userId);
        }


        #region Event Handlers

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IMediaService, DeleteRevisionsEventArgs> DeletingVersions;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IMediaService, DeleteRevisionsEventArgs> DeletedVersions;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IMediaService, DeleteEventArgs<IMedia>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IMediaService, DeleteEventArgs<IMedia>> Deleted;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IMediaService, SaveEventArgs<IMedia>> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IMediaService, SaveEventArgs<IMedia>> Saved;

        /// <summary>
        /// Occurs before Create
        /// </summary>
        [Obsolete("Use the Created event instead, the Creating and Created events both offer the same functionality, Creating event has been deprecated.")]
        public static event TypedEventHandler<IMediaService, NewEventArgs<IMedia>> Creating;

        /// <summary>
        /// Occurs after Create
        /// </summary>
        /// <remarks>
        /// Please note that the Media object has been created, but not saved
        /// so it does not have an identity yet (meaning no Id has been set).
        /// </remarks>
        public static event TypedEventHandler<IMediaService, NewEventArgs<IMedia>> Created;

        /// <summary>
        /// Occurs before Content is moved to Recycle Bin
        /// </summary>
        public static event TypedEventHandler<IMediaService, MoveEventArgs<IMedia>> Trashing;

        /// <summary>
        /// Occurs after Content is moved to Recycle Bin
        /// </summary>
        public static event TypedEventHandler<IMediaService, MoveEventArgs<IMedia>> Trashed;

        /// <summary>
        /// Occurs before Move
        /// </summary>
        public static event TypedEventHandler<IMediaService, MoveEventArgs<IMedia>> Moving;

        /// <summary>
        /// Occurs after Move
        /// </summary>
        public static event TypedEventHandler<IMediaService, MoveEventArgs<IMedia>> Moved;

        /// <summary>
        /// Occurs before the Recycle Bin is emptied
        /// </summary>
        public static event TypedEventHandler<IMediaService, RecycleBinEventArgs> EmptyingRecycleBin;

        /// <summary>
        /// Occurs after the Recycle Bin has been Emptied
        /// </summary>
        public static event TypedEventHandler<IMediaService, RecycleBinEventArgs> EmptiedRecycleBin;
        #endregion
    }
}
