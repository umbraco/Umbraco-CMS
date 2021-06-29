using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services.Changes;

namespace Umbraco.Core.Services.Implement
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

        private readonly IMediaFileSystem _mediaFileSystem;

        #region Constructors

        public MediaService(IScopeProvider provider, IMediaFileSystem mediaFileSystem, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IMediaRepository mediaRepository, IAuditRepository auditRepository, IMediaTypeRepository mediaTypeRepository,
            IEntityRepository entityRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _mediaFileSystem = mediaFileSystem;
            _mediaRepository = mediaRepository;
            _auditRepository = auditRepository;
            _mediaTypeRepository = mediaTypeRepository;
            _entityRepository = entityRepository;
        }

        #endregion

        #region Count

        public int Count(string mediaTypeAlias = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MediaTree);
                return _mediaRepository.Count(mediaTypeAlias);
            }
        }

        public int CountNotTrashed(string mediaTypeAlias = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MediaTree);

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
                scope.ReadLock(Constants.Locks.MediaTree);
                return _mediaRepository.CountChildren(parentId, mediaTypeAlias);
            }
        }

        public int CountDescendants(int parentId, string mediaTypeAlias = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MediaTree);
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
        public IMedia CreateMedia(string name, Guid parentId, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
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
        public IMedia CreateMedia(string name, int parentId, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
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

            var media = new Models.Media(name, parentId, mediaType);
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
        public IMedia CreateMedia(string name, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            // not locking since not saving anything

            var mediaType = GetMediaType(mediaTypeAlias);
            if (mediaType == null)
                throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias));
            if (name != null && name.Length > 255)
            {
                throw new InvalidOperationException("Name cannot be more than 255 characters in length."); throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
            }

            var media = new Models.Media(name, -1, mediaType);
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
        public IMedia CreateMedia(string name, IMedia parent, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
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

                var media = new Models.Media(name, parent, mediaType);
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
        public IMedia CreateMediaWithIdentity(string name, int parentId, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                // locking the media tree secures media types too
                scope.WriteLock(Constants.Locks.MediaTree);

                var mediaType = GetMediaType(mediaTypeAlias); // + locks
                if (mediaType == null)
                    throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias)); // causes rollback

                var parent = parentId > 0 ? GetById(parentId) : null; // + locks
                if (parentId > 0 && parent == null)
                    throw new ArgumentException("No media with that id.", nameof(parentId)); // causes rollback

                var media = parentId > 0 ? new Models.Media(name, parent, mediaType) : new Models.Media(name, parentId, mediaType);
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
        public IMedia CreateMediaWithIdentity(string name, IMedia parent, string mediaTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            using (var scope = ScopeProvider.CreateScope())
            {
                // locking the media tree secures media types too
                scope.WriteLock(Constants.Locks.MediaTree);

                var mediaType = GetMediaType(mediaTypeAlias); // + locks
                if (mediaType == null)
                    throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias)); // causes rollback

                var media = new Models.Media(name, parent, mediaType);
                CreateMedia(scope, media, parent, userId, true);

                scope.Complete();
                return media;
            }
        }

        private void CreateMedia(IScope scope, Models.Media media, IMedia parent, int userId, bool withIdentity)
        {
            media.CreatorId = userId;

            if (withIdentity)
            {
                // if saving is cancelled, media remains without an identity
                var saveEventArgs = new SaveEventArgs<IMedia>(media);
                if (Saving.IsRaisedEventCancelled(saveEventArgs, this))
                    return;

                _mediaRepository.Save(media);

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs);
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
                scope.ReadLock(Constants.Locks.MediaTree);
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
                scope.ReadLock(Constants.Locks.MediaTree);
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
                scope.ReadLock(Constants.Locks.MediaTree);
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
                scope.ReadLock(Constants.Locks.MediaTree);
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
                scope.ReadLock(Constants.Locks.ContentTree);
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
                scope.ReadLock(Constants.Locks.ContentTree);
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
                scope.ReadLock(Constants.Locks.MediaTree);
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
                scope.ReadLock(Constants.Locks.MediaTree);
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
                scope.ReadLock(Constants.Locks.MediaTree);
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

            var rootId = Constants.System.RootString;
            var ids = media.Path.Split(Constants.CharArrays.Comma)
                .Where(x => x != rootId && x != media.Id.ToString(CultureInfo.InvariantCulture))
                .Select(int.Parse)
                .ToArray();
            if (ids.Any() == false)
                return new List<IMedia>();

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MediaTree);
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
                scope.ReadLock(Constants.Locks.MediaTree);

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
                scope.ReadLock(Constants.Locks.MediaTree);

                //if the id is System Root, then just get all
                if (id != Constants.System.Root)
                {
                    var mediaPath = _entityRepository.GetAllPaths(Constants.ObjectTypes.Media, id).ToArray();
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MediaTree);
                var query = Query<IMedia>().Where(x => x.ParentId == Constants.System.Root);
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

                scope.ReadLock(Constants.Locks.MediaTree);
                var query = Query<IMedia>().Where(x => x.Path.StartsWith(Constants.System.RecycleBinMediaPathPrefix));
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
        public Attempt<OperationResult> Save(IMedia media, int userId = Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IMedia>(media, evtMsgs);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
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

                scope.WriteLock(Constants.Locks.MediaTree);
                if (media.HasIdentity == false)
                    media.CreatorId = userId;

                _mediaRepository.Save(media);
                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Saved, this, saveEventArgs);
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
        public Attempt<OperationResult> Save(IEnumerable<IMedia> medias, int userId = Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var mediasA = medias.ToArray();

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IMedia>(mediasA, evtMsgs);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, new SaveEventArgs<IMedia>(mediasA, evtMsgs)))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                var treeChanges = mediasA.Select(x => new TreeChange<IMedia>(x, TreeChangeTypes.RefreshNode));

                scope.WriteLock(Constants.Locks.MediaTree);
                foreach (var media in mediasA)
                {
                    if (media.HasIdentity == false)
                        media.CreatorId = userId;
                    _mediaRepository.Save(media);
                }

                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Saved, this, saveEventArgs);
                }
                scope.Events.Dispatch(TreeChanged, this, treeChanges.ToEventArgs());
                Audit(AuditType.Save, userId == -1 ? 0 : userId, Constants.System.Root, "Bulk save media");

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
        public Attempt<OperationResult> Delete(IMedia media, int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            using (var scope = ScopeProvider.CreateScope())
            {
                if (scope.Events.DispatchCancelable(Deleting, this, new DeleteEventArgs<IMedia>(media, evtMsgs)))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                scope.WriteLock(Constants.Locks.MediaTree);

                DeleteLocked(scope, media);

                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>(media, TreeChangeTypes.Remove).ToEventArgs());
                Audit(AuditType.Delete, userId, media.Id);

                scope.Complete();
            }

            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        private void DeleteLocked(IScope scope, IMedia media)
        {
            void DoDelete(IMedia c)
            {
                _mediaRepository.Delete(c);
                var args = new DeleteEventArgs<IMedia>(c, false); // raise event & get flagged files
                scope.Events.Dispatch(Deleted, this, args);

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
        public void DeleteVersions(int id, DateTime versionDate, int userId = Constants.Security.SuperUserId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                DeleteVersions(scope, true, id, versionDate, userId);
                scope.Complete();

                //if (uow.Events.DispatchCancelable(DeletingVersions, this, new DeleteRevisionsEventArgs(id, dateToRetain: versionDate)))
                //{
                //    uow.Complete();
                //    return;
                //}

                //uow.WriteLock(Constants.Locks.MediaTree);
                //var repository = uow.CreateRepository<IMediaRepository>();
                //repository.DeleteVersions(id, versionDate);

                //uow.Events.Dispatch(DeletedVersions, this, new DeleteRevisionsEventArgs(id, false, dateToRetain: versionDate));
                //Audit(uow, AuditType.Delete, "Delete Media by version date, userId, Constants.System.Root);

                //uow.Complete();
            }
        }

        private void DeleteVersions(IScope scope, bool wlock, int id, DateTime versionDate, int userId = Constants.Security.SuperUserId)
        {
            var args = new DeleteRevisionsEventArgs(id, dateToRetain: versionDate);
            if (scope.Events.DispatchCancelable(DeletingVersions, this, args))
                return;

            if (wlock)
                scope.WriteLock(Constants.Locks.MediaTree);
            _mediaRepository.DeleteVersions(id, versionDate);

            args.CanCancel = false;
            scope.Events.Dispatch(DeletedVersions, this, args);
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
            using (var scope = ScopeProvider.CreateScope())
            {
                var args = new DeleteRevisionsEventArgs(id, /*specificVersion:*/ versionId);
                if (scope.Events.DispatchCancelable(DeletingVersions, this, args))
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
                    scope.WriteLock(Constants.Locks.MediaTree);
                }

                _mediaRepository.DeleteVersion(versionId);

                args.CanCancel = false;
                scope.Events.Dispatch(DeletedVersions, this, args);
                Audit(AuditType.Delete, userId, Constants.System.Root, "Delete Media by version");

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
        public Attempt<OperationResult> MoveToRecycleBin(IMedia media, int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var moves = new List<(IMedia, string)>();

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.MediaTree);

                // TODO: missing 7.6 "ensure valid path" thing here?
                // but then should be in PerformMoveLocked on every moved item?

                var originalPath = media.Path;

                var moveEventInfo = new MoveEventInfo<IMedia>(media, originalPath, Constants.System.RecycleBinMedia);
                var moveEventArgs = new MoveEventArgs<IMedia>(true, evtMsgs, moveEventInfo);
                if (scope.Events.DispatchCancelable(Trashing, this, moveEventArgs, nameof(Trashing)))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                PerformMoveLocked(media, Constants.System.RecycleBinMedia, null, userId, moves, true);

                scope.Events.Dispatch(TreeChanged, this, new TreeChange<IMedia>(media, TreeChangeTypes.RefreshBranch).ToEventArgs());
                var moveInfo = moves.Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId))
                    .ToArray();
                moveEventArgs.MoveInfoCollection = moveInfo;
                moveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Trashed, this, moveEventArgs, nameof(Trashed));
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
        public Attempt<OperationResult> Move(IMedia media, int parentId, int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            // if moving to the recycle bin then use the proper method
            if (parentId == Constants.System.RecycleBinMedia)
            {
                MoveToRecycleBin(media, userId);
                return OperationResult.Attempt.Succeed(evtMsgs);
            }

            var moves = new List<(IMedia, string)>();

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.MediaTree);

                var parent = parentId == Constants.System.Root ? null : GetById(parentId);
                if (parentId != Constants.System.Root && (parent == null || parent.Trashed))
                    throw new InvalidOperationException("Parent does not exist or is trashed."); // causes rollback

                var moveEventInfo = new MoveEventInfo<IMedia>(media, media.Path, parentId);
                var moveEventArgs = new MoveEventArgs<IMedia>(true, evtMsgs, moveEventInfo);
                if (scope.Events.DispatchCancelable(Moving, this, moveEventArgs, nameof(Moving)))
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
                moveEventArgs.MoveInfoCollection = moveInfo;
                moveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Moved, this, moveEventArgs, nameof(Moved));
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
            paths[media.Id] = (parent == null ? (parentId == Constants.System.RecycleBinMedia ? "-1,-21" : Constants.System.RootString) : parent.Path) + "," + media.Id;

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
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use EmptyRecycleBin with explicit indication of user ID instead")]
        public OperationResult EmptyRecycleBin() => EmptyRecycleBin(Constants.Security.SuperUserId);

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IMedia"/> that resides in the bin
        /// </summary>
        /// <param name="userId">Optional Id of the User emptying the Recycle Bin</param>
        public OperationResult EmptyRecycleBin(int userId = Constants.Security.SuperUserId)
        {
            var nodeObjectType = Constants.ObjectTypes.Media;
            var deleted = new List<IMedia>();
            var evtMsgs = EventMessagesFactory.Get(); // TODO: and then?

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.MediaTree);

                // no idea what those events are for, keep a simplified version

                // v7 EmptyingRecycleBin and EmptiedRecycleBin events are greatly simplified since
                // each deleted items will have its own deleting/deleted events. so, files and such
                // are managed by Delete, and not here.
                var args = new RecycleBinEventArgs(nodeObjectType, evtMsgs);

                if (scope.Events.DispatchCancelable(EmptyingRecycleBin, this, args))
                {
                    scope.Complete();
                    return OperationResult.Cancel(evtMsgs);
                }
                // emptying the recycle bin means deleting whatever is in there - do it properly!
                var query = Query<IMedia>().Where(x => x.ParentId == Constants.System.RecycleBinMedia);
                var medias = _mediaRepository.Get(query).ToArray();
                foreach (var media in medias)
                {
                    DeleteLocked(scope, media);
                    deleted.Add(media);
                }
                args.CanCancel = false;
                scope.Events.Dispatch(EmptiedRecycleBin, this, args);
                scope.Events.Dispatch(TreeChanged, this, deleted.Select(x => new TreeChange<IMedia>(x, TreeChangeTypes.Remove)).ToEventArgs());
                Audit(AuditType.Delete, userId, Constants.System.RecycleBinMedia, "Empty Media recycle bin");
                scope.Complete();
            }

            return OperationResult.Succeed(evtMsgs);
        }

        public bool RecycleBinSmells()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MediaTree);
                return _mediaRepository.RecycleBinSmells();
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
        public bool Sort(IEnumerable<IMedia> items, int userId = Constants.Security.SuperUserId, bool raiseEvents = true)
        {
            var itemsA = items.ToArray();
            if (itemsA.Length == 0) return true;

            using (var scope = ScopeProvider.CreateScope())
            {
                var args = new SaveEventArgs<IMedia>(itemsA);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, args))
                {
                    scope.Complete();
                    return false;
                }

                var saved = new List<IMedia>();

                scope.WriteLock(Constants.Locks.MediaTree);
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
                    args.CanCancel = false;
                    scope.Events.Dispatch(Saved, this, args);
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
                scope.WriteLock(Constants.Locks.MediaTree);

                var report = _mediaRepository.CheckDataIntegrity(options);

                if (report.FixedIssues.Count > 0)
                {
                    //The event args needs a content item so we'll make a fake one with enough properties to not cause a null ref
                    var root = new Models.Media("root", -1, new MediaType(-1)) { Id = -1, Key = Guid.Empty };
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
        public static event TypedEventHandler<IMediaService, TreeChange<IMedia>.EventArgs> TreeChanged;

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

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.MediaTree);

                var query = Query<IMedia>().WhereIn(x => x.ContentTypeId, mediaTypeIdsA);
                var medias = _mediaRepository.Get(query).ToArray();

                if (scope.Events.DispatchCancelable(Deleting, this, new DeleteEventArgs<IMedia>(medias)))
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
                        PerformMoveLocked(child, Constants.System.RecycleBinMedia, null, userId, moves, true);
                        changes.Add(new TreeChange<IMedia>(media, TreeChangeTypes.RefreshBranch));
                    }

                    // delete media
                    // triggers the deleted event (and handles the files)
                    DeleteLocked(scope, media);
                    changes.Add(new TreeChange<IMedia>(media, TreeChangeTypes.Remove));
                }

                var moveInfos = moves.Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentId))
                    .ToArray();
                if (moveInfos.Length > 0)
                    scope.Events.Dispatch(Trashed, this, new MoveEventArgs<IMedia>(false, moveInfos), nameof(Trashed));
                scope.Events.Dispatch(TreeChanged, this, changes.ToEventArgs());

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
            if (mediaTypeAlias == null) throw new ArgumentNullException(nameof(mediaTypeAlias));
            if (string.IsNullOrWhiteSpace(mediaTypeAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(mediaTypeAlias));

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.MediaTypes);

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
