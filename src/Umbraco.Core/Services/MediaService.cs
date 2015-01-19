using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Umbraco.Core.Auditing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Media Service, which is an easy access to operations involving <see cref="IMedia"/>
    /// </summary>
    public class MediaService : RepositoryService, IMediaService
    {

        //Support recursive locks because some of the methods that require locking call other methods that require locking. 
        //for example, the Move method needs to be locked but this calls the Save method which also needs to be locked.
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private readonly EntityXmlSerializer _entitySerializer = new EntityXmlSerializer();
        private readonly IDataTypeService _dataTypeService;
        private readonly IUserService _userService;

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public MediaService(RepositoryFactory repositoryFactory)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {
        }

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public MediaService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
            : base(provider, repositoryFactory, LoggerResolver.Current.Logger)
        {
            _dataTypeService = new DataTypeService(provider, repositoryFactory);
            _userService = new UserService(provider, repositoryFactory);
        }

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public MediaService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, IDataTypeService dataTypeService, IUserService userService)
            : this(provider, repositoryFactory, LoggerResolver.Current.Logger, dataTypeService, userService)
        {
        }

        public MediaService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IDataTypeService dataTypeService, IUserService userService)
            : base(provider, repositoryFactory, logger)
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
        public IMedia CreateMedia(string name, int parentId, string mediaTypeAlias, int userId = 0)
        {
            var mediaType = FindMediaTypeByAlias(mediaTypeAlias);
            var media = new Models.Media(name, parentId, mediaType);

            if (Creating.IsRaisedEventCancelled(new NewEventArgs<IMedia>(media, mediaTypeAlias, parentId), this))
            {
                media.WasCancelled = true;
                return media;
            }

            media.CreatorId = userId;

            Created.RaiseEvent(new NewEventArgs<IMedia>(media, false, mediaTypeAlias, parentId), this);

            Audit(AuditType.New, string.Format("Media '{0}' was created", name), media.CreatorId, media.Id);

            return media;
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
            var mediaType = FindMediaTypeByAlias(mediaTypeAlias);
            var media = new Models.Media(name, parent, mediaType);
            if (Creating.IsRaisedEventCancelled(new NewEventArgs<IMedia>(media, mediaTypeAlias, parent), this))
            {
                media.WasCancelled = true;
                return media;
            }

            media.CreatorId = userId;

            Created.RaiseEvent(new NewEventArgs<IMedia>(media, false, mediaTypeAlias, parent), this);

            Audit(AuditType.New, string.Format("Media '{0}' was created", name), media.CreatorId, media.Id);

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
        /// <param name="parentId">Id of Parent for the new Media item</param>
        /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user creating the media item</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia CreateMediaWithIdentity(string name, int parentId, string mediaTypeAlias, int userId = 0)
        {
            var mediaType = FindMediaTypeByAlias(mediaTypeAlias);
            var media = new Models.Media(name, parentId, mediaType);

            //NOTE: I really hate the notion of these Creating/Created events - they are so inconsistent, I've only just found
            // out that in these 'WithIdentity' methods, the Saving/Saved events were not fired, wtf. Anyways, they're added now.
            if (Creating.IsRaisedEventCancelled(new NewEventArgs<IMedia>(media, mediaTypeAlias, parentId), this))
            {
                media.WasCancelled = true;
                return media;
            }

            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMedia>(media), this))
            {
                media.WasCancelled = true;
                return media;
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                media.CreatorId = userId;
                repository.AddOrUpdate(media);

                repository.AddOrUpdateContentXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                // generate preview for blame history?
                if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                {
                    repository.AddOrUpdatePreviewXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                }

                uow.Commit();
            }

            Saved.RaiseEvent(new SaveEventArgs<IMedia>(media, false), this);

            Created.RaiseEvent(new NewEventArgs<IMedia>(media, false, mediaTypeAlias, parentId), this);

            Audit(AuditType.New, string.Format("Media '{0}' was created with Id {1}", name, media.Id), media.CreatorId, media.Id);

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
            var mediaType = FindMediaTypeByAlias(mediaTypeAlias);
            var media = new Models.Media(name, parent, mediaType);

            //NOTE: I really hate the notion of these Creating/Created events - they are so inconsistent, I've only just found
            // out that in these 'WithIdentity' methods, the Saving/Saved events were not fired, wtf. Anyways, they're added now.
            if (Creating.IsRaisedEventCancelled(new NewEventArgs<IMedia>(media, mediaTypeAlias, parent), this))
            {
                media.WasCancelled = true;
                return media;
            }

            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMedia>(media), this))
            {
                media.WasCancelled = true;
                return media;
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                media.CreatorId = userId;
                repository.AddOrUpdate(media);
                repository.AddOrUpdateContentXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                // generate preview for blame history?
                if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                {
                    repository.AddOrUpdatePreviewXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                }

                uow.Commit();
            }

            Saved.RaiseEvent(new SaveEventArgs<IMedia>(media, false), this);

            Created.RaiseEvent(new NewEventArgs<IMedia>(media, false, mediaTypeAlias, parent), this);

            Audit(AuditType.New, string.Format("Media '{0}' was created with Id {1}", name, media.Id), media.CreatorId, media.Id);

            return media;
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by Id
        /// </summary>
        /// <param name="id">Id of the Content to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetById(int id)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                return repository.Get(id);
            }
        }

        public int Count(string contentTypeAlias = null)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                return repository.Count(contentTypeAlias);
            }
        }

        public int CountChildren(int parentId, string contentTypeAlias = null)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                return repository.CountChildren(parentId, contentTypeAlias);
            }
        }

        public int CountDescendants(int parentId, string contentTypeAlias = null)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                return repository.CountDescendants(parentId, contentTypeAlias);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by Id
        /// </summary>
        /// <param name="ids">Ids of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IEnumerable<IMedia> GetByIds(IEnumerable<int> ids)
        {
            using (var repository = RepositoryFactory.CreateMediaRepository(UowProvider.GetUnitOfWork()))
            {
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
            using (var repository = RepositoryFactory.CreateMediaRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IMedia>.Builder.Where(x => x.Key == key);
                var contents = repository.GetByQuery(query);
                return contents.SingleOrDefault();
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Level
        /// </summary>
        /// <param name="level">The level to retrieve Media from</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetByLevel(int level)
        {
            using (var repository = RepositoryFactory.CreateMediaRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IMedia>.Builder.Where(x => x.Level == level && !x.Path.StartsWith("-21"));
                var contents = repository.GetByQuery(query);

                return contents;
            }
        }

        /// <summary>
        /// Gets a specific version of an <see cref="IMedia"/> item.
        /// </summary>
        /// <param name="versionId">Id of the version to retrieve</param>
        /// <returns>An <see cref="IMedia"/> item</returns>
        public IMedia GetByVersion(Guid versionId)
        {
            using (var repository = RepositoryFactory.CreateMediaRepository(UowProvider.GetUnitOfWork()))
            {
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
            using (var repository = RepositoryFactory.CreateMediaRepository(UowProvider.GetUnitOfWork()))
            {
                var versions = repository.GetAllVersions(id);
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
            var ids = media.Path.Split(',').Where(x => x != "-1" && x != media.Id.ToString(CultureInfo.InvariantCulture)).Select(int.Parse).ToArray();
            if (ids.Any() == false)
                return new List<IMedia>();

            using (var repository = RepositoryFactory.CreateMediaRepository(UowProvider.GetUnitOfWork()))
            {
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
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                var query = Query<IMedia>.Builder.Where(x => x.ParentId == id);
                var medias = repository.GetByQuery(query);

                return medias;
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
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IMedia> GetPagedChildren(int id, int pageIndex, int pageSize, out int totalChildren,
            string orderBy, Direction orderDirection, string filter = "")
        {
            Mandate.ParameterCondition(pageIndex >= 0, "pageSize");
            Mandate.ParameterCondition(pageSize > 0, "pageSize");
            using (var repository = RepositoryFactory.CreateMediaRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IMedia>.Builder;
                //if the id is -1, then just get all
                if (id != -1)
                {
                    query.Where(x => x.ParentId == id);
                }
                var medias = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, filter);

                return medias;
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
        public IEnumerable<IMedia> GetPagedDescendants(int id, int pageIndex, int pageSize, out int totalChildren, string orderBy = "Path", Direction orderDirection = Direction.Ascending, string filter = "")
        {
            Mandate.ParameterCondition(pageIndex >= 0, "pageSize");
            Mandate.ParameterCondition(pageSize > 0, "pageSize");
            using (var repository = RepositoryFactory.CreateMediaRepository(UowProvider.GetUnitOfWork()))
            {

                var query = Query<IMedia>.Builder;
                //if the id is -1, then just get all
                if (id != -1)
                {
                    query.Where(x => x.Path.SqlContains(string.Format(",{0},", id), TextColumnType.NVarchar));
                }
                var contents = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalChildren, orderBy, orderDirection, filter);

                return contents;
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
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                var pathMatch = media.Path + ",";
                var query = Query<IMedia>.Builder.Where(x => x.Path.StartsWith(pathMatch) && x.Id != media.Id);
                var medias = repository.GetByQuery(query);

                return medias;
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
            if (media.ParentId == -1 || media.ParentId == -21)
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
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                var query = Query<IMedia>.Builder.Where(x => x.ContentTypeId == id);
                var medias = repository.GetByQuery(query);

                return medias;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetRootMedia()
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                var query = Query<IMedia>.Builder.Where(x => x.ParentId == -1);
                var medias = repository.GetByQuery(query);

                return medias;
            }
        }

        /// <summary>
        /// Gets a collection of an <see cref="IMedia"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetMediaInRecycleBin()
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                var query = Query<IMedia>.Builder.Where(x => x.Path.Contains("-21"));
                var medias = repository.GetByQuery(query);

                return medias;
            }
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object from the path stored in the 'umbracoFile' property.
        /// </summary>
        /// <param name="mediaPath">Path of the media item to retrieve (for example: /media/1024/koala_403x328.jpg)</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetMediaByPath(string mediaPath)
        {
            var umbracoFileValue = mediaPath;
            const string Pattern = ".*[_][0-9]+[x][0-9]+[.].*";
            var isResized = Regex.IsMatch(mediaPath, Pattern);

            // If the image has been resized we strip the "_403x328" of the original "/media/1024/koala_403x328.jpg" url.
            if (isResized)
            {
                var underscoreIndex = mediaPath.LastIndexOf('_');
                var dotIndex = mediaPath.LastIndexOf('.');
                umbracoFileValue = string.Concat(mediaPath.Substring(0, underscoreIndex), mediaPath.Substring(dotIndex));
            }

            Func<string, Sql> createSql = url => new Sql().Select("*")
                                                  .From<PropertyDataDto>()
                                                  .InnerJoin<PropertyTypeDto>()
                                                  .On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                                                  .Where<PropertyTypeDto>(x => x.Alias == "umbracoFile")
                                                  .Where<PropertyDataDto>(x => x.VarChar == url);

            var sql = createSql(umbracoFileValue);

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var propertyDataDto = uow.Database.Fetch<PropertyDataDto, PropertyTypeDto>(sql).FirstOrDefault();

                // If the stripped-down url returns null, we try again with the original url. 
                // Previously, the function would fail on e.g. "my_x_image.jpg"
                if (propertyDataDto == null)
                {
                    sql = createSql(mediaPath);
                    propertyDataDto = uow.Database.Fetch<PropertyDataDto, PropertyTypeDto>(sql).FirstOrDefault();
                }

                return propertyDataDto == null ? null : GetById(propertyDataDto.NodeId);
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IMedia"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/></param>
        /// <returns>True if the media has any children otherwise False</returns>
        public bool HasChildren(int id)
        {
            using (var repository = RepositoryFactory.CreateMediaRepository(UowProvider.GetUnitOfWork()))
            {
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
        public void Move(IMedia media, int parentId, int userId = 0)
        {
            //TODO: This all needs to be on the repo layer in one transaction!

            if (media == null) throw new ArgumentNullException("media");

            using (new WriteLock(Locker))
            {
                //This ensures that the correct method is called if this method is used to Move to recycle bin.
                if (parentId == -21)
                {
                    MoveToRecycleBin(media, userId);
                    return;
                }

                var originalPath = media.Path;

                if (Moving.IsRaisedEventCancelled(
                    new MoveEventArgs<IMedia>(
                        new MoveEventInfo<IMedia>(media, originalPath, parentId)), this))
                {
                    return;
                }

                media.ParentId = parentId;
                if (media.Trashed)
                {
                    media.ChangeTrashedState(false, parentId);
                }
                Save(media, userId,
                    //no events!
                    false);

                //used to track all the moved entities to be given to the event
                var moveInfo = new List<MoveEventInfo<IMedia>>
                {
                    new MoveEventInfo<IMedia>(media, originalPath, parentId)
                };

                //Ensure that relevant properties are updated on children
                var children = GetChildren(media.Id).ToArray();
                if (children.Any())
                {
                    var parentPath = media.Path;
                    var parentLevel = media.Level;
                    var parentTrashed = media.Trashed;
                    var updatedDescendants = UpdatePropertiesOnChildren(children, parentPath, parentLevel, parentTrashed, moveInfo);
                    Save(updatedDescendants, userId,
                        //no events!
                        false);
                }

                Moved.RaiseEvent(new MoveEventArgs<IMedia>(false, moveInfo.ToArray()), this);

                Audit(AuditType.Move, "Move Media performed by user", userId, media.Id);
            }
        }

        /// <summary>
        /// Deletes an <see cref="IMedia"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        public void MoveToRecycleBin(IMedia media, int userId = 0)
        {
            if (media == null) throw new ArgumentNullException("media");

            var originalPath = media.Path;

            if (Trashing.IsRaisedEventCancelled(
                    new MoveEventArgs<IMedia>(
                        new MoveEventInfo<IMedia>(media, originalPath, Constants.System.RecycleBinMedia)), this))
            {
                return;
            }

            var moveInfo = new List<MoveEventInfo<IMedia>>
            {
                new MoveEventInfo<IMedia>(media, originalPath, Constants.System.RecycleBinMedia)
            };

            //Find Descendants, which will be moved to the recycle bin along with the parent/grandparent.
            var descendants = GetDescendants(media).OrderBy(x => x.Level).ToList();

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                //TODO: This should be part of the repo!

                //Remove 'published' xml from the cmsContentXml table for the unpublished media
                uow.Database.Delete<ContentXmlDto>("WHERE nodeId = @Id", new { Id = media.Id });

                media.ChangeTrashedState(true, Constants.System.RecycleBinMedia);
                repository.AddOrUpdate(media);

                //Loop through descendants to update their trash state, but ensuring structure by keeping the ParentId
                foreach (var descendant in descendants)
                {
                    //Remove 'published' xml from the cmsContentXml table for the unpublished media
                    uow.Database.Delete<ContentXmlDto>("WHERE nodeId = @Id", new { Id = descendant.Id });

                    descendant.ChangeTrashedState(true, descendant.ParentId);
                    repository.AddOrUpdate(descendant);

                    moveInfo.Add(new MoveEventInfo<IMedia>(descendant, descendant.Path, descendant.ParentId));
                }

                uow.Commit();
            }

            Trashed.RaiseEvent(new MoveEventArgs<IMedia>(false, moveInfo.ToArray()), this);

            Audit(AuditType.Move, "Move Media to Recycle Bin performed by user", userId, media.Id);
        }

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IMedia"/> that resides in the bin
        /// </summary>
        public void EmptyRecycleBin()
        {
            using (new WriteLock(Locker))
            {
                Dictionary<int, IEnumerable<Property>> entities;
                List<string> files;
                bool success;
                var nodeObjectType = new Guid(Constants.ObjectTypes.Media);

                var uow = UowProvider.GetUnitOfWork();
                using (var repository = RepositoryFactory.CreateMediaRepository(uow))
                {
                    //Create a dictionary of ids -> dictionary of property aliases + values
                    entities = repository.GetEntitiesInRecycleBin()
                        .ToDictionary(
                            key => key.Id,
                            val => (IEnumerable<Property>)val.Properties);

                    files = ((MediaRepository)repository).GetFilesInRecycleBinForUploadField();

                    if (EmptyingRecycleBin.IsRaisedEventCancelled(new RecycleBinEventArgs(nodeObjectType, entities, files), this))
                        return;

                    success = repository.EmptyRecycleBin();

                    EmptiedRecycleBin.RaiseEvent(new RecycleBinEventArgs(nodeObjectType, entities, files, success), this);

                    if (success)
                        repository.DeleteFiles(files);
                }
            }
            Audit(AuditType.Delete, "Empty Media Recycle Bin performed by user", 0, -21);
        }

        /// <summary>
        /// Deletes all media of specified type. All children of deleted media is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="mediaTypeId">Id of the <see cref="IMediaType"/></param>
        /// <param name="userId">Optional id of the user deleting the media</param>
        public void DeleteMediaOfType(int mediaTypeId, int userId = 0)
        {
            //TODO: This all needs to be done on the repo level in one trans

            using (new WriteLock(Locker))
            {
                var uow = UowProvider.GetUnitOfWork();
                using (var repository = RepositoryFactory.CreateMediaRepository(uow))
                {
                    //NOTE What about media that has the contenttype as part of its composition?
                    //The ContentType has to be removed from the composition somehow as it would otherwise break
                    //Dbl.check+test that the ContentType's Id is removed from the ContentType2ContentType table
                    var query = Query<IMedia>.Builder.Where(x => x.ContentTypeId == mediaTypeId);
                    var contents = repository.GetByQuery(query).ToArray();

                    if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMedia>(contents), this))
                        return;

                    foreach (var content in contents.OrderByDescending(x => x.ParentId))
                    {
                        //Look for children of current content and move that to trash before the current content is deleted
                        var c = content;
                        var childQuery = Query<IMedia>.Builder.Where(x => x.Path.StartsWith(c.Path));
                        var children = repository.GetByQuery(childQuery);

                        foreach (var child in children)
                        {
                            if (child.ContentType.Id != mediaTypeId)
                                MoveToRecycleBin(child, userId);
                        }

                        //Permanently delete the content
                        Delete(content, userId);
                    }
                }

                Audit(AuditType.Delete, "Delete Media items by Type performed by user", userId, -1);
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
            //TODO: IT would be much nicer to mass delete all in one trans in the repo level!

            if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMedia>(media), this))
                return;

            //Delete children before deleting the 'possible parent'
            var children = GetChildren(media.Id);
            foreach (var child in children)
            {
                Delete(child, userId);
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                repository.Delete(media);
                uow.Commit();

                var args = new DeleteEventArgs<IMedia>(media, false);
                Deleted.RaiseEvent(args, this);

                //remove any flagged media files
                repository.DeleteFiles(args.MediaFilesToDelete);
            }

            Audit(AuditType.Delete, "Delete Media performed by user", userId, media.Id);
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
            if (DeletingVersions.IsRaisedEventCancelled(new DeleteRevisionsEventArgs(id, dateToRetain: versionDate), this))
                return;

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                repository.DeleteVersions(id, versionDate);
                uow.Commit();
            }

            DeletedVersions.RaiseEvent(new DeleteRevisionsEventArgs(id, false, dateToRetain: versionDate), this);

            Audit(AuditType.Delete, "Delete Media by version date performed by user", userId, -1);
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
            if (DeletingVersions.IsRaisedEventCancelled(new DeleteRevisionsEventArgs(id, specificVersion: versionId), this))
                return;

            if (deletePriorVersions)
            {
                var content = GetByVersion(versionId);
                DeleteVersions(id, content.UpdateDate, userId);
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                repository.DeleteVersion(versionId);
                uow.Commit();
            }

            DeletedVersions.RaiseEvent(new DeleteRevisionsEventArgs(id, false, specificVersion: versionId), this);

            Audit(AuditType.Delete, "Delete Media by version performed by user", userId, -1);
        }

        /// <summary>
        /// Saves a single <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public void Save(IMedia media, int userId = 0, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMedia>(media), this))
                    return;
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                media.CreatorId = userId;
                repository.AddOrUpdate(media);
                repository.AddOrUpdateContentXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                // generate preview for blame history?
                if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                {
                    repository.AddOrUpdatePreviewXml(media, m => _entitySerializer.Serialize(this, _dataTypeService, _userService, m));
                }

                uow.Commit();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IMedia>(media, false), this);

            Audit(AuditType.Save, "Save Media performed by user", userId, media.Id);
        }

        /// <summary>
        /// Saves a collection of <see cref="IMedia"/> objects
        /// </summary>
        /// <param name="medias">Collection of <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        public void Save(IEnumerable<IMedia> medias, int userId = 0, bool raiseEvents = true)
        {
            var asArray = medias.ToArray();

            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMedia>(asArray), this))
                    return;
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
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

                //commit the whole lot in one go
                uow.Commit();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IMedia>(asArray, false), this);

            Audit(AuditType.Save, "Save Media items performed by user", userId, -1);
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

            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMedia>(asArray), this))
                    return false;
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
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

                uow.Commit();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IMedia>(asArray, false), this);

            Audit(AuditType.Sort, "Sorting Media performed by user", userId, 0);

            return true;
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
            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaRepository(uow))
            {
                repository.RebuildXmlStructures(
                    media => _entitySerializer.Serialize(this, _dataTypeService, _userService, media),
                    contentTypeIds: contentTypeIds.Length == 0 ? null : contentTypeIds);
            }

            Audit(AuditType.Publish, "MediaService.RebuildXmlStructures completed, the xml has been regenerated in the database", 0, -1);
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
        //    var poco = new ContentXmlDto { NodeId = id, Xml = xml.ToString(SaveOptions.None) };
        //    var exists = db.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = id }) != null;
        //    int result = exists ? db.Update(poco) : Convert.ToInt32(db.Insert(poco));
        //}

        private IMediaType FindMediaTypeByAlias(string mediaTypeAlias)
        {
            Mandate.ParameterNotNullOrEmpty(mediaTypeAlias, "mediaTypeAlias");

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMediaTypeRepository(uow))
            {
                var query = Query<IMediaType>.Builder.Where(x => x.Alias == mediaTypeAlias);
                var mediaTypes = repository.GetByQuery(query);

                if (mediaTypes.Any() == false)
                    throw new Exception(string.Format("No MediaType matching the passed in Alias: '{0}' was found",
                                                      mediaTypeAlias));

                var mediaType = mediaTypes.First();

                if (mediaType == null)
                    throw new Exception(string.Format("MediaType matching the passed in Alias: '{0}' was null",
                                                      mediaTypeAlias));

                return mediaType;
            }
        }

        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var auditRepo = RepositoryFactory.CreateAuditRepository(uow))
            {
                auditRepo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
                uow.Commit();
            }
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
