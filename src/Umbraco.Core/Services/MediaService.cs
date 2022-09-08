using System.Globalization;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services
{
    /// <summary>
    /// Represents the Media Service, which is an easy access to operations involving <see cref="IMedia"/>
    /// </summary>
    public class MediaService : RepositoryService, IMediaService
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IMediaTypeRepository _mediaTypeRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly IEntityRepository _entityRepository;
        private readonly IShortStringHelper _shortStringHelper;

        private readonly MediaFileManager _mediaFileManager;

        #region Constructors

        public MediaService(
            ICoreScopeProvider provider,
            MediaFileManager mediaFileManager,
            ILoggerFactory loggerFactory,
            IEventMessagesFactory eventMessagesFactory,
            IMediaRepository mediaRepository,
            IAuditRepository auditRepository,
            IMediaTypeRepository mediaTypeRepository,
            IEntityRepository entityRepository,
            IShortStringHelper shortStringHelper)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _mediaFileManager = mediaFileManager;
            _mediaRepository = mediaRepository;
            _auditRepository = auditRepository;
            _mediaTypeRepository = mediaTypeRepository;
            _entityRepository = entityRepository;
            _shortStringHelper = shortStringHelper;
        }

        #endregion

        #region Count

        public int Count(string? mediaTypeAlias = null)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            return _mediaRepository.Count(mediaTypeAlias);
        }

        public int CountNotTrashed(string? mediaTypeAlias = null)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);

            var mediaTypeId = 0;
            if (string.IsNullOrWhiteSpace(mediaTypeAlias) == false)
            {
                IMediaType? mediaType = _mediaTypeRepository.Get(mediaTypeAlias);
                if (mediaType == null)
                {
                    return 0;
                }

                mediaTypeId = mediaType.Id;
            }

            IQuery<IMedia> query = Query<IMedia>().Where(x => x.Trashed == false);
            if (mediaTypeId > 0)
            {
                query = query.Where(x => x.ContentTypeId == mediaTypeId);
            }

            return _mediaRepository.Count(query);
        }

        public int CountChildren(int parentId, string? mediaTypeAlias = null)
        {
            using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MediaTree);
                return _mediaRepository.CountChildren(parentId, mediaTypeAlias);
            }
        }

        public int CountDescendants(int parentId, string? mediaTypeAlias = null)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            return _mediaRepository.CountDescendants(parentId, mediaTypeAlias);
        }

        #endregion

        #region Create

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
        public IMedia CreateMedia(string name, Guid parentId, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            IMedia? parent = GetById(parentId);
            return CreateMedia(name, parent, mediaTypeAlias, userId);
        }

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
        public IMedia CreateMedia(string? name, int parentId, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            IMediaType? mediaType = GetMediaType(mediaTypeAlias);
            if (mediaType == null)
            {
                throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias));
            }

            IMedia? parent = parentId > 0 ? GetById(parentId) : null;
            if (parentId > 0 && parent == null)
            {
                throw new ArgumentException("No media with that id.", nameof(parentId));
            }

            if (name != null && name.Length > 255)
            {
                throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
            }

            var media = new Core.Models.Media(name, parentId, mediaType);
            using ICoreScope scope = ScopeProvider.CreateCoreScope();
            CreateMedia(scope, media, parent!, userId, false);
            scope.Complete();

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
        public IMedia CreateMedia(string name, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            // not locking since not saving anything

            IMediaType? mediaType = GetMediaType(mediaTypeAlias);
            if (mediaType == null)
            {
                throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias));
            }

            if (name != null && name.Length > 255)
            {
                throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
            }

            var media = new Core.Models.Media(name, -1, mediaType);
            using ICoreScope scope = ScopeProvider.CreateCoreScope();
            CreateMedia(scope, media, null, userId, false);
            scope.Complete();

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
        public IMedia CreateMedia(string name, IMedia? parent, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            using ICoreScope scope = ScopeProvider.CreateCoreScope();
            // not locking since not saving anything

            IMediaType? mediaType = GetMediaType(mediaTypeAlias);
            if (mediaType == null)
            {
                throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias)); // causes rollback
            }

            if (name != null && name.Length > 255)
            {
                throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
            }

            var media = new Core.Models.Media(name, parent, mediaType);
            CreateMedia(scope, media, parent, userId, false);

            scope.Complete();
            return media;
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
        public IMedia CreateMediaWithIdentity(string name, int parentId, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope();
            // locking the media tree secures media types too
            scope.WriteLock(Constants.Locks.MediaTree);

            IMediaType? mediaType = GetMediaType(mediaTypeAlias); // + locks
            if (mediaType == null)
            {
                throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias)); // causes rollback
            }

            IMedia? parent = parentId > 0 ? GetById(parentId) : null; // + locks
            if (parentId > 0 && parent == null)
            {
                throw new ArgumentException("No media with that id.", nameof(parentId)); // causes rollback
            }

            Models.Media media = parentId > 0 ? new Core.Models.Media(name, parent, mediaType) : new Core.Models.Media(name, parentId, mediaType);
            CreateMedia(scope, media, parent, userId, true);

            scope.Complete();
            return media;
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
        public IMedia CreateMediaWithIdentity(string name, IMedia parent, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            using ICoreScope scope = ScopeProvider.CreateCoreScope();
            // locking the media tree secures media types too
            scope.WriteLock(Constants.Locks.MediaTree);

            IMediaType? mediaType = GetMediaType(mediaTypeAlias); // + locks
            if (mediaType == null)
            {
                throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias)); // causes rollback
            }

            var media = new Core.Models.Media(name, parent, mediaType);
            CreateMedia(scope, media, parent, userId, true);

            scope.Complete();
            return media;
        }

        private void CreateMedia(ICoreScope scope, Core.Models.Media media, IMedia? parent, int userId, bool withIdentity)
        {
            EventMessages eventMessages = EventMessagesFactory.Get();

            media.CreatorId = userId;

            if (withIdentity)
            {
                var savingNotification = new MediaSavingNotification(media, eventMessages);
                if (scope.Notifications.PublishCancelable(savingNotification))
                {
                    return;
                }

                _mediaRepository.Save(media);

                scope.Notifications.Publish(new MediaSavedNotification(media, eventMessages).WithStateFrom(savingNotification));
                scope.Notifications.Publish(new MediaTreeChangeNotification(media, TreeChangeTypes.RefreshNode, eventMessages));
            }

            if (withIdentity == false)
            {
                return;
            }

            Audit(AuditType.New, media.CreatorId, media.Id, $"Media '{media.Name}' was created with Id {media.Id}");
        }

        #endregion

        #region Get, Has, Is

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by Id
        /// </summary>
        /// <param name="id">Id of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia? GetById(int id)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            return _mediaRepository.Get(id);
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by Id
        /// </summary>
        /// <param name="ids">Ids of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IEnumerable<IMedia> GetByIds(IEnumerable<int> ids)
        {
            var idsA = ids.ToArray();
            if (idsA.Length == 0)
            {
                return Enumerable.Empty<IMedia>();
            }

            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            return _mediaRepository.GetMany(idsA);
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by its 'UniqueId'
        /// </summary>
        /// <param name="key">Guid key of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia? GetById(Guid key)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            return _mediaRepository.Get(key);
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by Id
        /// </summary>
        /// <param name="ids">Ids of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IEnumerable<IMedia> GetByIds(IEnumerable<Guid> ids)
        {
            Guid[] idsA = ids.ToArray();
            if (idsA.Length == 0)
            {
                return Enumerable.Empty<IMedia>();
            }

            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            return _mediaRepository.GetMany(idsA);
        }

        /// <inheritdoc />
        public IEnumerable<IMedia> GetPagedOfType(int contentTypeId, long pageIndex, int pageSize, out long totalRecords, IQuery<IMedia>? filter = null, Ordering? ordering = null)
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
                ordering = Ordering.By("sortOrder");
            }

            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.ContentTree);
            return _mediaRepository.GetPage(Query<IMedia>()?.Where(x => x.ContentTypeId == contentTypeId), pageIndex, pageSize, out totalRecords, filter, ordering);
        }

        /// <inheritdoc />
        public IEnumerable<IMedia> GetPagedOfTypes(int[] contentTypeIds, long pageIndex, int pageSize, out long totalRecords, IQuery<IMedia>? filter = null, Ordering? ordering = null)
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
                ordering = Ordering.By("sortOrder");
            }

            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.ContentTree);
            return _mediaRepository.GetPage(
                Query<IMedia>()?.Where(x => contentTypeIds.Contains(x.ContentTypeId)), pageIndex, pageSize, out totalRecords, filter, ordering);
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Level
        /// </summary>
        /// <param name="level">The level to retrieve Media from</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        /// <remarks>Contrary to most methods, this method filters out trashed media items.</remarks>
        public IEnumerable<IMedia>? GetByLevel(int level)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            IQuery<IMedia> query = Query<IMedia>().Where(x => x.Level == level && x.Trashed == false);
            return _mediaRepository.Get(query);
        }

        /// <summary>
        /// Gets a specific version of an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="versionId">Id of the version to retrieve</param>
        /// <returns>An <see cref="IMedia"/> item</returns>
        public IMedia? GetVersion(int versionId)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            return _mediaRepository.GetVersion(versionId);
        }

        /// <summary>
        /// Gets a collection of an <see cref="IMedia"/> objects versions by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetVersions(int id)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            return _mediaRepository.GetAllVersions(id);
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which are ancestors of the current media.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> to retrieve ancestors for</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetAncestors(int id)
        {
            // intentionally not locking
            IMedia? media = GetById(id);
            return GetAncestors(media);
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which are ancestors of the current media.
        /// </summary>
        /// <param name="media"><see cref="IMedia"/> to retrieve ancestors for</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetAncestors(IMedia? media)
        {
            //null check otherwise we get exceptions
            if (media is null || media.Path.IsNullOrWhiteSpace())
            {
                return Enumerable.Empty<IMedia>();
            }

            var rootId = Constants.System.RootString;
            var ids = media.Path.Split(Constants.CharArrays.Comma)
                .Where(x => x != rootId && x != media.Id.ToString(CultureInfo.InvariantCulture))
                .Select(s => int.Parse(s, CultureInfo.InvariantCulture))
                .ToArray();
            if (ids.Any() == false)
            {
                return new List<IMedia>();
            }

            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            return _mediaRepository.GetMany(ids);
        }

        /// <inheritdoc />
        public IEnumerable<IMedia> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IMedia>? filter = null, Ordering? ordering = null)
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
                ordering = Ordering.By("sortOrder");
            }

            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);

            IQuery<IMedia>? query = Query<IMedia>()?.Where(x => x.ParentId == id);
            return _mediaRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering);
        }

        /// <inheritdoc />
        public IEnumerable<IMedia> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IMedia>? filter = null, Ordering? ordering = null)
        {
            if (ordering == null)
            {
                ordering = Ordering.By("Path");
            }

            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);

            //if the id is System Root, then just get all
            if (id != Constants.System.Root)
            {
                TreeEntityPath[] mediaPath = _entityRepository.GetAllPaths(Constants.ObjectTypes.Media, id).ToArray();
                if (mediaPath.Length == 0)
                {
                    totalChildren = 0;
                    return Enumerable.Empty<IMedia>();
                }

                return GetPagedLocked(GetPagedDescendantQuery(mediaPath[0].Path), pageIndex, pageSize, out totalChildren, filter, ordering);
            }

            return GetPagedLocked(GetPagedDescendantQuery(null), pageIndex, pageSize, out totalChildren, filter, ordering);
        }

        private IQuery<IMedia>? GetPagedDescendantQuery(string? mediaPath)
        {
            IQuery<IMedia>? query = Query<IMedia>();
            if (!mediaPath.IsNullOrWhiteSpace())
            {
                query?.Where(x => x.Path.SqlStartsWith(mediaPath + ",", TextColumnType.NVarchar));
            }

            return query;
        }

        private IEnumerable<IMedia> GetPagedLocked(IQuery<IMedia>? query, long pageIndex, int pageSize, out long totalChildren, IQuery<IMedia>? filter, Ordering ordering)
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

            return _mediaRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering);
        }

        /// <summary>
        /// Gets the parent of the current media as an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> to retrieve the parent from</param>
        /// <returns>Parent <see cref="IMedia"/> object</returns>
        public IMedia? GetParent(int id)
        {
            // intentionally not locking
            IMedia? media = GetById(id);
            return GetParent(media);
        }

        /// <summary>
        /// Gets the parent of the current media as an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="media"><see cref="IMedia"/> to retrieve the parent from</param>
        /// <returns>Parent <see cref="IMedia"/> object</returns>
        public IMedia? GetParent(IMedia? media)
        {
            var parentId = media?.ParentId;
            if (parentId is null || media?.ParentId == Constants.System.Root || media?.ParentId == Constants.System.RecycleBinMedia)
            {
                return null;
            }

            return GetById(parentId.Value);
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetRootMedia()
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            IQuery<IMedia> query = Query<IMedia>().Where(x => x.ParentId == Constants.System.Root);
            return _mediaRepository.Get(query);
        }

        /// <inheritdoc />
        public IEnumerable<IMedia> GetPagedMediaInRecycleBin(long pageIndex, int pageSize, out long totalRecords, IQuery<IMedia>? filter = null, Ordering? ordering = null)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            if (ordering == null)
            {
                ordering = Ordering.By("Path");
            }

            scope.ReadLock(Constants.Locks.MediaTree);
            IQuery<IMedia>? query = Query<IMedia>()?.Where(x => x.Path.StartsWith(Constants.System.RecycleBinMediaPathPrefix));
            return _mediaRepository.GetPage(query, pageIndex, pageSize, out totalRecords, filter, ordering);
        }

        /// <summary>
        /// Checks whether an <see cref="IMedia"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/></param>
        /// <returns>True if the media has any children otherwise False</returns>
        public bool HasChildren(int id)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IQuery<IMedia> query = Query<IMedia>().Where(x => x.ParentId == id);
            var count = _mediaRepository.Count(query);
            return count > 0;
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object from the path stored in the 'umbracoFile' property.
        /// </summary>
        /// <param name="mediaPath">Path of the media item to retrieve (for example: /media/1024/koala_403x328.jpg)</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia? GetMediaByPath(string mediaPath)
        {
            using (ScopeProvider.CreateCoreScope(autoComplete: true))
            {
                return _mediaRepository.GetMediaByPath(mediaPath);
            }
        }

        #endregion

        #region Save



        /// <summary>
        /// Saves a single <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Media</param>
        public Attempt<OperationResult?> Save(IMedia media, int userId = Constants.Security.SuperUserId)
        {
            EventMessages eventMessages = EventMessagesFactory.Get();

            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                var savingNotification = new MediaSavingNotification(media, eventMessages);
                if (scope.Notifications.PublishCancelable(savingNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(eventMessages);
                }

                // poor man's validation?
                if (string.IsNullOrWhiteSpace(media.Name))
                {
                    throw new ArgumentException("Media has no name.", nameof(media));
                }

                if (media.Name != null && media.Name.Length > 255)
                {
                    throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
                }

                scope.WriteLock(Constants.Locks.MediaTree);
                if (media.HasIdentity == false)
                {
                    media.CreatorId = userId;
                }

                _mediaRepository.Save(media);
                scope.Notifications.Publish(new MediaSavedNotification(media, eventMessages).WithStateFrom(savingNotification));
                // TODO: See note about suppressing events in content service
                scope.Notifications.Publish(new MediaTreeChangeNotification(media, TreeChangeTypes.RefreshNode, eventMessages));

                Audit(AuditType.Save, userId, media.Id);
                scope.Complete();
            }

            return OperationResult.Attempt.Succeed(eventMessages);
        }

        /// <summary>
        /// Saves a collection of <see cref="IMedia"/> objects
        /// </summary>
        /// <param name="medias">Collection of <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Media</param>
        public Attempt<OperationResult?> Save(IEnumerable<IMedia> medias, int userId = Constants.Security.SuperUserId)
        {
            EventMessages messages = EventMessagesFactory.Get();
            IMedia[] mediasA = medias.ToArray();

            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                var savingNotification = new MediaSavingNotification(mediasA, messages);
                if (scope.Notifications.PublishCancelable(savingNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(messages);
                }

                IEnumerable<TreeChange<IMedia>> treeChanges = mediasA.Select(x => new TreeChange<IMedia>(x, TreeChangeTypes.RefreshNode));

                scope.WriteLock(Constants.Locks.MediaTree);
                foreach (IMedia media in mediasA)
                {
                    if (media.HasIdentity == false)
                    {
                        media.CreatorId = userId;
                    }

                    _mediaRepository.Save(media);
                }

                scope.Notifications.Publish(new MediaSavedNotification(mediasA, messages).WithStateFrom(savingNotification));
                // TODO: See note about suppressing events in content service
                scope.Notifications.Publish(new MediaTreeChangeNotification(treeChanges, messages));
                Audit(AuditType.Save, userId == -1 ? 0 : userId, Constants.System.Root, "Bulk save media");

                scope.Complete();
            }

            return OperationResult.Attempt.Succeed(messages);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Permanently deletes an <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        public Attempt<OperationResult?> Delete(IMedia media, int userId = Constants.Security.SuperUserId)
        {
            EventMessages messages = EventMessagesFactory.Get();

            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                if (scope.Notifications.PublishCancelable(new MediaDeletingNotification(media, messages)))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(messages);
                }

                scope.WriteLock(Constants.Locks.MediaTree);

                DeleteLocked(scope, media, messages);

                scope.Notifications.Publish(new MediaTreeChangeNotification(media, TreeChangeTypes.Remove, messages));
                Audit(AuditType.Delete, userId, media.Id);

                scope.Complete();
            }

            return OperationResult.Attempt.Succeed(messages);
        }

        private void DeleteLocked(ICoreScope scope, IMedia media, EventMessages evtMsgs)
        {
            void DoDelete(IMedia c)
            {
                _mediaRepository.Delete(c);
                scope.Notifications.Publish(new MediaDeletedNotification(c, evtMsgs));

                // media files deleted by QueuingEventDispatcher
            }

            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                //get descendants - ordered from deepest to shallowest
                IEnumerable<IMedia> descendants = GetPagedDescendants(media.Id, page, pageSize, out total, ordering: Ordering.By("Path", Direction.Descending));
                foreach (IMedia c in descendants)
                {
                    DoDelete(c);
                }
            }

            DoDelete(media);
        }

        //TODO: both DeleteVersions methods below have an issue. Sort of. They do NOT take care of files the way
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
        public void DeleteVersions(int id, DateTime versionDate, int userId = Constants.Security.SuperUserId)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope();
            DeleteVersions(scope, true, id, versionDate, userId);
            scope.Complete();
        }

        private void DeleteVersions(ICoreScope scope, bool wlock, int id, DateTime versionDate, int userId = Constants.Security.SuperUserId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();

            var deletingVersionsNotification = new MediaDeletingVersionsNotification(id, evtMsgs, dateToRetain: versionDate);
            if (scope.Notifications.PublishCancelable(deletingVersionsNotification))
            {
                return;
            }

            if (wlock)
            {
                scope.WriteLock(Constants.Locks.MediaTree);
            }

            _mediaRepository.DeleteVersions(id, versionDate);

            scope.Notifications.Publish(new MediaDeletedVersionsNotification(id, evtMsgs, dateToRetain: versionDate).WithStateFrom(deletingVersionsNotification));
            Audit(AuditType.Delete, userId, Constants.System.Root, "Delete Media by version date");
        }

        /// <summary>
        /// Permanently deletes specific version(s) from an <see cref="IMedia"/> object.
        /// This method will never delete the latest version of a media item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> object to delete a version from</param>
        /// <param name="versionId">Id of the version to delete</param>
        /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Media object</param>
        public void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();

            using ICoreScope scope = ScopeProvider.CreateCoreScope();
            var deletingVersionsNotification = new MediaDeletingVersionsNotification(id, evtMsgs, specificVersion: versionId);
            if (scope.Notifications.PublishCancelable(deletingVersionsNotification))
            {
                scope.Complete();
                return;
            }

            if (deletePriorVersions)
            {
                IMedia? media = GetVersion(versionId);
                if (media is not null)
                {
                    DeleteVersions(scope, true, id, media.UpdateDate, userId);
                }
            }
            else
            {
                scope.WriteLock(Constants.Locks.MediaTree);
            }

            _mediaRepository.DeleteVersion(versionId);

            scope.Notifications.Publish(new MediaDeletedVersionsNotification(id, evtMsgs, specificVersion: versionId).WithStateFrom(deletingVersionsNotification));
            Audit(AuditType.Delete, userId, Constants.System.Root, "Delete Media by version");

            scope.Complete();
        }

        #endregion

        #region Move, RecycleBin

        /// <summary>
        /// Deletes an <see cref="IMedia"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        public Attempt<OperationResult?> MoveToRecycleBin(IMedia media, int userId = Constants.Security.SuperUserId)
        {
            EventMessages messages = EventMessagesFactory.Get();
            var moves = new List<(IMedia, string)>();

            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                scope.WriteLock(Constants.Locks.MediaTree);

                // TODO: missing 7.6 "ensure valid path" thing here?
                // but then should be in PerformMoveLocked on every moved item?

                var originalPath = media.Path;

                var moveEventInfo = new MoveEventInfo<IMedia>(media, originalPath, Constants.System.RecycleBinMedia);

                var movingToRecycleBinNotification = new MediaMovingToRecycleBinNotification(moveEventInfo, messages);
                if (scope.Notifications.PublishCancelable(movingToRecycleBinNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(messages);
                }

                PerformMoveLocked(media, Constants.System.RecycleBinMedia, null, userId, moves, true);

                scope.Notifications.Publish(new MediaTreeChangeNotification(media, TreeChangeTypes.RefreshBranch, messages));
                MoveEventInfo<IMedia>[] moveInfo = moves.Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId)).ToArray();
                scope.Notifications.Publish(new MediaMovedToRecycleBinNotification(moveInfo, messages).WithStateFrom(movingToRecycleBinNotification));
                Audit(AuditType.Move, userId, media.Id, "Move Media to recycle bin");

                scope.Complete();
            }

            return OperationResult.Attempt.Succeed(messages);
        }

        /// <summary>
        /// Moves an <see cref="IMedia"/> object to a new location
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to move</param>
        /// <param name="parentId">Id of the Media's new Parent</param>
        /// <param name="userId">Id of the User moving the Media</param>
        public Attempt<OperationResult?> Move(IMedia media, int parentId, int userId = Constants.Security.SuperUserId)
        {
            EventMessages messages = EventMessagesFactory.Get();

            // if moving to the recycle bin then use the proper method
            if (parentId == Constants.System.RecycleBinMedia)
            {
                MoveToRecycleBin(media, userId);
                return OperationResult.Attempt.Succeed(messages);
            }

            var moves = new List<(IMedia, string)>();

            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                scope.WriteLock(Constants.Locks.MediaTree);

                IMedia? parent = parentId == Constants.System.Root ? null : GetById(parentId);
                if (parentId != Constants.System.Root && (parent == null || parent.Trashed))
                {
                    throw new InvalidOperationException("Parent does not exist or is trashed."); // causes rollback
                }

                var moveEventInfo = new MoveEventInfo<IMedia>(media, media.Path, parentId);
                var movingNotification = new MediaMovingNotification(moveEventInfo, messages);
                if (scope.Notifications.PublishCancelable(movingNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(messages);
                }

                // if media was trashed, and since we're not moving to the recycle bin,
                // indicate that the trashed status should be changed to false, else just
                // leave it unchanged
                var trashed = media.Trashed ? false : (bool?)null;

                PerformMoveLocked(media, parentId, parent, userId, moves, trashed);
                scope.Notifications.Publish(new MediaTreeChangeNotification(media, TreeChangeTypes.RefreshBranch, messages));

                MoveEventInfo<IMedia>[] moveInfo = moves //changes
                    .Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId))
                    .ToArray();
                scope.Notifications.Publish(new MediaMovedNotification(moveInfo, messages).WithStateFrom(movingNotification));
                Audit(AuditType.Move, userId, media.Id);
                scope.Complete();
            }
            return OperationResult.Attempt.Succeed(messages);
        }

        // MUST be called from within WriteLock
        // trash indicates whether we are trashing, un-trashing, or not changing anything
        private void PerformMoveLocked(IMedia media, int parentId, IMedia? parent, int userId, ICollection<(IMedia, string)> moves, bool? trash)
        {
            media.ParentId = parentId;

            // get the level delta (old pos to new pos)
            // note that recycle bin (id:-20) level is 0!
            var levelDelta = 1 - media.Level + (parent?.Level ?? 0);

            var paths = new Dictionary<int, string>();

            moves.Add((media, media.Path)); // capture original path

            //need to store the original path to lookup descendants based on it below
            var originalPath = media.Path;

            // these will be updated by the repo because we changed parentId
            //media.Path = (parent == null ? "-1" : parent.Path) + "," + media.Id;
            //media.SortOrder = ((MediaRepository) repository).NextChildSortOrder(parentId);
            //media.Level += levelDelta;
            PerformMoveMediaLocked(media, trash);

            // if uow is not immediate, content.Path will be updated only when the UOW commits,
            // and because we want it now, we have to calculate it by ourselves
            //paths[media.Id] = media.Path;
            paths[media.Id] = (parent == null ? parentId == Constants.System.RecycleBinMedia ? "-1,-21" : Constants.System.RootString : parent.Path) + "," + media.Id;

            const int pageSize = 500;
            IQuery<IMedia>? query = GetPagedDescendantQuery(originalPath);
            long total;
            do
            {
                // We always page a page 0 because for each page, we are moving the result so the resulting total will be reduced
                IEnumerable<IMedia> descendants = GetPagedLocked(query, 0, pageSize, out total, null, Ordering.By("Path"));

                foreach (IMedia descendant in descendants)
                {
                    moves.Add((descendant, descendant.Path)); // capture original path

                    // update path and level since we do not update parentId
                    descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
                    descendant.Level += levelDelta;
                    PerformMoveMediaLocked(descendant, trash);
                }

            }
            while (total > pageSize);

        }

        private void PerformMoveMediaLocked(IMedia media, bool? trash)
        {
            if (trash.HasValue)
            {
                ((ContentBase)media).Trashed = trash.Value;
            }

            _mediaRepository.Save(media);
        }

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IMedia"/> that resides in the bin
        /// </summary>
        /// <param name="userId">Optional Id of the User emptying the Recycle Bin</param>
        public OperationResult EmptyRecycleBin(int userId = Constants.Security.SuperUserId)
        {
            var deleted = new List<IMedia>();
            EventMessages messages = EventMessagesFactory.Get(); // TODO: and then?

            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                scope.WriteLock(Constants.Locks.MediaTree);

                // emptying the recycle bin means deleting whatever is in there - do it properly!
                IQuery<IMedia>? query = Query<IMedia>().Where(x => x.ParentId == Constants.System.RecycleBinMedia);
                IMedia[] medias = _mediaRepository.Get(query)?.ToArray() ?? Array.Empty<IMedia>();

                var emptyingRecycleBinNotification = new MediaEmptyingRecycleBinNotification(medias, messages);
                if (scope.Notifications.PublishCancelable(emptyingRecycleBinNotification))
                {
                    scope.Complete();
                    return OperationResult.Cancel(messages);
                }

                foreach (IMedia media in medias)
                {
                    DeleteLocked(scope, media, messages);
                    deleted.Add(media);
                }
                scope.Notifications.Publish(new MediaEmptiedRecycleBinNotification(deleted, new EventMessages()).WithStateFrom(emptyingRecycleBinNotification));
                scope.Notifications.Publish(new MediaTreeChangeNotification(deleted, TreeChangeTypes.Remove, messages));
                Audit(AuditType.Delete, userId, Constants.System.RecycleBinMedia, "Empty Media recycle bin");
                scope.Complete();
            }

            return OperationResult.Succeed(messages);
        }

        public bool RecycleBinSmells()
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            return _mediaRepository.RecycleBinSmells();
        }

        #endregion

        #region Others

        /// <summary>
        /// Sorts a collection of <see cref="IMedia"/> objects by updating the SortOrder according
        /// to the ordering of items in the passed in <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="userId"></param>
        /// <returns>True if sorting succeeded, otherwise False</returns>
        public bool Sort(IEnumerable<IMedia> items, int userId = Constants.Security.SuperUserId)
        {
            IMedia[] itemsA = items.ToArray();
            if (itemsA.Length == 0)
            {
                return true;
            }

            EventMessages messages = EventMessagesFactory.Get();

            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                var savingNotification = new MediaSavingNotification(itemsA, messages);
                if (scope.Notifications.PublishCancelable(savingNotification))
                {
                    scope.Complete();
                    return false;
                }

                var saved = new List<IMedia>();

                scope.WriteLock(Constants.Locks.MediaTree);
                var sortOrder = 0;

                foreach (IMedia media in itemsA)
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
                    _mediaRepository.Save(media);
                }

                scope.Notifications.Publish(new MediaSavedNotification(itemsA, messages).WithStateFrom(savingNotification));
                // TODO: See note about suppressing events in content service
                scope.Notifications.Publish(new MediaTreeChangeNotification(saved, TreeChangeTypes.RefreshNode, messages));
                Audit(AuditType.Sort, userId, 0);

                scope.Complete();
            }

            return true;

        }

        public ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options)
        {
            using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
            {
                scope.WriteLock(Constants.Locks.MediaTree);

                ContentDataIntegrityReport report = _mediaRepository.CheckDataIntegrity(options);

                if (report.FixedIssues.Count > 0)
                {
                    //The event args needs a content item so we'll make a fake one with enough properties to not cause a null ref
                    var root = new Core.Models.Media("root", -1, new MediaType(_shortStringHelper, -1)) { Id = -1, Key = Guid.Empty };
                    scope.Notifications.Publish(new MediaTreeChangeNotification(root, TreeChangeTypes.RefreshAll, EventMessagesFactory.Get()));
                }

                return report;
            }
        }

        #endregion

        #region Private Methods

        private void Audit(AuditType type, int userId, int objectId, string? message = null)
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId, ObjectTypes.GetName(UmbracoObjectTypes.Media), message));
        }

        #endregion

        #region File Management

        public Stream GetMediaFileContentStream(string filepath)
        {
            if (_mediaFileManager.FileSystem.FileExists(filepath) == false)
            {
                return Stream.Null;
            }

            try
            {
                return _mediaFileManager.FileSystem.OpenFile(filepath);
            }
            catch
            {
                return Stream.Null; // deal with race conds
            }
        }

        public void SetMediaFileContent(string filepath, Stream stream)
        {
            _mediaFileManager.FileSystem.AddFile(filepath, stream, true);
        }

        public void DeleteMediaFile(string filepath)
        {
            _mediaFileManager.FileSystem.DeleteFile(filepath);
        }

        public long GetMediaFileSize(string filepath)
        {
            return _mediaFileManager.FileSystem.GetSize(filepath);
        }

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
        /// <param name="mediaTypeIds">Id of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user deleting the media</param>
        public void DeleteMediaOfTypes(IEnumerable<int> mediaTypeIds, int userId = Constants.Security.SuperUserId)
        {
            // TODO: This currently this is called from the ContentTypeService but that needs to change,
            // if we are deleting a content type, we should just delete the data and do this operation slightly differently.
            // This method will recursively go lookup every content item, check if any of it's descendants are
            // of a different type, move them to the recycle bin, then permanently delete the content items.
            // The main problem with this is that for every content item being deleted, events are raised...
            // which we need for many things like keeping caches in sync, but we can surely do this MUCH better.

            var changes = new List<TreeChange<IMedia>>();
            var moves = new List<(IMedia, string)>();
            var mediaTypeIdsA = mediaTypeIds.ToArray();
            EventMessages messages = EventMessagesFactory.Get();

            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                scope.WriteLock(Constants.Locks.MediaTree);

                IQuery<IMedia>? query = Query<IMedia>().WhereIn(x => x.ContentTypeId, mediaTypeIdsA);
                IMedia[] medias = _mediaRepository.Get(query)?.ToArray() ?? Array.Empty<IMedia>();

                if (scope.Notifications.PublishCancelable(new MediaDeletingNotification(medias, messages)))
                {
                    scope.Complete();
                    return;
                }

                // order by level, descending, so deepest first - that way, we cannot move
                // a media of the deleted type, to the recycle bin (and then delete it...)
                foreach (IMedia media in medias.OrderByDescending(x => x.ParentId))
                {
                    // if current media has children, move them to trash
                    IMedia m = media;
                    IQuery<IMedia>? childQuery = Query<IMedia>().Where(x => x.Path.StartsWith(m.Path));
                    IEnumerable<IMedia>? children = _mediaRepository.Get(childQuery);
                    if (children is not null)
                    {
                        foreach (IMedia child in children.Where(x => mediaTypeIdsA.Contains(x.ContentTypeId) == false))
                        {
                            // see MoveToRecycleBin
                            PerformMoveLocked(child, Constants.System.RecycleBinMedia, null, userId, moves, true);
                            changes.Add(new TreeChange<IMedia>(media, TreeChangeTypes.RefreshBranch));
                        }
                    }

                    // delete media
                    // triggers the deleted event (and handles the files)
                    DeleteLocked(scope, media, messages);
                    changes.Add(new TreeChange<IMedia>(media, TreeChangeTypes.Remove));
                }

                MoveEventInfo<IMedia>[] moveInfos = moves.Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId))
                    .ToArray();
                if (moveInfos.Length > 0)
                {
                    scope.Notifications.Publish(new MediaMovedToRecycleBinNotification(moveInfos, messages));
                }
                scope.Notifications.Publish(new MediaTreeChangeNotification(changes, messages));

                Audit(AuditType.Delete, userId, Constants.System.Root, $"Delete Media of types {string.Join(",", mediaTypeIdsA)}");

                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes all media of specified type. All children of deleted media is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="mediaTypeId">Id of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user deleting the media</param>
        public void DeleteMediaOfType(int mediaTypeId, int userId = Constants.Security.SuperUserId)
        {
            DeleteMediaOfTypes(new[] { mediaTypeId }, userId);
        }

        private IMediaType GetMediaType(string mediaTypeAlias)
        {
            if (mediaTypeAlias == null)
            {
                throw new ArgumentNullException(nameof(mediaTypeAlias));
            }

            if (string.IsNullOrWhiteSpace(mediaTypeAlias))
            {
                throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(mediaTypeAlias));
            }

            using ICoreScope scope = ScopeProvider.CreateCoreScope();
            scope.ReadLock(Constants.Locks.MediaTypes);

            IQuery<IMediaType> query = Query<IMediaType>().Where(x => x.Alias == mediaTypeAlias);
            IMediaType? mediaType = _mediaTypeRepository.Get(query)?.FirstOrDefault();

            if (mediaType == null)
            {
                throw new InvalidOperationException($"No media type matched the specified alias '{mediaTypeAlias}'.");
            }

            scope.Complete();
            return mediaType;
        }

        #endregion


    }
}
