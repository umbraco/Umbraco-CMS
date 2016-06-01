using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Media Service, which is an easy access to operations involving <see cref="IMedia"/>
    /// </summary>
    public class MediaService : RepositoryService, IMediaService, IMediaServiceOperations
    {
        private readonly EntityXmlSerializer _entitySerializer = new EntityXmlSerializer();
        private readonly IDataTypeService _dataTypeService;
        private readonly IUserService _userService;
        private readonly IEnumerable<IUrlSegmentProvider> _urlSegmentProviders;

        #region Constructors

        public MediaService(
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

        public int Count(string mediaTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repo = uow.CreateRepository<IMediaRepository>();
                var count = repo.Count(mediaTypeAlias);
                uow.Complete();
                return count;
            }
        }

        public int CountChildren(int parentId, string mediaTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repo = uow.CreateRepository<IMediaRepository>();
                var count = repo.CountChildren(parentId, mediaTypeAlias);
                uow.Complete();
                return count;
            }
        }

        public int CountDescendants(int parentId, string mediaTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repo = uow.CreateRepository<IMediaRepository>();
                var count = repo.CountDescendants(parentId, mediaTypeAlias);
                uow.Complete();
                return count;
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

        private void CreateMedia(IDatabaseUnitOfWork uow, Models.Media media, IMedia parent, int userId, bool withIdentity)
        {
            // NOTE: I really hate the notion of these Creating/Created events - they are so inconsistent, I've only just found
            // out that in these 'WithIdentity' methods, the Saving/Saved events were not fired, wtf. Anyways, they're added now.
            var newArgs = parent != null
                ? new NewEventArgs<IMedia>(media, media.ContentType.Alias, parent)
                : new NewEventArgs<IMedia>(media, media.ContentType.Alias, -1);

            if (Creating.IsRaisedEventCancelled(newArgs, this))
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
                // FIXME contentXml?!
                uow.Flush(); // need everything so we can serialize
                repo.AddOrUpdatePreviewXml(media, c => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, c));

                Saved.RaiseEvent(new SaveEventArgs<IMedia>(media, false), this);
            }

            Created.RaiseEvent(new NewEventArgs<IMedia>(media, false, media.ContentType.Alias, parent), this);

            var msg = withIdentity
                ? "Media '{0}' was created with Id {1}"
                : "Media '{0}' was created";
            Audit(AuditType.New, string.Format(msg, media.Name, media.Id), media.CreatorId, media.Id);
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var media = repository.Get(id);
                uow.Complete();
                return media;
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var items = repository.GetAll(idsA);
                uow.Complete();
                return items;
            }
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by its 'UniqueId'
        /// </summary>
        /// <param name="key">Guid key of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetById(Guid key)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.Query.Where(x => x.Key == key);
                var media = repository.GetByQuery(query).SingleOrDefault();
                uow.Complete();
                return media;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by the Id of the <see cref="IMediaType"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetMediaOfMediaType(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.Query.Where(x => x.ContentTypeId == id);
                var items = repository.GetByQuery(query);
                uow.Complete();
                return items;
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.Query.Where(x => x.Level == level && x.Trashed == false);
                var items = repository.GetByQuery(query);
                uow.Complete();
                return items;
            }
        }

        /// <summary>
        /// Gets a specific version of an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="versionId">Id of the version to retrieve</param>
        /// <returns>An <see cref="IMedia"/> item</returns>
        public IMedia GetByVersion(Guid versionId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var media = repository.GetByVersion(versionId);
                uow.Complete();
                return media;
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IMedia"/> objects versions by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetVersions(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var versions = repository.GetAllVersions(id);
                uow.Complete();
                return versions;
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

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var ancestors = repository.GetAll(ids);
                uow.Complete();
                return ancestors;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetChildren(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.Query.Where(x => x.ParentId == id);
                var children = repository.GetByQuery(query).OrderBy(x => x.SortOrder);
                uow.Complete();
                return children;
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IContentRepository>();
                var filterQuery = filter.IsNullOrWhiteSpace()
                    ? null
                    : repository.QueryFactory.Create<IMedia>().Where(x => x.Name.Contains(filter));
                return GetPagedChildren(id, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, true, filterQuery);
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
        /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
        /// <param name="filter"></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IMedia> filter)
        {
            Mandate.ParameterCondition(pageIndex >= 0, "pageIndex");
            Mandate.ParameterCondition(pageSize > 0, "pageSize");
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var filterQuery = filter.IsNullOrWhiteSpace()
                    ? null
                    : repository.QueryFactory.Create<IMedia>().Where(x => x.Name.Contains(filter));
                return GetPagedDescendants(id, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, true, filterQuery);
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
        /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
        /// <param name="filter"></param>        
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IMedia> filter)
        {
            Mandate.ParameterCondition(pageIndex >= 0, nameof(pageIndex));
            Mandate.ParameterCondition(pageSize > 0, nameof(pageSize));

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();

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
        /// Gets descendants of a <see cref="IMedia"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve descendants from</param>
        /// <returns>An Enumerable flat list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetDescendants(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var media = GetById(id);
                if (media == null)
                {
                    uow.Complete(); // else causes rollback
                    return Enumerable.Empty<IMedia>();
                }
                var pathMatch = media.Path + ",";
                var query = repository.Query.Where(x => x.Id != media.Id && x.Path.StartsWith(pathMatch));
                var descendants = repository.GetByQuery(query);
                uow.Complete();
                return descendants;
            }
        }

        /// <summary>
        /// Gets descendants of a <see cref="IMedia"/> object by its Id
        /// </summary>
        /// <param name="media">The Parent <see cref="IMedia"/> object to retrieve descendants from</param>
        /// <returns>An Enumerable flat list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetDescendants(IMedia media)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var pathMatch = media.Path + ",";
                var query = repository.Query.Where(x => x.Id != media.Id && x.Path.StartsWith(pathMatch));
                var descendants = repository.GetByQuery(query);
                uow.Complete();
                return descendants;
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.Query.Where(x => x.ParentId == Constants.System.Root);
                var items = repository.GetByQuery(query);
                uow.Complete();
                return items;
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IMedia"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetMediaInRecycleBin()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                var bin = $"{Constants.System.Root},{Constants.System.RecycleBinMedia},";
                var query = repository.Query.Where(x => x.Path.StartsWith(bin));
                var medias = repository.GetByQuery(query);
                uow.Complete();
                return medias;
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IMedia"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/></param>
        /// <returns>True if the media has any children otherwise False</returns>
        public bool HasChildren(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMediaRepository>();
                var query = repository.Query.Where(x => x.ParentId == id);
                var count = repository.Count(query);
                uow.Complete();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IMediaRepository>();
                var item = repo.GetMediaByPath(mediaPath);
                uow.Complete();
                return item;
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
            ((IMediaServiceOperations)this).Save(media, userId, raiseEvents);
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

            if (raiseEvents && Saving.IsRaisedEventCancelled(new SaveEventArgs<IMedia>(media, evtMsgs), this))
                return OperationStatus.Attempt.Cancel(evtMsgs);

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MediaTree);

                var repository = uow.CreateRepository<IMediaRepository>();
                if (media.HasIdentity == false)
                    media.CreatorId = userId;
                repository.AddOrUpdate(media);
                repository.AddOrUpdateContentXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, m));

                // generate preview for blame history?
                if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                    repository.AddOrUpdatePreviewXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, m));

                uow.Complete();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IMedia>(media, false, evtMsgs), this);
            Audit(AuditType.Save, "Save Media performed by user", userId, media.Id);

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

            if (raiseEvents && Saving.IsRaisedEventCancelled(new SaveEventArgs<IMedia>(mediasA, evtMsgs), this))
                return OperationStatus.Attempt.Cancel(evtMsgs);

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                foreach (var media in mediasA)
                {
                    if (media.HasIdentity == false)
                        media.CreatorId = userId;
                    repository.AddOrUpdate(media);
                    repository.AddOrUpdateContentXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, m));

                    // generate preview for blame history?
                    if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                        repository.AddOrUpdatePreviewXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, m));
                }

                uow.Complete();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IMedia>(mediasA, false, evtMsgs), this);
            Audit(AuditType.Save, "Bulk Save media performed by user", userId == -1 ? 0 : userId, Constants.System.Root);

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
        /// <remarks>
        /// Please note that this method will completely remove the Media from the database,
        /// but current not from the file system.
        /// FIXME uh?
        /// </remarks>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        Attempt<OperationStatus> IMediaServiceOperations.Delete(IMedia media, int userId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMedia>(media, evtMsgs), this))
                return OperationStatus.Attempt.Cancel(evtMsgs);

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();

                DeleteLocked(repository, media);
                uow.Complete();
            }

            Audit(AuditType.Delete, "Delete Media performed by user", userId, media.Id);

            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        private void DeleteLocked(IMediaRepository repository, IMedia media)
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
                Deleted.RaiseEvent(args, this);

                IOHelper.DeleteFiles(args.MediaFilesToDelete, // remove flagged files
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
            if (DeletingVersions.IsRaisedEventCancelled(new DeleteRevisionsEventArgs(id, dateToRetain: versionDate), this))
                return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                repository.DeleteVersions(id, versionDate);
                uow.Complete();
            }

            DeletedVersions.RaiseEvent(new DeleteRevisionsEventArgs(id, false, dateToRetain: versionDate), this);

            Audit(AuditType.Delete, "Delete Media by version date performed by user", userId, Constants.System.Root);
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
            if (DeletingVersions.IsRaisedEventCancelled(new DeleteRevisionsEventArgs(id, /*specificVersion:*/ versionId), this))
                return;

            if (deletePriorVersions)
            {
                var media = GetByVersion(versionId);
                DeleteVersions(id, media.UpdateDate, userId);
            }

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                repository.DeleteVersion(versionId);
                uow.Complete();
            }

            DeletedVersions.RaiseEvent(new DeleteRevisionsEventArgs(id, false, /*specificVersion:*/ versionId), this);
            Audit(AuditType.Delete, "Delete Media by version performed by user", userId, Constants.System.Root);
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

                var originalPath = media.Path;
                if (Trashing.IsRaisedEventCancelled(new MoveEventArgs<IMedia>(new MoveEventInfo<IMedia>(media, originalPath, Constants.System.RecycleBinMedia)), this))
                    return OperationStatus.Attempt.Cancel(evtMsgs); // causes rollback

                PerformMoveLocked(repository, media, Constants.System.RecycleBinMedia, null, userId, moves, true);
                uow.Complete();
            }

            var moveInfo = moves
                .Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId))
                .ToArray();

            Trashed.RaiseEvent(new MoveEventArgs<IMedia>(false, evtMsgs, moveInfo), this);
            Audit(AuditType.Move, "Move Media to Recycle Bin performed by user", userId, media.Id);

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

                if (Moving.IsRaisedEventCancelled(new MoveEventArgs<IMedia>(new MoveEventInfo<IMedia>(media, media.Path, parentId)), this))
                    return; // causes rollback

                // if media was trashed, and since we're not moving to the recycle bin,
                // indicate that the trashed status should be changed to false, else just
                // leave it unchanged
                var trashed = media.Trashed ? false : (bool?)null;

                PerformMoveLocked(repository, media, parentId, parent, userId, moves, trashed);

                uow.Complete();
            }

            var moveInfo = moves //changes
                .Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId))
                .ToArray();

            Moved.RaiseEvent(new MoveEventArgs<IMedia>(false, moveInfo), this);

            Audit(AuditType.Move, "Move Media performed by user", userId, media.Id);
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
                if (EmptyingRecycleBin.IsRaisedEventCancelled(new RecycleBinEventArgs(nodeObjectType), this))
                    return; // causes rollback

                // emptying the recycle bin means deleting whetever is in there - do it properly!
                var query = repository.Query.Where(x => x.ParentId == Constants.System.RecycleBinMedia);
                var medias = repository.GetByQuery(query).ToArray();
                foreach (var media in medias)
                {
                    DeleteLocked(repository, media);
                    deleted.Add(media);
                }

                EmptiedRecycleBin.RaiseEvent(new RecycleBinEventArgs(nodeObjectType, true), this);
                uow.Complete();
            }

            Audit(AuditType.Delete, "Empty Media Recycle Bin performed by user", 0, Constants.System.RecycleBinMedia);
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

            if (raiseEvents && Saving.IsRaisedEventCancelled(new SaveEventArgs<IMedia>(itemsA), this))
                    return false;

            var saved = new List<IMedia>();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
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
                    repository.AddOrUpdateContentXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, m));

                    // generate preview for blame history?
                    if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                        repository.AddOrUpdatePreviewXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, m));
                }

                uow.Complete();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IMedia>(saved, false), this);
            Audit(AuditType.Sort, "Sorting Media performed by user", userId, 0);

            return true;
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

        #endregion

        #region Content Types

        /// <summary>
        /// Deletes all media of specified type. All children of deleted media is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="mediaTypeId">Id of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user deleting the media</param>
        public void DeleteMediaOfType(int mediaTypeId, int userId = 0)
        {
            //TODO: This currently this is called from the ContentTypeService but that needs to change,
            // if we are deleting a content type, we should just delete the data and do this operation slightly differently.
            // This method will recursively go lookup every content item, check if any of it's descendants are
            // of a different type, move them to the recycle bin, then permanently delete the content items.
            // The main problem with this is that for every content item being deleted, events are raised...
            // which we need for many things like keeping caches in sync, but we can surely do this MUCH better.

            var moves = new List<Tuple<IMedia, string>>();

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();

                // fixme what about media that has the contenttype as part of its composition?
                var query = repository.Query.Where(x => x.ContentTypeId == mediaTypeId);
                var medias = repository.GetByQuery(query).ToArray();

                if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMedia>(medias), this))
                    return; // causes rollback

                // order by level, descending, so deepest first - that way, we cannot move
                // a media of the deleted type, to the recycle bin (and then delete it...)
                foreach (var media in medias.OrderByDescending(x => x.ParentId))
                {
                    // if current media has children, move them to trash
                    var m = media;
                    var childQuery = repository.Query.Where(x => x.Path.StartsWith(m.Path));
                    var children = repository.GetByQuery(childQuery);
                    foreach (var child in children.Where(x => x.ContentTypeId != mediaTypeId))
                    {
                        // see MoveToRecycleBin
                        PerformMoveLocked(repository, child, Constants.System.RecycleBinMedia, null, userId, moves, true);
                    }

                    // delete media
                    // triggers the deleted event (and handles the files)
                    DeleteLocked(repository, media);
                }

                uow.Complete();
            }

            var moveInfos = moves
                .Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId))
                .ToArray();
            if (moveInfos.Length > 0)
                Trashed.RaiseEvent(new MoveEventArgs<IMedia>(false, moveInfos), this);

            Audit(AuditType.Delete, $"Delete Media of Type {mediaTypeId} performed by user", userId, Constants.System.Root);
        }

        private IMediaType GetMediaType(string mediaTypeAlias)
        {
            Mandate.ParameterNotNullOrEmpty(mediaTypeAlias, nameof(mediaTypeAlias));

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MediaTypes);

                var repository = uow.CreateRepository<IMediaTypeRepository>();
                var query = repository.Query.Where(x => x.Alias == mediaTypeAlias);
                var mediaType = repository.GetByQuery(query).FirstOrDefault();

                if (mediaType == null)
                    throw new Exception($"No MediaType matching the passed in Alias: '{mediaTypeAlias}' was found"); // causes rollback

                uow.Complete();
                return mediaType;
            }
        }

        #endregion

        #region Xml - Should Move!

        /// <summary>
        /// Rebuilds all xml content in the cmsContentXml table for all media
        /// </summary>
        /// <param name="contentTypeIds">
        /// Only rebuild the xml structures for the content type ids passed in, if none then rebuilds the structures
        /// for all media
        /// </param>
        public void RebuildXmlStructures(params int[] contentTypeIds)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MediaTree);
                var repository = uow.CreateRepository<IMediaRepository>();
                repository.RebuildXmlStructures(
                    media => _entitySerializer.Serialize(this, _dataTypeService, _userService, _urlSegmentProviders, media),
                    contentTypeIds: contentTypeIds.Length == 0 ? null : contentTypeIds);
                uow.Complete();
            }

            Audit(AuditType.Publish, "MediaService.RebuildXmlStructures completed, the xml has been regenerated in the database", 0, -1);
        }

        #endregion
    }
}
