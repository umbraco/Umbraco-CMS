using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services.Changes;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Media Service, which is an easy access to operations involving <see cref="IMedia"/>
    /// </summary>
    public class MediaService : ScopeRepositoryService, IMediaService, IMediaServiceOperations
    {
        private readonly MediaFileSystem _mediaFileSystem;

        #region Constructors

        public MediaService(IScopeUnitOfWorkProvider provider, MediaFileSystem mediaFileSystem, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        {
            _mediaFileSystem = mediaFileSystem;
        }

        #endregion

        #region Count

        public int Count(string mediaTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repo = uow.CreateRepository<IMediaRepository>();
                return repo.Count(mediaTypeAlias);
            }
        }

        public int CountNotTrashed(string mediaTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);

                var mediaTypeId = 0;
                if (string.IsNullOrWhiteSpace(mediaTypeAlias) == false)
                {
                    var trepo = uow.CreateRepository<IMediaTypeRepository>();
                    var mediaType = trepo.Get(mediaTypeAlias);
                    if (mediaType == null) return 0;
                    mediaTypeId = mediaType.Id;
                }

                var repo = uow.CreateRepository<IMediaRepository>();
                var query = repo.QueryT.Where(x => x.Trashed == false);
                if (mediaTypeId > 0)
                    query = query.Where(x => x.ContentTypeId == mediaTypeId);
                return repo.Count(query);
            }
        }

        public int CountChildren(int parentId, string mediaTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repo = uow.CreateRepository<IMediaRepository>();
                return repo.CountChildren(parentId, mediaTypeAlias);
            }
        }

        public int CountDescendants(int parentId, string mediaTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repo = uow.CreateRepository<IMediaRepository>();
                return repo.CountDescendants(parentId, mediaTypeAlias);
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// Creates an <see cref="IMedia"/> object of a specified media type.
        /// </summary>
        /// <remarks>This method simply returns a new, non-persisted, IMedia without any identity. It
        /// is intended as a shortcut to creating new media objects that does not invoke a save
        /// operation against the database.
        /// </remarks>
        /// <param name="name">The name of the media object.</param>
        /// <param name="parentId">The identifier of the parent, or -1.</param>
        /// <param name="mediaTypeAlias">The alias of the media type.</param>
        /// <param name="userId">The optional id of the user creating the media.</param>
        /// <returns>The media object.</returns>
        public IMedia CreateMedia(string name, int parentId, string mediaTypeAlias, int userId = 0)
        {
            var mediaType = GetMediaType(mediaTypeAlias);
            if (mediaType == null)
                throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias));
            var parent = parentId > 0 ? GetById(parentId) : null;
            if (parentId > 0 && parent == null)
                throw new ArgumentException("No media with that id.", nameof(parentId));

            var media = new Models.Media(name, parentId, mediaType);
            CreateMedia(null, media, parent, userId, false);

            return media;
        }

        /// <summary>
        /// Creates an <see cref="IMedia"/> object of a specified media type, at root.
        /// </summary>
        /// <remarks>This method simply returns a new, non-persisted, IMedia without any identity. It
        /// is intended as a shortcut to creating new media objects that does not invoke a save
        /// operation against the database.
        /// </remarks>
        /// <param name="name">The name of the media object.</param>
        /// <param name="mediaTypeAlias">The alias of the media type.</param>
        /// <param name="userId">The optional id of the user creating the media.</param>
        /// <returns>The media object.</returns>
        public IMedia CreateMedia(string name, string mediaTypeAlias, int userId = 0)
        {
            // not locking since not saving anything

            var mediaType = GetMediaType(mediaTypeAlias);
            if (mediaType == null)
                throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias));

            var media = new Models.Media(name, -1, mediaType);
            CreateMedia(null, media, null, userId, false);

            return media;
        }

        /// <summary>
        /// Creates an <see cref="IMedia"/> object of a specified media type, under a parent.
        /// </summary>
        /// <remarks>This method simply returns a new, non-persisted, IMedia without any identity. It
        /// is intended as a shortcut to creating new media objects that does not invoke a save
        /// operation against the database.
        /// </remarks>
        /// <param name="name">The name of the media object.</param>
        /// <param name="parent">The parent media object.</param>
        /// <param name="mediaTypeAlias">The alias of the media type.</param>
        /// <param name="userId">The optional id of the user creating the media.</param>
        /// <returns>The media object.</returns>
        public IMedia CreateMedia(string name, IMedia parent, string mediaTypeAlias, int userId = 0)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                // not locking since not saving anything

                var mediaType = GetMediaType(mediaTypeAlias);
                if (mediaType == null)
                    throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias)); // causes rollback

                var media = new Models.Media(name, parent, mediaType);
                CreateMedia(uow, media, parent, userId, false);

                uow.Complete();
                return media;
            }
        }

        /// <summary>
        /// Creates an <see cref="IMedia"/> object of a specified media type.
        /// </summary>
        /// <remarks>This method returns a new, persisted, IMedia with an identity.</remarks>
        /// <param name="name">The name of the media object.</param>
        /// <param name="parentId">The identifier of the parent, or -1.</param>
        /// <param name="mediaTypeAlias">The alias of the media type.</param>
        /// <param name="userId">The optional id of the user creating the media.</param>
        /// <returns>The media object.</returns>
        public IMedia CreateMediaWithIdentity(string name, int parentId, string mediaTypeAlias, int userId = 0)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                // locking the media tree secures media types too
                uow.WriteLock(Constants.Locks.MediaTree);

                var mediaType = GetMediaType(mediaTypeAlias); // + locks
                if (mediaType == null)
                    throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias)); // causes rollback

                var parent = parentId > 0 ? GetById(parentId) : null; // + locks
                if (parentId > 0 && parent == null)
                    throw new ArgumentException("No media with that id.", nameof(parentId)); // causes rollback

                var media = parentId > 0 ? new Models.Media(name, parent, mediaType) : new Models.Media(name, parentId, mediaType);
                CreateMedia(uow, media, parent, userId, true);

                uow.Complete();
                return media;
            }
        }

        /// <summary>
        /// Creates an <see cref="IMedia"/> object of a specified media type, under a parent.
        /// </summary>
        /// <remarks>This method returns a new, persisted, IMedia with an identity.</remarks>
        /// <param name="name">The name of the media object.</param>
        /// <param name="parent">The parent media object.</param>
        /// <param name="mediaTypeAlias">The alias of the media type.</param>
        /// <param name="userId">The optional id of the user creating the media.</param>
        /// <returns>The media object.</returns>
        public IMedia CreateMediaWithIdentity(string name, IMedia parent, string mediaTypeAlias, int userId = 0)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                // locking the media tree secures media types too
                uow.WriteLock(Constants.Locks.MediaTree);

                var mediaType = GetMediaType(mediaTypeAlias); // + locks
                if (mediaType == null)
                    throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias)); // causes rollback

                var media = new Models.Media(name, parent, mediaType);
                CreateMedia(uow, media, parent, userId, true);

                uow.Complete();
                return media;
            }
        }

        private void CreateMedia(IScopeUnitOfWork uow, Models.Media media, IMedia parent, int userId, bool withIdentity)
        {
            // NOTE: I really hate the notion of these Creating/Created events - they are so inconsistent, I've only just found
            // out that in these 'WithIdentity' methods, the Saving/Saved events were not fired, wtf. Anyways, they're added now.
            var newArgs = parent != null
                ? new NewEventArgs<IMedia>(media, media.ContentType.Alias, parent)
                : new NewEventArgs<IMedia>(media, media.ContentType.Alias, -1);

            if (uow.Events.DispatchCancelable(Creating, this, newArgs))
            {
                media.WasCancelled = true;
                return;
            }

            media.CreatorId = userId;

            if (withIdentity)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMedia>(media), this))
                {
                    media.WasCancelled = true;
                    return;
                }

                var repo = uow.CreateRepository<IMediaRepository>();
                repo.AddOrUpdate(media);

                uow.Events.Dispatch(Saved, this, new SaveEventArgs<IMedia>(media, false));
                uow.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>(media, TreeChangeTypes.RefreshNode).ToEventArgs());
            }

            uow.Events.Dispatch(Created, this, new NewEventArgs<IMedia>(media, false, media.ContentType.Alias, parent));

            var msg = withIdentity
                ? "Media '{0}' was created with Id {1}"
                : "Media '{0}' was created";

            Audit(uow, AuditType.New, string.Format(msg, media.Name, media.Id), media.CreatorId, media.Id);
        }

        #endregion

        #region Get, Has, Is

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by Id
        /// </summary>
        /// <param name="id">Id of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetById(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by Id
        /// </summary>
        /// <param name="ids">Ids of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IEnumerable<IMedia> GetByIds(IEnumerable<int> ids)
        {
            var idsA = ids.ToArray();
            if (idsA.Length == 0) return Enumerable.Empty<IMedia>();

            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                return repository.GetAll(idsA);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by its 'UniqueId'
        /// </summary>
        /// <param name="key">Guid key of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetById(Guid key)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.QueryT.Where(x => x.Key == key);
                return repository.GetByQuery(query).SingleOrDefault();
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by the Id of the <see cref="IMediaType"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetMediaOfMediaType(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.QueryT.Where(x => x.ContentTypeId == id);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Level
        /// </summary>
        /// <param name="level">The level to retrieve Media from</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        /// <remarks>Contrary to most methods, this method filters out trashed media items.</remarks>
        public IEnumerable<IMedia> GetByLevel(int level)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.QueryT.Where(x => x.Level == level && x.Trashed == false);
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
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
            // intentionnaly not locking
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
            //null check otherwise we get exceptions
            if (media.Path.IsNullOrWhiteSpace()) return Enumerable.Empty<IMedia>();

            var rootId = Constants.System.Root.ToInvariantString();
            var ids = media.Path.Split(',')
                .Where(x => x != rootId && x != media.Id.ToString(CultureInfo.InvariantCulture)).Select(int.Parse).ToArray();
            if (ids.Any() == false)
                return new List<IMedia>();

            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.QueryT.Where(x => x.ParentId == id);
                return repository.GetByQuery(query).OrderBy(x => x.SortOrder);
            }
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
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren,
            string orderBy, Direction orderDirection, string filter = "")
        {
            var filterQuery = filter.IsNullOrWhiteSpace()
                ? null
                : Query<IMedia>().Where(x => x.Name.Contains(filter));

            return GetPagedChildren(id, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, true, filterQuery);
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
        /// <param name="filter"></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IMedia> filter)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.QueryT;
                //if the id is System Root, then just get all - NO! does not make sense!
                //if (id != Constants.System.Root)
                query.Where(x => x.ParentId == id);

                return repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, orderBySystemField, filter);
            }
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
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();

                var query = repository.QueryT;
                // always check for a parent - else it will also get decendants (and then you should use the GetPagedDescendants method)
                query.Where(x => x.ParentId == id);

                if (contentTypeFilter != null && contentTypeFilter.Length > 0)
                {
                    query.Where(x => contentTypeFilter.Contains(x.ContentTypeId));
                }

                var filterQuery = filter.IsNullOrWhiteSpace()
                    ? null
                    : Query<IMedia>().Where(x => x.Name.Contains(filter));

                return repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, orderBySystemField, filterQuery);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Descendants from</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalChildren">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, string orderBy = "Path", Direction orderDirection = Direction.Ascending, string filter = "")
        {
            var filterQuery = filter.IsNullOrWhiteSpace()
                ? null
                : Query<IMedia>().Where(x => x.Name.Contains(filter));

            return GetPagedDescendants(id, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, true, filterQuery);
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Descendants from</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalChildren">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
        /// <param name="filter"></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IMedia> filter)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();

                var query = repository.QueryT;
                //if the id is System Root, then just get all
                if (id != Constants.System.Root)
                    query.Where(x => x.Path.SqlContains($",{id},", TextColumnType.NVarchar));
                return repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, orderBySystemField, filter);
            }
        }

        /// <summary>
        /// Gets descendants of a <see cref="IMedia"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve descendants from</param>
        /// <returns>An Enumerable flat list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetDescendants(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var media = GetById(id);
                if (media == null)
                    return Enumerable.Empty<IMedia>();

                var pathMatch = media.Path + ",";
                var query = repository.QueryT.Where(x => x.Id != media.Id && x.Path.StartsWith(pathMatch));
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets descendants of a <see cref="IMedia"/> object by its Id
        /// </summary>
        /// <param name="media">The Parent <see cref="IMedia"/> object to retrieve descendants from</param>
        /// <returns>An Enumerable flat list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetDescendants(IMedia media)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var pathMatch = media.Path + ",";
                var query = repository.QueryT.Where(x => x.Id != media.Id && x.Path.StartsWith(pathMatch));
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
            // intentionnaly not locking
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
        /// Gets a collection of <see cref="IMedia"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetRootMedia()
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.QueryT.Where(x => x.ParentId == Constants.System.Root);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IMedia"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetMediaInRecycleBin()
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var bin = $"{Constants.System.Root},{Constants.System.RecycleBinMedia},";
                var query = repository.QueryT.Where(x => x.Path.StartsWith(bin));
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IMedia"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/></param>
        /// <returns>True if the media has any children otherwise False</returns>
        public bool HasChildren(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.QueryT.Where(x => x.ParentId == id);
                var count = repository.Count(query);
                return count > 0;
            }
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object from the path stored in the 'umbracoFile' property.
        /// </summary>
        /// <param name="mediaPath">Path of the media item to retrieve (for example: /media/1024/koala_403x328.jpg)</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetMediaByPath(string mediaPath)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repo = uow.CreateRepository<IMediaRepository>();
                return repo.GetMediaByPath(mediaPath);
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves a single <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Media</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public void Save(IMedia media, int userId = 0, bool raiseEvents = true)
        {
            ((IMediaServiceOperations) this).Save(media, userId, raiseEvents);
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (raiseEvents && uow.Events.DispatchCancelable(Saving, this, new SaveEventArgs<IMedia>(media, evtMsgs)))
                {
                    uow.Complete();
                    return OperationStatus.Attempt.Cancel(evtMsgs);
                }

                // poor man's validation?
                if (string.IsNullOrWhiteSpace(media.Name))
                    throw new ArgumentException("Media has no name.", nameof(media));

                var isNew = media.IsNewEntity();

                uow.WriteLock(Constants.Locks.MediaTree);

                var repository = uow.CreateRepository<IMediaRepository>();
                if (media.HasIdentity == false)
                    media.CreatorId = userId;
                repository.AddOrUpdate(media);

                if (raiseEvents)
                    uow.Events.Dispatch(Saved, this, new SaveEventArgs<IMedia>(media, false, evtMsgs));
                var changeType = isNew ? TreeChangeTypes.RefreshBranch : TreeChangeTypes.RefreshNode;
                uow.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>(media, changeType).ToEventArgs());
                Audit(uow, AuditType.Save, "Save Media performed by user", userId, media.Id);

                uow.Complete();
            }

            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        /// <summary>
        /// Saves a collection of <see cref="IMedia"/> objects
        /// </summary>
        /// <param name="medias">Collection of <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Media</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public void Save(IEnumerable<IMedia> medias, int userId = 0, bool raiseEvents = true)
        {
            ((IMediaServiceOperations) this).Save(medias, userId, raiseEvents);
        }

        /// <summary>
        /// Saves a collection of <see cref="IMedia"/> objects
        /// </summary>
        /// <param name="medias">Collection of <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Media</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        Attempt<OperationStatus> IMediaServiceOperations.Save(IEnumerable<IMedia> medias, int userId, bool raiseEvents)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var mediasA = medias.ToArray();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (raiseEvents && uow.Events.DispatchCancelable(Saving, this, new SaveEventArgs<IMedia>(mediasA, evtMsgs)))
                {
                    uow.Complete();
                    return OperationStatus.Attempt.Cancel(evtMsgs);
                }

                var treeChanges = mediasA.Select(x => new TreeChange<IMedia>(x,
                    x.IsNewEntity() ? TreeChangeTypes.RefreshBranch : TreeChangeTypes.RefreshNode));

                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                foreach (var media in mediasA)
                {
                    if (media.HasIdentity == false)
                        media.CreatorId = userId;
                    repository.AddOrUpdate(media);
                }

                if (raiseEvents)
                    uow.Events.Dispatch(Saved, this, new SaveEventArgs<IMedia>(mediasA, false, evtMsgs));
                uow.Events.Dispatch(TreeChanged, this, treeChanges.ToEventArgs());
                Audit(uow, AuditType.Save, "Bulk Save media performed by user", userId == -1 ? 0 : userId, Constants.System.Root);

                uow.Complete();
            }

            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        #endregion

        #region Delete

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
            ((IMediaServiceOperations) this).Delete(media, userId);
        }

        /// <summary>
        /// Permanently deletes an <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        Attempt<OperationStatus> IMediaServiceOperations.Delete(IMedia media, int userId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(Deleting, this, new DeleteEventArgs<IMedia>(media, evtMsgs)))
                {
                    uow.Complete();
                    return OperationStatus.Attempt.Cancel(evtMsgs);
                }

                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();

                DeleteLocked(uow, repository, media);

                uow.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>(media, TreeChangeTypes.Remove).ToEventArgs());
                Audit(uow, AuditType.Delete, "Delete Media performed by user", userId, media.Id);

                uow.Complete();
            }

            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        private void DeleteLocked(IScopeUnitOfWork uow, IMediaRepository repository, IMedia media)
        {
            // then recursively delete descendants, bottom-up
            // just repository.Delete + an event
            var stack = new Stack<IMedia>();
            stack.Push(media);
            var level = 1;
            while (stack.Count > 0)
            {
                var c = stack.Peek();
                IMedia[] cc;
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
                var args = new DeleteEventArgs<IMedia>(c, false); // raise event & get flagged files
                uow.Events.Dispatch(Deleted, this, args);

                _mediaFileSystem.DeleteFiles(args.MediaFilesToDelete, // remove flagged files
                    (file, e) => Logger.Error<MediaService>("An error occurred while deleting file attached to nodes: " + file, e));
            }
        }

        //TODO:
        // both DeleteVersions methods below have an issue. Sort of. They do NOT take care of files the way
        // Delete does - for a good reason: the file may be referenced by other, non-deleted, versions. BUT,
        // if that's not the case, then the file will never be deleted, because when we delete the media,
        // the version referencing the file will not be there anymore. SO, we can leak files.

        /// <summary>
        /// Permanently deletes versions from an <see cref="IMedia"/> object prior to a specific date.
        /// This method will never delete the latest version of a media item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> object to delete versions from</param>
        /// <param name="versionDate">Latest version date</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Media object</param>
        public void DeleteVersions(int id, DateTime versionDate, int userId = 0)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                IMediaRepository repository = null;

                DeleteVersions(uow, ref repository, id, versionDate, userId);
                uow.Complete();

                //if (uow.Events.DispatchCancelable(DeletingVersions, this, new DeleteRevisionsEventArgs(id, dateToRetain: versionDate)))
                //{
                //    uow.Complete();
                //    return;
                //}

                //uow.WriteLock(Constants.Locks.MediaTree);
                //var repository = uow.CreateRepository<IMediaRepository>();
                //repository.DeleteVersions(id, versionDate);

                //uow.Events.Dispatch(DeletedVersions, this, new DeleteRevisionsEventArgs(id, false, dateToRetain: versionDate));
                //Audit(uow, AuditType.Delete, "Delete Media by version date performed by user", userId, Constants.System.Root);

                //uow.Complete();
            }
        }

        private void DeleteVersions(IScopeUnitOfWork uow, ref IMediaRepository repository, int id, DateTime versionDate, int userId = 0)
        {
            if (uow.Events.DispatchCancelable(DeletingVersions, this, new DeleteRevisionsEventArgs(id, dateToRetain: versionDate)))
                return;

            if (repository == null)
            {
                repository = uow.CreateRepository<IMediaRepository>();
                uow.WriteLock(Constants.Locks.MediaTree);
            }
            repository.DeleteVersions(id, versionDate);

            uow.Events.Dispatch(DeletedVersions, this, new DeleteRevisionsEventArgs(id, false, dateToRetain: versionDate));
            Audit(uow, AuditType.Delete, "Delete Media by version date performed by user", userId, Constants.System.Root);
        }

        /// <summary>
        /// Permanently deletes specific version(s) from an <see cref="IMedia"/> object.
        /// This method will never delete the latest version of a media item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> object to delete a version from</param>
        /// <param name="versionId">Id of the version to delete</param>
        /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Media object</param>
        public void DeleteVersion(int id, Guid versionId, bool deletePriorVersions, int userId = 0)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(DeletingVersions, this, new DeleteRevisionsEventArgs(id, /*specificVersion:*/ versionId)))
                {
                    uow.Complete();
                    return;
                }

                IMediaRepository repository = null;
                if (deletePriorVersions)
                {
                    var media = GetByVersion(versionId);
                    DeleteVersions(uow, ref repository, id, media.UpdateDate, userId);
                }

                if (repository == null)
                {
                    repository = uow.CreateRepository<IMediaRepository>();
                    uow.WriteLock(Constants.Locks.MediaTree);
                }
                repository.DeleteVersion(versionId);

                uow.Events.Dispatch(DeletedVersions, this, new DeleteRevisionsEventArgs(id, false, /*specificVersion:*/ versionId));
                Audit(uow, AuditType.Delete, "Delete Media by version performed by user", userId, Constants.System.Root);

                uow.Complete();
            }
        }

        #endregion

        #region Move, RecycleBin

        /// <summary>
        /// Deletes an <see cref="IMedia"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        public void MoveToRecycleBin(IMedia media, int userId = 0)
        {
            ((IMediaServiceOperations) this).MoveToRecycleBin(media, userId);
        }

        /// <summary>
        /// Deletes an <see cref="IMedia"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        Attempt<OperationStatus> IMediaServiceOperations.MoveToRecycleBin(IMedia media, int userId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var moves = new List<Tuple<IMedia, string>>();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();

                // fixme - missing 7.6 "ensure valid path" thing here?
                // but then should be in PerformMoveLocked on every moved item?

                var originalPath = media.Path;
                if (uow.Events.DispatchCancelable(Trashing, this, new MoveEventArgs<IMedia>(new MoveEventInfo<IMedia>(media, originalPath, Constants.System.RecycleBinMedia))))
                {
                    uow.Complete();
                    return OperationStatus.Attempt.Cancel(evtMsgs);
                }

                PerformMoveLocked(repository, media, Constants.System.RecycleBinMedia, null, userId, moves, true);

                uow.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>(media, TreeChangeTypes.RefreshBranch).ToEventArgs());

                var moveInfo = moves
                    .Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId))
                    .ToArray();

                uow.Events.Dispatch(Trashed, this, new MoveEventArgs<IMedia>(false, evtMsgs, moveInfo));
                Audit(uow, AuditType.Move, "Move Media to Recycle Bin performed by user", userId, media.Id);

                uow.Complete();
            }

            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        /// <summary>
        /// Moves an <see cref="IMedia"/> object to a new location
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to move</param>
        /// <param name="parentId">Id of the Media's new Parent</param>
        /// <param name="userId">Id of the User moving the Media</param>
        public void Move(IMedia media, int parentId, int userId = 0)
        {
            // if moving to the recycle bin then use the proper method
            if (parentId == Constants.System.RecycleBinMedia)
            {
                MoveToRecycleBin(media, userId);
                return;
            }

            var moves = new List<Tuple<IMedia, string>>();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();

                var parent = parentId == Constants.System.Root ? null : GetById(parentId);
                if (parentId != Constants.System.Root && (parent == null || parent.Trashed))
                    throw new InvalidOperationException("Parent does not exist or is trashed."); // causes rollback

                if (uow.Events.DispatchCancelable(Moving, this, new MoveEventArgs<IMedia>(new MoveEventInfo<IMedia>(media, media.Path, parentId))))
                {
                    uow.Complete();
                    return;
                }

                // if media was trashed, and since we're not moving to the recycle bin,
                // indicate that the trashed status should be changed to false, else just
                // leave it unchanged
                var trashed = media.Trashed ? false : (bool?)null;

                PerformMoveLocked(repository, media, parentId, parent, userId, moves, trashed);

                uow.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>(media, TreeChangeTypes.RefreshBranch).ToEventArgs());

                var moveInfo = moves //changes
                    .Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId))
                    .ToArray();

                uow.Events.Dispatch(Moved, this, new MoveEventArgs<IMedia>(false, moveInfo));
                Audit(uow, AuditType.Move, "Move Media performed by user", userId, media.Id);
                uow.Complete();
            }
        }

        // MUST be called from within WriteLock
        // trash indicates whether we are trashing, un-trashing, or not changing anything
        private void PerformMoveLocked(IMediaRepository repository,
            IMedia media, int parentId, IMedia parent, int userId,
            ICollection<Tuple<IMedia, string>> moves,
            bool? trash)
        {
            media.ParentId = parentId;

            // get the level delta (old pos to new pos)
            var levelDelta = parent == null
                ? 1 - media.Level + (parentId == Constants.System.RecycleBinMedia ? 1 : 0)
                : parent.Level + 1 - media.Level;

            var paths = new Dictionary<int, string>();

            moves.Add(Tuple.Create(media, media.Path)); // capture original path

            // these will be updated by the repo because we changed parentId
            //media.Path = (parent == null ? "-1" : parent.Path) + "," + media.Id;
            //media.SortOrder = ((MediaRepository) repository).NextChildSortOrder(parentId);
            //media.Level += levelDelta;
            PerformMoveMediaLocked(repository, media, userId, trash);

            // BUT media.Path will be updated only when the UOW commits, and
            //  because we want it now, we have to calculate it by ourselves
            //paths[media.Id] = media.Path;
            paths[media.Id] = (parent == null ? (parentId == Constants.System.RecycleBinMedia ? "-1,-21" : "-1") : parent.Path) + "," + media.Id;

            var descendants = GetDescendants(media);
            foreach (var descendant in descendants)
            {
                moves.Add(Tuple.Create(descendant, descendant.Path)); // capture original path

                // update path and level since we do not update parentId
                descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
                descendant.Level += levelDelta;
                PerformMoveMediaLocked(repository, descendant, userId, trash);
            }
        }

        private static void PerformMoveMediaLocked(IMediaRepository repository, IMedia media, int userId,
            bool? trash)
        {
            if (trash.HasValue) ((ContentBase) media).Trashed = trash.Value;
            repository.AddOrUpdate(media);
        }

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IMedia"/> that resides in the bin
        /// </summary>
        public void EmptyRecycleBin()
        {
            var nodeObjectType = new Guid(Constants.ObjectTypes.Media);
            var deleted = new List<IMedia>();
            var evtMsgs = EventMessagesFactory.Get(); // todo - and then?

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();

                // v7 EmptyingRecycleBin and EmptiedRecycleBin events are greatly simplified since
                // each deleted items will have its own deleting/deleted events. so, files and such
                // are managed by Delete, and not here.

                // no idea what those events are for, keep a simplified version
                if (uow.Events.DispatchCancelable(EmptyingRecycleBin, this, new RecycleBinEventArgs(nodeObjectType)))
                {
                    uow.Complete();
                    return;
                }

                // emptying the recycle bin means deleting whetever is in there - do it properly!
                var query = repository.QueryT.Where(x => x.ParentId == Constants.System.RecycleBinMedia);
                var medias = repository.GetByQuery(query).ToArray();
                foreach (var media in medias)
                {
                    DeleteLocked(uow, repository, media);
                    deleted.Add(media);
                }

                uow.Events.Dispatch(EmptiedRecycleBin, this, new RecycleBinEventArgs(nodeObjectType, true));
                uow.Events.Dispatch(TreeChanged, this, deleted.Select(x => new TreeChange<IMedia>(x, TreeChangeTypes.Remove)).ToEventArgs());
                Audit(uow, AuditType.Delete, "Empty Media Recycle Bin performed by user", 0, Constants.System.RecycleBinMedia);
                uow.Complete();
            }
        }

        #endregion

        #region Others

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
            var itemsA = items.ToArray();
            if (itemsA.Length == 0) return true;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (raiseEvents && uow.Events.DispatchCancelable(Saving, this, new SaveEventArgs<IMedia>(itemsA)))
                {
                    uow.Complete();
                    return false;
                }

                var saved = new List<IMedia>();

                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var sortOrder = 0;

                foreach (var media in itemsA)
                {
                    // if the current sort order equals that of the media we don't
                    // need to update it, so just increment the sort order and continue.
                    if (media.SortOrder == sortOrder)
                    {
                        sortOrder++;
                        continue;
                    }

                    // else update
                    media.SortOrder = sortOrder++;
                    // save
                    saved.Add(media);
                    repository.AddOrUpdate(media);
                }

                if (raiseEvents)
                    uow.Events.Dispatch(Saved, this, new SaveEventArgs<IMedia>(saved, false));
                uow.Events.Dispatch(TreeChanged, this, saved.Select(x => new TreeChange<IMedia>(x, TreeChangeTypes.RefreshNode)).ToEventArgs());
                Audit(uow, AuditType.Sort, "Sorting Media performed by user", userId, 0);

                uow.Complete();
            }

            return true;
        }

        #endregion

        #region Private Methods

        private void Audit(IScopeUnitOfWork uow, AuditType type, string message, int userId, int objectId)
        {
            var repo = uow.CreateRepository<IAuditRepository>();
            repo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
        }

        #endregion

        #region File Management

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

        public void DeleteMediaFile(string filepath)
        {
            _mediaFileSystem.DeleteFile(filepath);
        }

        public long GetMediaFileSize(string filepath)
        {
            return _mediaFileSystem.GetSize(filepath);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IMediaService, DeleteEventArgs<IMedia>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IMediaService, DeleteEventArgs<IMedia>> Deleted;

        /// <summary>
        /// Occurs before Delete Versions
        /// </summary>
        public static event TypedEventHandler<IMediaService, DeleteRevisionsEventArgs> DeletingVersions;

        /// <summary>
        /// Occurs after Delete Versions
        /// </summary>
        public static event TypedEventHandler<IMediaService, DeleteRevisionsEventArgs> DeletedVersions;

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
        /// Please note that the Media object has been created, but might not have been saved
        /// so it does not have an identity yet (meaning no Id has been set).
        /// </remarks>
        public static event TypedEventHandler<IMediaService, NewEventArgs<IMedia>> Created;

        /// <summary>
        /// Occurs before Media is moved to Recycle Bin
        /// </summary>
        public static event TypedEventHandler<IMediaService, MoveEventArgs<IMedia>> Trashing;

        /// <summary>
        /// Occurs after Media is moved to Recycle Bin
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

        /// <summary>
        /// Occurs after change.
        /// </summary>
        internal static event TypedEventHandler<IMediaService, TreeChange<IMedia>.EventArgs> TreeChanged;

        #endregion

        #region Content Types

        /// <summary>
        /// Deletes all media of specified type. All children of deleted media is moved to Recycle Bin.
        /// </summary>
        /// <remarks>
        /// <para>This needs extra care and attention as its potentially a dangerous and extensive operation.</para>
        /// <para>Deletes media items of the specified type, and only that type. Does *not* handle content types
        /// inheritance and compositions, which need to be managed outside of this method.</para>
        /// </remarks>
        /// <param name="mediaTypeId">Id of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user deleting the media</param>
        public void DeleteMediaOfTypes(IEnumerable<int> mediaTypeIds, int userId = 0)
        {
            //TODO: This currently this is called from the ContentTypeService but that needs to change,
            // if we are deleting a content type, we should just delete the data and do this operation slightly differently.
            // This method will recursively go lookup every content item, check if any of it's descendants are
            // of a different type, move them to the recycle bin, then permanently delete the content items.
            // The main problem with this is that for every content item being deleted, events are raised...
            // which we need for many things like keeping caches in sync, but we can surely do this MUCH better.

            var changes = new List<TreeChange<IMedia>>();
            var moves = new List<Tuple<IMedia, string>>();
            var mediaTypeIdsA = mediaTypeIds.ToArray();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();

                var query = repository.QueryT.WhereIn(x => x.ContentTypeId, mediaTypeIdsA);
                var medias = repository.GetByQuery(query).ToArray();

                if (uow.Events.DispatchCancelable(Deleting, this, new DeleteEventArgs<IMedia>(medias)))
                {
                    uow.Complete();
                    return;
                }

                // order by level, descending, so deepest first - that way, we cannot move
                // a media of the deleted type, to the recycle bin (and then delete it...)
                foreach (var media in medias.OrderByDescending(x => x.ParentId))
                {
                    // if current media has children, move them to trash
                    var m = media;
                    var childQuery = repository.QueryT.Where(x => x.Path.StartsWith(m.Path));
                    var children = repository.GetByQuery(childQuery);
                    foreach (var child in children.Where(x => mediaTypeIdsA.Contains(x.ContentTypeId) == false))
                    {
                        // see MoveToRecycleBin
                        PerformMoveLocked(repository, child, Constants.System.RecycleBinMedia, null, userId, moves, true);
                        changes.Add(new TreeChange<IMedia>(media, TreeChangeTypes.RefreshBranch));
                    }

                    // delete media
                    // triggers the deleted event (and handles the files)
                    DeleteLocked(uow, repository, media);
                    changes.Add(new TreeChange<IMedia>(media, TreeChangeTypes.Remove));
                }

                var moveInfos = moves
                    .Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId))
                    .ToArray();
                if (moveInfos.Length > 0)
                    uow.Events.Dispatch(Trashed, this, new MoveEventArgs<IMedia>(false, moveInfos));
                uow.Events.Dispatch(TreeChanged, this, changes.ToEventArgs());

                Audit(uow, AuditType.Delete, $"Delete Media of types {string.Join(",", mediaTypeIdsA)} performed by user", userId, Constants.System.Root);

                uow.Complete();
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

        private IMediaType GetMediaType(string mediaTypeAlias)
        {
            if (string.IsNullOrWhiteSpace(mediaTypeAlias)) throw new ArgumentNullOrEmptyException(nameof(mediaTypeAlias));

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTypes);

                var repository = uow.CreateRepository<IMediaTypeRepository>();
                var query = repository.QueryT.Where(x => x.Alias == mediaTypeAlias);
                var mediaType = repository.GetByQuery(query).FirstOrDefault();

                if (mediaType == null)
                    throw new Exception($"No MediaType matching the passed in Alias: '{mediaTypeAlias}' was found"); // causes rollback

                uow.Complete();
                return mediaType;
            }
        }

        #endregion
    }
}
