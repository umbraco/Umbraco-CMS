using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Auditing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
	/// <summary>
	/// Represents the Media Service, which is an easy access to operations involving <see cref="IMedia"/>
	/// </summary>
	public class MediaService : IMediaService
	{
		private readonly IDatabaseUnitOfWorkProvider _uowProvider;
		private readonly RepositoryFactory _repositoryFactory;
		private readonly IUserService _userService;
		private HttpContextBase _httpContext;

		public MediaService(RepositoryFactory repositoryFactory)
			: this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
		{
		}

		public MediaService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
		{
			_uowProvider = provider;
			_repositoryFactory = repositoryFactory;
		}

		internal MediaService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, IUserService userService)
		{
			_uowProvider = provider;
			_repositoryFactory = repositoryFactory;
			_userService = userService;
		}

	    /// <summary>
	    /// Creates an <see cref="IMedia"/> object using the alias of the <see cref="IMediaType"/>
	    /// that this Media is based on.
	    /// </summary>
	    /// <param name="parentId">Id of Parent for the new Media item</param>
	    /// <param name="mediaTypeAlias">Alias of the <see cref="IMediaType"/></param>
	    /// <param name="userId">Optional id of the user creating the media item</param>
	    /// <returns><see cref="IMedia"/></returns>
	    public IMedia CreateMedia(int parentId, string mediaTypeAlias, int userId = -1)
	    {
	        IMediaType mediaType = null;
	        var uow = _uowProvider.GetUnitOfWork();
	        using (var repository = _repositoryFactory.CreateMediaTypeRepository(uow))
	        {
	            var query = Query<IMediaType>.Builder.Where(x => x.Alias == mediaTypeAlias);
	            var mediaTypes = repository.GetByQuery(query);

	            if (!mediaTypes.Any())
	                throw new Exception(string.Format("No ContentType matching the passed in Alias: '{0}' was found",
	                                                  mediaTypeAlias));

	            mediaType = mediaTypes.First();

	            if (mediaType == null)
	                throw new Exception(string.Format("ContentType matching the passed in Alias: '{0}' was null",
	                                                  mediaTypeAlias));
	        }

	        var media = new Models.Media(parentId, mediaType);

			if (Creating.IsRaisedEventCancelled(new NewEventArgs<IMedia>(media, mediaTypeAlias, parentId), this))
				return media;

			SetUser(media, userId);

			Created.RaiseEvent(new NewEventArgs<IMedia>(media, false, mediaTypeAlias, parentId), this);			

			Audit.Add(AuditTypes.New, "", media.CreatorId, media.Id);

	        return media;
	    }

	    /// <summary>
		/// Gets an <see cref="IMedia"/> object by Id
		/// </summary>
		/// <param name="id">Id of the Content to retrieve</param>
		/// <returns><see cref="IMedia"/></returns>
		public IMedia GetById(int id)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
			{
				return repository.Get(id);	
			}
		}

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by its 'UniqueId'
        /// </summary>
        /// <param name="key">Guid key of the Media to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetById(Guid key)
        {
            using (var repository = _repositoryFactory.CreateMediaRepository(_uowProvider.GetUnitOfWork()))
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
            using (var repository = _repositoryFactory.CreateMediaRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMedia>.Builder.Where(x => x.Level == level);
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
            using (var repository = _repositoryFactory.CreateMediaRepository(_uowProvider.GetUnitOfWork()))
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
            using (var repository = _repositoryFactory.CreateMediaRepository(_uowProvider.GetUnitOfWork()))
            {
                var versions = repository.GetAllVersions(id);
                return versions;
            }
        }

		/// <summary>
		/// Gets a collection of <see cref="IMedia"/> objects by Parent Id
		/// </summary>
		/// <param name="id">Id of the Parent to retrieve Children from</param>
		/// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
		public IEnumerable<IMedia> GetChildren(int id)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
			{
				var query = Query<IMedia>.Builder.Where(x => x.ParentId == id);
				var medias = repository.GetByQuery(query);

				return medias;
			}
		}

		/// <summary>
		/// Gets descendants of a <see cref="IMedia"/> object by its Id
		/// </summary>
		/// <param name="id">Id of the Parent to retrieve descendants from</param>
		/// <returns>An Enumerable flat list of <see cref="IMedia"/> objects</returns>
		public IEnumerable<IMedia> GetDescendants(int id)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
			{
				var media = repository.Get(id);

				var query = Query<IMedia>.Builder.Where(x => x.Path.StartsWith(media.Path));
				var medias = repository.GetByQuery(query);

				return medias;
			}			
		}

		/// <summary>
		/// Gets a collection of <see cref="IMedia"/> objects by the Id of the <see cref="IContentType"/>
		/// </summary>
		/// <param name="id">Id of the <see cref="IMediaType"/></param>
		/// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
		public IEnumerable<IMedia> GetMediaOfMediaType(int id)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
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
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
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
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
			{
				var query = Query<IMedia>.Builder.Where(x => x.ParentId == -21);
				var medias = repository.GetByQuery(query);

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
            using (var repository = _repositoryFactory.CreateMediaRepository(_uowProvider.GetUnitOfWork()))
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
		public void Move(IMedia media, int parentId, int userId = -1)
		{
            //This ensures that the correct method is called if this method is used to Move to recycle bin.
            if (parentId == -21)
            {
                MoveToRecycleBin(media, userId);
                return;
            }

			if (Moving.IsRaisedEventCancelled(new MoveEventArgs<IMedia>(media, parentId), this))
				return;

			media.ParentId = parentId;
			Save(media, userId);

			Moved.RaiseEvent(new MoveEventArgs<IMedia>(media, false, parentId), this);

			Audit.Add(AuditTypes.Move, "Move Media performed by user", userId == -1 ? 0 : userId, media.Id);
		}

	    /// <summary>
	    /// Deletes an <see cref="IMedia"/> object by moving it to the Recycle Bin
	    /// </summary>
	    /// <param name="media">The <see cref="IMedia"/> to delete</param>
	    /// <param name="userId">Id of the User deleting the Media</param>
	    public void MoveToRecycleBin(IMedia media, int userId = -1)
	    {
	        if (Trashing.IsRaisedEventCancelled(new MoveEventArgs<IMedia>(media, -21), this))
				return;

            //Move children to Recycle Bin before the 'possible parent' is moved there
            var children = GetChildren(media.Id);
            foreach (var child in children)
            {
                MoveToRecycleBin(child, userId);
            }

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
			{
				media.ChangeTrashedState(true);
				repository.AddOrUpdate(media);
				uow.Commit();
			}

			Trashed.RaiseEvent(new MoveEventArgs<IMedia>(media, false, -21), this);

			Audit.Add(AuditTypes.Move, "Move Media to Recycle Bin performed by user", userId == -1 ? 0 : userId,
					  media.Id);
	    }

	    /// <summary>
		/// Empties the Recycle Bin by deleting all <see cref="IMedia"/> that resides in the bin
		/// </summary>
		public void EmptyRecycleBin()
		{
			//TODO: Why don't we have a base class to share between MediaService/ContentService as some of this is exacty the same?

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
			{
				var query = Query<IMedia>.Builder.Where(x => x.ParentId == -21);
				var contents = repository.GetByQuery(query);

				foreach (var content in contents)
				{
					if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMedia>(content), this))
						continue;

					repository.Delete(content);

					Deleted.RaiseEvent(new DeleteEventArgs<IMedia>(content, false), this);
				}
				uow.Commit();
			}

            Audit.Add(AuditTypes.Delete, "Empty Recycle Bin performed by user", 0, -20);
		}

	    /// <summary>
	    /// Deletes all media of specified type. All children of deleted media is moved to Recycle Bin.
	    /// </summary>
	    /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
	    /// <param name="mediaTypeId">Id of the <see cref="IMediaType"/></param>
	    /// <param name="userId">Optional id of the user deleting the media</param>
	    public void DeleteMediaOfType(int mediaTypeId, int userId = -1)
	    {			
	        var uow = _uowProvider.GetUnitOfWork();
	        using (var repository = _repositoryFactory.CreateMediaRepository(uow))
	        {
				//NOTE What about media that has the contenttype as part of its composition?
				//The ContentType has to be removed from the composition somehow as it would otherwise break
				//Dbl.check+test that the ContentType's Id is removed from the ContentType2ContentType table
				var query = Query<IMedia>.Builder.Where(x => x.ContentTypeId == mediaTypeId);
				var contents = repository.GetByQuery(query);

				if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMedia>(contents), this))
					return;

				foreach (var content in contents)
				{
					((Core.Models.Media)content).ChangeTrashedState(true);
					repository.AddOrUpdate(content);
				}

				uow.Commit();

				Deleted.RaiseEvent(new DeleteEventArgs<IMedia>(contents, false), this);
	        }			

			Audit.Add(AuditTypes.Delete, "Delete Media items by Type performed by user", userId == -1 ? 0 : userId, -1);
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
	    public void Delete(IMedia media, int userId = -1)
	    {
			if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMedia>(media), this))
				return;

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
			{
				repository.Delete(media);
				uow.Commit();
			}

			Deleted.RaiseEvent(new DeleteEventArgs<IMedia>(media, false), this);

			Audit.Add(AuditTypes.Delete, "Delete Media performed by user", userId == -1 ? 0 : userId, media.Id);
	    }

        /// <summary>
        /// Permanently deletes versions from an <see cref="IMedia"/> object prior to a specific date.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> object to delete versions from</param>
        /// <param name="versionDate">Latest version date</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        public void DeleteVersions(int id, DateTime versionDate, int userId = -1)
        {
			if (DeletingVersions.IsRaisedEventCancelled(new DeleteRevisionsEventArgs(id, dateToRetain: versionDate), this))
				return;
			
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
			{
				repository.DeleteVersions(id, versionDate);
				uow.Commit();
			}

	        DeletedVersions.RaiseEvent(new DeleteRevisionsEventArgs(id, false, dateToRetain: versionDate), this);

			Audit.Add(AuditTypes.Delete, "Delete Media by version date performed by user", userId == -1 ? 0 : userId, -1);
        }

        /// <summary>
        /// Permanently deletes specific version(s) from an <see cref="IMedia"/> object.
        /// </summary>
        /// <param name="id">Id of the <see cref="IMedia"/> object to delete a version from</param>
        /// <param name="versionId">Id of the version to delete</param>
        /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        public void DeleteVersion(int id, Guid versionId, bool deletePriorVersions, int userId = -1)
        {
            if (deletePriorVersions)
            {
                var content = GetByVersion(versionId);
                DeleteVersions(id, content.UpdateDate, userId);
            }

			if (DeletingVersions.IsRaisedEventCancelled(new DeleteRevisionsEventArgs(id, specificVersion:versionId), this))
				return;

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
			{
				repository.DeleteVersion(versionId);
				uow.Commit();
			}

	        DeletedVersions.RaiseEvent(new DeleteRevisionsEventArgs(id, false, specificVersion: versionId), this);

			Audit.Add(AuditTypes.Delete, "Delete Media by version performed by user", userId == -1 ? 0 : userId, -1);
        }

	    /// <summary>
	    /// Saves a single <see cref="IMedia"/> object
	    /// </summary>
	    /// <param name="media">The <see cref="IMedia"/> to save</param>
	    /// <param name="userId">Id of the User saving the Content</param>
	    public void Save(IMedia media, int userId = -1)
	    {
			if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMedia>(media), this))
				return;

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
			{
				SetUser(media, userId);
				repository.AddOrUpdate(media);
				uow.Commit();
			}

			Saved.RaiseEvent(new SaveEventArgs<IMedia>(media, false), this);

	        Audit.Add(AuditTypes.Save, "Save Media performed by user", media.CreatorId, media.Id);
	    }

	    /// <summary>
	    /// Saves a collection of <see cref="IMedia"/> objects
	    /// </summary>
	    /// <param name="medias">Collection of <see cref="IMedia"/> to save</param>
	    /// <param name="userId">Id of the User saving the Content</param>
	    public void Save(IEnumerable<IMedia> medias, int userId = -1)
	    {
			if (SavingCollection.IsRaisedEventCancelled(new SaveEventArgs<IEnumerable<IMedia>>(medias), this))
				return;

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = _repositoryFactory.CreateMediaRepository(uow))
			{
				foreach (var media in medias)
				{
					SetUser(media, userId);
					repository.AddOrUpdate(media);
				}

				//commit the whole lot in one go
				uow.Commit();
			}

		    SavedCollection.RaiseEvent(new SaveEventArgs<IEnumerable<IMedia>>(medias, false), this);

			Audit.Add(AuditTypes.Save, "Save Media items performed by user", userId == -1 ? 0 : userId, -1);
	    }

	    /// <summary>
		/// Internal method to set the HttpContextBase for testing.
		/// </summary>
		/// <param name="httpContext"><see cref="HttpContextBase"/></param>
		internal void SetHttpContext(HttpContextBase httpContext)
		{
			_httpContext = httpContext;
		}

		/// <summary>
		/// Updates a media object with the User (id), who created the content.
		/// </summary>
		/// <param name="media"><see cref="IMedia"/> object to update</param>
		/// <param name="userId">Optional Id of the User</param>
		private void SetUser(IMedia media, int userId)
		{
			if (userId > -1)
			{
				//If a user id was passed in we use that
				media.CreatorId = userId;
			}
			else if (UserServiceOrContext())
			{
				var profile = _httpContext == null
								  ? _userService.GetCurrentBackOfficeUser()
								  : _userService.GetCurrentBackOfficeUser(_httpContext);
				media.CreatorId = profile.Id.SafeCast<int>();
			}
			else
			{
				//Otherwise we default to Admin user, which should always exist (almost always)
				media.CreatorId = 0;
			}
		}

		private bool UserServiceOrContext()
		{
			return _userService != null && (HttpContext.Current != null || _httpContext != null);
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
		/// Occurs before saving a collection
		/// </summary>
		public static event TypedEventHandler<IMediaService, SaveEventArgs<IEnumerable<IMedia>>> SavingCollection;

		/// <summary>
		/// Occurs after saving a collection
		/// </summary>
		public static event TypedEventHandler<IMediaService, SaveEventArgs<IEnumerable<IMedia>>> SavedCollection;

		/// <summary>
		/// Occurs before Create
		/// </summary>
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
		#endregion
	}
}