using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Services.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Implement
{
    /// <summary>
    /// Represents the Media Service, which is an easy access to operations involving <see cref="IMedia"/>
    /// </summary>
    public class MediaService : ScopeRepositoryService, IMediaService
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IMediaTypeRepository _mediaTypeRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly IEntityRepository _entityRepository;
        private readonly IShortStringHelper _shortStringHelper;

        private readonly IMediaFileSystem _mediaFileSystem;

        #region Constructors

        public MediaService(IScopeProvider provider, IMediaFileSystem mediaFileSystem, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory,
            IMediaRepository mediaRepository, IAuditRepository auditRepository, IMediaTypeRepository mediaTypeRepository,
            IEntityRepository entityRepository, IShortStringHelper shortStringHelper)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _mediaFileSystem = mediaFileSystem;
            _mediaRepository = mediaRepository;
            _auditRepository = auditRepository;
            _mediaTypeRepository = mediaTypeRepository;
            _entityRepository = entityRepository;
            _shortStringHelper = shortStringHelper;
        }

        #endregion

        #region Count

        public int Count(string mediaTypeAlias = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                return _mediaRepository.Count(mediaTypeAlias);
            }
        }

        public int CountNotTrashed(string mediaTypeAlias = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);

                var mediaTypeId = 0;
                if (string.IsNullOrWhiteSpace(mediaTypeAlias) == false)
                {
                    var mediaType = _mediaTypeRepository.Get(mediaTypeAlias);
                    if (mediaType == null) return 0;
                    mediaTypeId = mediaType.Id;
                }

                var query = Query<IMedia>().Where(x => x.Trashed == false);
                if (mediaTypeId > 0)
                    query = query.Where(x => x.ContentTypeId == mediaTypeId);
                return _mediaRepository.Count(query);
            }
        }

        public int CountChildren(int parentId, string mediaTypeAlias = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                return _mediaRepository.CountChildren(parentId, mediaTypeAlias);
            }
        }

        public int CountDescendants(int parentId, string mediaTypeAlias = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                return _mediaRepository.CountDescendants(parentId, mediaTypeAlias);
            }
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
        public IMedia CreateMedia(string name, Guid parentId, string mediaTypeAlias, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var parent = GetById(parentId);
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
        public IMedia CreateMedia(string name, int parentId, string mediaTypeAlias, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var mediaType = GetMediaType(mediaTypeAlias);
            if (mediaType == null)
                throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias));
            var parent = parentId > 0 ? GetById(parentId) : null;
            if (parentId > 0 && parent == null)
                throw new ArgumentException("No media with that id.", nameof(parentId));
            if (name != null && name.Length > 255)
            {
                throw new InvalidOperationException("Name cannot be more than 255 characters in length."); throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
            }

            var media = new Core.Models.Media(name, parentId, mediaType);
            using (var scope = ScopeProvider.CreateScope())
            {
                CreateMedia(scope, media, parent, userId, false);
                scope.Complete();
            }

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
        public IMedia CreateMedia(string name, string mediaTypeAlias, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            // not locking since not saving anything

            var mediaType = GetMediaType(mediaTypeAlias);
            if (mediaType == null)
                throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias));
            if (name != null && name.Length > 255)
            {
                throw new InvalidOperationException("Name cannot be more than 255 characters in length."); throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
            }

            var media = new Core.Models.Media(name, -1, mediaType);
            using (var scope = ScopeProvider.CreateScope())
            {
                CreateMedia(scope, media, null, userId, false);
                scope.Complete();
            }

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
        public IMedia CreateMedia(string name, IMedia parent, string mediaTypeAlias, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            using (var scope = ScopeProvider.CreateScope())
            {
                // not locking since not saving anything

                var mediaType = GetMediaType(mediaTypeAlias);
                if (mediaType == null)
                    throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias)); // causes rollback
                if (name != null && name.Length > 255)
                {
                    throw new InvalidOperationException("Name cannot be more than 255 characters in length."); throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
                }

                var media = new Core.Models.Media(name, parent, mediaType);
                CreateMedia(scope, media, parent, userId, false);

                scope.Complete();
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
        public IMedia CreateMediaWithIdentity(string name, int parentId, string mediaTypeAlias, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                // locking the media tree secures media types too
                scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);

                var mediaType = GetMediaType(mediaTypeAlias); // + locks
                if (mediaType == null)
                    throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias)); // causes rollback

                var parent = parentId > 0 ? GetById(parentId) : null; // + locks
                if (parentId > 0 && parent == null)
                    throw new ArgumentException("No media with that id.", nameof(parentId)); // causes rollback

                var media = parentId > 0 ? new Core.Models.Media(name, parent, mediaType) : new Core.Models.Media(name, parentId, mediaType);
                CreateMedia(scope, media, parent, userId, true);

                scope.Complete();
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
        public IMedia CreateMediaWithIdentity(string name, IMedia parent, string mediaTypeAlias, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            using (var scope = ScopeProvider.CreateScope())
            {
                // locking the media tree secures media types too
                scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);

                var mediaType = GetMediaType(mediaTypeAlias); // + locks
                if (mediaType == null)
                    throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias)); // causes rollback

                var media = new Core.Models.Media(name, parent, mediaType);
                CreateMedia(scope, media, parent, userId, true);

                scope.Complete();
                return media;
            }
        }

        private void CreateMedia(IScope scope, Core.Models.Media media, IMedia parent, int userId, bool withIdentity)
        {
            var evtMsgs = EventMessagesFactory.Get();

            media.CreatorId = userId;

            if (withIdentity)
            {
                var savingNotification = new MediaSavingNotification(media, evtMsgs);
                if (scope.Notifications.PublishCancelable(savingNotification))
                {
                    return;
                }

                _mediaRepository.Save(media);

                scope.Notifications.Publish(new MediaSavedNotification(media, evtMsgs).WithStateFrom(savingNotification));
                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>(media, TreeChangeTypes.RefreshNode).ToEventArgs());
            }

            if (withIdentity == false)
                return;

            Audit(AuditType.New, media.CreatorId, media.Id, $"Media '{media.Name}' was created with Id {media.Id}");
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                return _mediaRepository.Get(id);
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

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                return _mediaRepository.GetMany(idsA);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by its 'UniqueId'
        /// </summary>
        /// <param name="key">Guid key of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetById(Guid key)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                return _mediaRepository.Get(key);
            }
        }



        /// <summary>
        /// Gets an <see cref="IMedia"/> object by Id
        /// </summary>
        /// <param name="ids">Ids of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IEnumerable<IMedia> GetByIds(IEnumerable<Guid> ids)
        {
            var idsA = ids.ToArray();
            if (idsA.Length == 0) return Enumerable.Empty<IMedia>();

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                return _mediaRepository.GetMany(idsA);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IMedia> GetPagedOfType(int contentTypeId, long pageIndex, int pageSize, out long totalRecords, IQuery<IMedia> filter = null, Ordering ordering = null)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            if (ordering == null)
                ordering = Ordering.By("sortOrder");

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.ContentTree);
                return _mediaRepository.GetPage(
                    Query<IMedia>().Where(x => x.ContentTypeId == contentTypeId),
                    pageIndex, pageSize, out totalRecords, filter, ordering);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IMedia> GetPagedOfTypes(int[] contentTypeIds, long pageIndex, int pageSize, out long totalRecords, IQuery<IMedia> filter = null, Ordering ordering = null)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            if (ordering == null)
                ordering = Ordering.By("sortOrder");

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.ContentTree);
                return _mediaRepository.GetPage(
                    Query<IMedia>().Where(x => contentTypeIds.Contains(x.ContentTypeId)),
                    pageIndex, pageSize, out totalRecords, filter, ordering);
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                var query = Query<IMedia>().Where(x => x.Level == level && x.Trashed == false);
                return _mediaRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets a specific version of an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="versionId">Id of the version to retrieve</param>
        /// <returns>An <see cref="IMedia"/> item</returns>
        public IMedia GetVersion(int versionId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                return _mediaRepository.GetVersion(versionId);
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IMedia"/> objects versions by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetVersions(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                return _mediaRepository.GetAllVersions(id);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which are ancestors of the current media.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> to retrieve ancestors for</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetAncestors(int id)
        {
            // intentionally not locking
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

            var rootId = Cms.Core.Constants.System.RootString;
            var ids = media.Path.Split(Constants.CharArrays.Comma)
                .Where(x => x != rootId && x != media.Id.ToString(CultureInfo.InvariantCulture))
                .Select(int.Parse)
                .ToArray();
            if (ids.Any() == false)
                return new List<IMedia>();

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                return _mediaRepository.GetMany(ids);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IMedia> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren,
            IQuery<IMedia> filter = null, Ordering ordering = null)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            if (ordering == null)
                ordering = Ordering.By("sortOrder");

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);

                var query = Query<IMedia>().Where(x => x.ParentId == id);
                return _mediaRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IMedia> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren,
            IQuery<IMedia> filter = null, Ordering ordering = null)
        {
            if (ordering == null)
                ordering = Ordering.By("Path");

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);

                //if the id is System Root, then just get all
                if (id != Cms.Core.Constants.System.Root)
                {
                    var mediaPath = _entityRepository.GetAllPaths(Cms.Core.Constants.ObjectTypes.Media, id).ToArray();
                    if (mediaPath.Length == 0)
                    {
                        totalChildren = 0;
                        return Enumerable.Empty<IMedia>();
                    }
                    return GetPagedLocked(GetPagedDescendantQuery(mediaPath[0].Path), pageIndex, pageSize, out totalChildren, filter, ordering);
                }
                return GetPagedLocked(GetPagedDescendantQuery(null), pageIndex, pageSize, out totalChildren, filter, ordering);
            }
        }

        private IQuery<IMedia> GetPagedDescendantQuery(string mediaPath)
        {
            var query = Query<IMedia>();
            if (!mediaPath.IsNullOrWhiteSpace())
                query.Where(x => x.Path.SqlStartsWith(mediaPath + ",", TextColumnType.NVarchar));
            return query;
        }

        private IEnumerable<IMedia> GetPagedLocked(IQuery<IMedia> query, long pageIndex, int pageSize, out long totalChildren,
            IQuery<IMedia> filter, Ordering ordering)
        {
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (ordering == null) throw new ArgumentNullException(nameof(ordering));

            return _mediaRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering);
        }

        /// <summary>
        /// Gets the parent of the current media as an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> to retrieve the parent from</param>
        /// <returns>Parent <see cref="IMedia"/> object</returns>
        public IMedia GetParent(int id)
        {
            // intentionally not locking
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
            if (media.ParentId == Cms.Core.Constants.System.Root || media.ParentId == Cms.Core.Constants.System.RecycleBinMedia)
                return null;

            return GetById(media.ParentId);
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetRootMedia()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                var query = Query<IMedia>().Where(x => x.ParentId == Cms.Core.Constants.System.Root);
                return _mediaRepository.Get(query);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IMedia> GetPagedMediaInRecycleBin(long pageIndex, int pageSize, out long totalRecords,
            IQuery<IMedia> filter = null, Ordering ordering = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                if (ordering == null)
                    ordering = Ordering.By("Path");

                scope.ReadLock(Cms.Core.Constants.Locks.MediaTree);
                var query = Query<IMedia>().Where(x => x.Path.StartsWith(Cms.Core.Constants.System.RecycleBinMediaPathPrefix));
                return _mediaRepository.GetPage(query, pageIndex, pageSize, out totalRecords, filter, ordering);
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IMedia"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/></param>
        /// <returns>True if the media has any children otherwise False</returns>
        public bool HasChildren(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IMedia>().Where(x => x.ParentId == id);
                var count = _mediaRepository.Count(query);
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
            using (ScopeProvider.CreateScope(autoComplete: true))
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
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public Attempt<OperationResult> Save(IMedia media, int userId = Cms.Core.Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                var savingNotification = new MediaSavingNotification(media, evtMsgs);
                if (raiseEvents && scope.Notifications.PublishCancelable(savingNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                // poor man's validation?
                if (string.IsNullOrWhiteSpace(media.Name))
                    throw new ArgumentException("Media has no name.", nameof(media));

                if (media.Name != null && media.Name.Length > 255)
                {
                    throw new InvalidOperationException("Name cannot be more than 255 characters in length."); throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
                }

                scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);
                if (media.HasIdentity == false)
                    media.CreatorId = userId;

                _mediaRepository.Save(media);
                if (raiseEvents)
                {
                    scope.Notifications.Publish(new MediaSavedNotification(media, evtMsgs).WithStateFrom(savingNotification));
                }
                var changeType = TreeChangeTypes.RefreshNode;
                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>(media, changeType).ToEventArgs());

                Audit(AuditType.Save, userId, media.Id);
                scope.Complete();
            }

            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        /// <summary>
        /// Saves a collection of <see cref="IMedia"/> objects
        /// </summary>
        /// <param name="medias">Collection of <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Media</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public Attempt<OperationResult> Save(IEnumerable<IMedia> medias, int userId = Cms.Core.Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var mediasA = medias.ToArray();

            using (var scope = ScopeProvider.CreateScope())
            {
                var savingNotification = new MediaSavingNotification(mediasA, evtMsgs);
                if (raiseEvents && scope.Notifications.PublishCancelable(savingNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                var treeChanges = mediasA.Select(x => new TreeChange<IMedia>(x, TreeChangeTypes.RefreshNode));

                scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);
                foreach (var media in mediasA)
                {
                    if (media.HasIdentity == false)
                        media.CreatorId = userId;
                    _mediaRepository.Save(media);
                }

                if (raiseEvents)
                {
                    scope.Notifications.Publish(new MediaSavedNotification(mediasA, evtMsgs).WithStateFrom(savingNotification));
                }
                scope.Events.Dispatch(TreeChanged, this, treeChanges.ToEventArgs());
                Audit(AuditType.Save, userId == -1 ? 0 : userId, Cms.Core.Constants.System.Root, "Bulk save media");

                scope.Complete();
            }

            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Permanently deletes an <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        public Attempt<OperationResult> Delete(IMedia media, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                if (scope.Notifications.PublishCancelable(new MediaDeletingNotification(media, evtMsgs)))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);

                DeleteLocked(scope, media, evtMsgs);

                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>(media, TreeChangeTypes.Remove).ToEventArgs());
                Audit(AuditType.Delete, userId, media.Id);

                scope.Complete();
            }

            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        private void DeleteLocked(IScope scope, IMedia media, EventMessages evtMsgs)
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
                var descendants = GetPagedDescendants(media.Id, page, pageSize, out total, ordering: Ordering.By("Path", Direction.Descending));
                foreach (var c in descendants)
                    DoDelete(c);
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
        public void DeleteVersions(int id, DateTime versionDate, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                DeleteVersions(scope, true, id, versionDate, userId);
                scope.Complete();
            }
        }

        private void DeleteVersions(IScope scope, bool wlock, int id, DateTime versionDate, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            var deletingVersionsNotification = new MediaDeletingVersionsNotification(id, evtMsgs, dateToRetain: versionDate);
            if (scope.Notifications.PublishCancelable(deletingVersionsNotification))
            {
                return;
            }

            if (wlock)
                scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);
            _mediaRepository.DeleteVersions(id, versionDate);

            scope.Notifications.Publish(new MediaDeletedVersionsNotification(id, evtMsgs, dateToRetain: versionDate).WithStateFrom(deletingVersionsNotification));
            Audit(AuditType.Delete, userId, Cms.Core.Constants.System.Root, "Delete Media by version date");
        }

        /// <summary>
        /// Permanently deletes specific version(s) from an <see cref="IMedia"/> object.
        /// This method will never delete the latest version of a media item.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> object to delete a version from</param>
        /// <param name="versionId">Id of the version to delete</param>
        /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Media object</param>
        public void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                var deletingVersionsNotification = new MediaDeletingVersionsNotification(id, evtMsgs, specificVersion: versionId);
                if (scope.Notifications.PublishCancelable(deletingVersionsNotification))
                {
                    scope.Complete();
                    return;
                }

                if (deletePriorVersions)
                {
                    var media = GetVersion(versionId);
                    DeleteVersions(scope, true, id, media.UpdateDate, userId);
                }
                else
                {
                    scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);
                }

                _mediaRepository.DeleteVersion(versionId);

                scope.Notifications.Publish(new MediaDeletedVersionsNotification(id, evtMsgs, specificVersion: versionId).WithStateFrom(deletingVersionsNotification));
                Audit(AuditType.Delete, userId, Cms.Core.Constants.System.Root, "Delete Media by version");

                scope.Complete();
            }
        }

        #endregion

        #region Move, RecycleBin

        /// <summary>
        /// Deletes an <see cref="IMedia"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        public Attempt<OperationResult> MoveToRecycleBin(IMedia media, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var moves = new List<(IMedia, string)>();

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);

                // TODO: missing 7.6 "ensure valid path" thing here?
                // but then should be in PerformMoveLocked on every moved item?

                var originalPath = media.Path;

                var moveEventInfo = new MoveEventInfo<IMedia>(media, originalPath, Cms.Core.Constants.System.RecycleBinMedia);

                var movingToRecycleBinNotification = new MediaMovingToRecycleBinNotification(moveEventInfo, evtMsgs);
                if (scope.Notifications.PublishCancelable(movingToRecycleBinNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                PerformMoveLocked(media, Cms.Core.Constants.System.RecycleBinMedia, null, userId, moves, true);

                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>(media, TreeChangeTypes.RefreshBranch).ToEventArgs());
                var moveInfo = moves.Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId)).ToArray();
                scope.Notifications.Publish(new MediaMovedToRecycleBinNotification(moveInfo, evtMsgs).WithStateFrom(movingToRecycleBinNotification));
                Audit(AuditType.Move, userId, media.Id, "Move Media to recycle bin");

                scope.Complete();
            }

            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        /// <summary>
        /// Moves an <see cref="IMedia"/> object to a new location
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to move</param>
        /// <param name="parentId">Id of the Media's new Parent</param>
        /// <param name="userId">Id of the User moving the Media</param>
        public Attempt<OperationResult> Move(IMedia media, int parentId, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            // if moving to the recycle bin then use the proper method
            if (parentId == Cms.Core.Constants.System.RecycleBinMedia)
            {
                MoveToRecycleBin(media, userId);
                return OperationResult.Attempt.Succeed(evtMsgs);
            }

            var moves = new List<(IMedia, string)>();

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);

                var parent = parentId == Cms.Core.Constants.System.Root ? null : GetById(parentId);
                if (parentId != Cms.Core.Constants.System.Root && (parent == null || parent.Trashed))
                    throw new InvalidOperationException("Parent does not exist or is trashed."); // causes rollback

                var moveEventInfo = new MoveEventInfo<IMedia>(media, media.Path, parentId);
                var movingNotification = new MediaMovingNotification(moveEventInfo, evtMsgs);
                if (scope.Notifications.PublishCancelable(movingNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                // if media was trashed, and since we're not moving to the recycle bin,
                // indicate that the trashed status should be changed to false, else just
                // leave it unchanged
                var trashed = media.Trashed ? false : (bool?)null;

                PerformMoveLocked(media, parentId, parent, userId, moves, trashed);
                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>(media, TreeChangeTypes.RefreshBranch).ToEventArgs());
                var moveInfo = moves //changes
                    .Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId))
                    .ToArray();
                scope.Notifications.Publish(new MediaMovedNotification(moveInfo, evtMsgs).WithStateFrom(movingNotification));
                Audit(AuditType.Move, userId, media.Id);
                scope.Complete();
            }
            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        // MUST be called from within WriteLock
        // trash indicates whether we are trashing, un-trashing, or not changing anything
        private void PerformMoveLocked(IMedia media, int parentId, IMedia parent, int userId, ICollection<(IMedia, string)> moves, bool? trash)
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
            PerformMoveMediaLocked(media, userId, trash);

            // if uow is not immediate, content.Path will be updated only when the UOW commits,
            // and because we want it now, we have to calculate it by ourselves
            //paths[media.Id] = media.Path;
            paths[media.Id] = (parent == null ? (parentId == Cms.Core.Constants.System.RecycleBinMedia ? "-1,-21" : Cms.Core.Constants.System.RootString) : parent.Path) + "," + media.Id;

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
                    PerformMoveMediaLocked(descendant, userId, trash);
                }

            } while (total > pageSize);

        }

        private void PerformMoveMediaLocked(IMedia media, int userId, bool? trash)
        {
            if (trash.HasValue) ((ContentBase)media).Trashed = trash.Value;
            _mediaRepository.Save(media);
        }

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IMedia"/> that resides in the bin
        /// </summary>
        /// <param name="userId">Optional Id of the User emptying the Recycle Bin</param>
        public OperationResult EmptyRecycleBin(int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            var deleted = new List<IMedia>();
            var evtMsgs = EventMessagesFactory.Get(); // TODO: and then?

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);

                // no idea what those events are for, keep a simplified version

                // v7 EmptyingRecycleBin and EmptiedRecycleBin events are greatly simplified since
                // each deleted items will have its own deleting/deleted events. so, files and such
                // are managed by Delete, and not here.
                var emptyingRecycleBinNotification = new MediaEmptyingRecycleBinNotification(evtMsgs);
                if (scope.Notifications.PublishCancelable(emptyingRecycleBinNotification))
                {
                    scope.Complete();
                    return OperationResult.Cancel(evtMsgs);
                }

                // emptying the recycle bin means deleting whatever is in there - do it properly!
                var query = Query<IMedia>().Where(x => x.ParentId == Cms.Core.Constants.System.RecycleBinMedia);
                var medias = _mediaRepository.Get(query).ToArray();
                foreach (var media in medias)
                {
                    DeleteLocked(scope, media, evtMsgs);
                    deleted.Add(media);
                }
                scope.Notifications.Publish(new MediaEmptiedRecycleBinNotification(new EventMessages()).WithStateFrom(emptyingRecycleBinNotification));
                scope.Events.Dispatch(TreeChanged, this, deleted.Select(x => new TreeChange<IMedia>(x, TreeChangeTypes.Remove)).ToEventArgs());
                Audit(AuditType.Delete, userId, Cms.Core.Constants.System.RecycleBinMedia, "Empty Media recycle bin");
                scope.Complete();
            }

            return OperationResult.Succeed(evtMsgs);
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
        public bool Sort(IEnumerable<IMedia> items, int userId = Cms.Core.Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            var itemsA = items.ToArray();
            if (itemsA.Length == 0) return true;

            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                var savingNotification = new MediaSavingNotification(itemsA, evtMsgs);
                if (raiseEvents && scope.Notifications.PublishCancelable(savingNotification))
                {
                    scope.Complete();
                    return false;
                }

                var saved = new List<IMedia>();

                scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);
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
                    _mediaRepository.Save(media);
                }

                if (raiseEvents)
                {
                    scope.Notifications.Publish(new MediaSavedNotification(itemsA, evtMsgs).WithStateFrom(savingNotification));
                }
                scope.Events.Dispatch(TreeChanged, this, saved.Select(x => new TreeChange<IMedia>(x, TreeChangeTypes.RefreshNode)).ToEventArgs());
                Audit(AuditType.Sort, userId, 0);

                scope.Complete();
            }

            return true;

        }

        public ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);

                var report = _mediaRepository.CheckDataIntegrity(options);

                if (report.FixedIssues.Count > 0)
                {
                    //The event args needs a content item so we'll make a fake one with enough properties to not cause a null ref
                    var root = new Core.Models.Media("root", -1, new MediaType(_shortStringHelper, -1)) { Id = -1, Key = Guid.Empty };
                    scope.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>.EventArgs(new TreeChange<IMedia>(root, TreeChangeTypes.RefreshAll)));
                }

                return report;
            }
        }

        #endregion

        #region Private Methods

        private void Audit(AuditType type, int userId, int objectId, string message = null)
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId, ObjectTypes.GetName(UmbracoObjectTypes.Media), message));
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
        /// Occurs after change.
        /// </summary>
        /// <remarks>
        /// This event needs to be rewritten using notifications instead
        /// </remarks>
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
        /// <param name="mediaTypeIds">Id of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user deleting the media</param>
        public void DeleteMediaOfTypes(IEnumerable<int> mediaTypeIds, int userId = Cms.Core.Constants.Security.SuperUserId)
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
            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Cms.Core.Constants.Locks.MediaTree);

                var query = Query<IMedia>().WhereIn(x => x.ContentTypeId, mediaTypeIdsA);
                var medias = _mediaRepository.Get(query).ToArray();

                if (scope.Notifications.PublishCancelable(new MediaDeletingNotification(medias, evtMsgs)))
                {
                    scope.Complete();
                    return;
                }

                // order by level, descending, so deepest first - that way, we cannot move
                // a media of the deleted type, to the recycle bin (and then delete it...)
                foreach (var media in medias.OrderByDescending(x => x.ParentId))
                {
                    // if current media has children, move them to trash
                    var m = media;
                    var childQuery = Query<IMedia>().Where(x => x.Path.StartsWith(m.Path));
                    var children = _mediaRepository.Get(childQuery);
                    foreach (var child in children.Where(x => mediaTypeIdsA.Contains(x.ContentTypeId) == false))
                    {
                        // see MoveToRecycleBin
                        PerformMoveLocked(child, Cms.Core.Constants.System.RecycleBinMedia, null, userId, moves, true);
                        changes.Add(new TreeChange<IMedia>(media, TreeChangeTypes.RefreshBranch));
                    }

                    // delete media
                    // triggers the deleted event (and handles the files)
                    DeleteLocked(scope, media, evtMsgs);
                    changes.Add(new TreeChange<IMedia>(media, TreeChangeTypes.Remove));
                }

                var moveInfos = moves.Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId))
                    .ToArray();
                if (moveInfos.Length > 0)
                {
                    scope.Notifications.Publish(new MediaMovedToRecycleBinNotification(moveInfos, evtMsgs));
                }
                scope.Events.Dispatch(TreeChanged, this, changes.ToEventArgs());

                Audit(AuditType.Delete, userId, Cms.Core.Constants.System.Root, $"Delete Media of types {string.Join(",", mediaTypeIdsA)}");

                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes all media of specified type. All children of deleted media is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="mediaTypeId">Id of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user deleting the media</param>
        public void DeleteMediaOfType(int mediaTypeId, int userId = Cms.Core.Constants.Security.SuperUserId)
        {
            DeleteMediaOfTypes(new[] { mediaTypeId }, userId);
        }

        private IMediaType GetMediaType(string mediaTypeAlias)
        {
            if (mediaTypeAlias == null) throw new ArgumentNullException(nameof(mediaTypeAlias));
            if (string.IsNullOrWhiteSpace(mediaTypeAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(mediaTypeAlias));

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.ReadLock(Cms.Core.Constants.Locks.MediaTypes);

                var query = Query<IMediaType>().Where(x => x.Alias == mediaTypeAlias);
                var mediaType = _mediaTypeRepository.Get(query).FirstOrDefault();

                if (mediaType == null)
                    throw new InvalidOperationException($"No media type matched the specified alias '{mediaTypeAlias}'.");

                scope.Complete();
                return mediaType;
            }
        }

        #endregion


    }
}
