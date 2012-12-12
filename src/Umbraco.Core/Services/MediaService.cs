using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Auditing;
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
		private readonly IUserService _userService;
		private HttpContextBase _httpContext;

		public MediaService()
			: this(new PetaPocoUnitOfWorkProvider())
		{
		}

		public MediaService(IDatabaseUnitOfWorkProvider provider)
		{
			_uowProvider = provider;			
		}

		internal MediaService(IDatabaseUnitOfWorkProvider provider, IUserService userService)
		{
			_uowProvider = provider;
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
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaTypeRepository(uow))
			{
				var query = Query<IMediaType>.Builder.Where(x => x.Alias == mediaTypeAlias);
				var mediaTypes = repository.GetByQuery(query);

				if (!mediaTypes.Any())
					throw new Exception(string.Format("No ContentType matching the passed in Alias: '{0}' was found", mediaTypeAlias));

				var mediaType = mediaTypes.First();

				if (mediaType == null)
					throw new Exception(string.Format("ContentType matching the passed in Alias: '{0}' was null", mediaTypeAlias));

				var media = new Models.Media(parentId, mediaType);

				var e = new NewEventArgs { Alias = mediaTypeAlias, ParentId = parentId };
				
				if (Creating != null)
					Creating(media, e);

				if (!e.Cancel)
				{
					
					SetUser(media, userId);

					if (Created != null)
						Created(media, e);

					Audit.Add(AuditTypes.New, "", media.CreatorId, media.Id);
				}

				return media;	
			}
			
		}

		/// <summary>
		/// Gets an <see cref="IMedia"/> object by Id
		/// </summary>
		/// <param name="id">Id of the Content to retrieve</param>
		/// <returns><see cref="IMedia"/></returns>
		public IMedia GetById(int id)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaRepository(uow))
			{
				return repository.Get(id);	
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
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaRepository(uow))
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
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaRepository(uow))
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
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaRepository(uow))
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
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaRepository(uow))
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
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaRepository(uow))
			{
				var query = Query<IMedia>.Builder.Where(x => x.ParentId == -20);
				var medias = repository.GetByQuery(query);

				return medias;
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
			var e = new MoveEventArgs { ParentId = parentId };
			if (Moving != null)
				Moving(media, e);

			if (!e.Cancel)
			{
				media.ParentId = parentId;
				Save(media, userId);

				if (Moved != null)
					Moved(media, e);

				Audit.Add(AuditTypes.Move, "Move Media performed by user", userId == -1 ? 0 : userId, media.Id);
			}
		}

		/// <summary>
		/// Deletes an <see cref="IMedia"/> object by moving it to the Recycle Bin
		/// </summary>
		/// <param name="media">The <see cref="IMedia"/> to delete</param>
		/// <param name="userId">Id of the User deleting the Media</param>
		public void MoveToRecycleBin(IMedia media, int userId = -1)
		{
			//TODO If media item has children those should also be moved to the recycle bin as well

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaRepository(uow))
			{
				var e = new MoveEventArgs { ParentId = -20 };
				if (Trashing != null)
					Trashing(media, e);

				if (!e.Cancel)
				{				
					((Core.Models.Media)media).ChangeTrashedState(true);
					repository.AddOrUpdate(media);
					uow.Commit();

					if (Trashed != null)
						Trashed(media, e);

					Audit.Add(AuditTypes.Move, "Move Media to Recycle Bin performed by user", userId == -1 ? 0 : userId, media.Id);
				}
			}			
		}

		/// <summary>
		/// Empties the Recycle Bin by deleting all <see cref="IMedia"/> that resides in the bin
		/// </summary>
		public void EmptyRecycleBin()
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaRepository(uow))
			{
				var query = Query<IMedia>.Builder.Where(x => x.ParentId == -20);
				var contents = repository.GetByQuery(query);

				foreach (var content in contents)
				{
					repository.Delete(content);
				}
				uow.Commit();

				Audit.Add(AuditTypes.Delete, "Empty Recycle Bin performed by user", 0, -20);
			}
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
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaRepository(uow))
			{
				//NOTE What about media that has the contenttype as part of its composition?
				//The ContentType has to be removed from the composition somehow as it would otherwise break
				//Dbl.check+test that the ContentType's Id is removed from the ContentType2ContentType table
				var query = Query<IMedia>.Builder.Where(x => x.ContentTypeId == mediaTypeId);
				var contents = repository.GetByQuery(query);

				var e = new DeleteEventArgs { Id = mediaTypeId };
				if (Deleting != null)
					Deleting(contents, e);

				if (!e.Cancel)
				{
					foreach (var content in contents)
					{
						((Core.Models.Media)content).ChangeTrashedState(true);
						repository.AddOrUpdate(content);
					}

					uow.Commit();

					if (Deleted != null)
						Deleted(contents, e);

					Audit.Add(AuditTypes.Delete, "Delete Media items by Type performed by user", userId == -1 ? 0 : userId, -1);
				}
			}			
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
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaRepository(uow))
			{
				var e = new DeleteEventArgs { Id = media.Id };
				if (Deleting != null)
					Deleting(media, e);

				if (!e.Cancel)
				{
					repository.Delete(media);
					uow.Commit();

					if (Deleted != null)
						Deleted(media, e);

					Audit.Add(AuditTypes.Delete, "Delete Media performed by user", userId == -1 ? 0 : userId, media.Id);
				}
			}			
		}

		/// <summary>
		/// Saves a single <see cref="IMedia"/> object
		/// </summary>
		/// <param name="media">The <see cref="IMedia"/> to save</param>
		/// <param name="userId">Id of the User saving the Content</param>
		public void Save(IMedia media, int userId = -1)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaRepository(uow))
			{
				var e = new SaveEventArgs();
				if (Saving != null)
					Saving(media, e);

				if (!e.Cancel)
				{
					SetUser(media, userId);
					repository.AddOrUpdate(media);
					uow.Commit();

					if (Saved != null)
						Saved(media, e);
				}
				Audit.Add(AuditTypes.Save, "Save Media performed by user", media.CreatorId, media.Id);
			}			
		}

		/// <summary>
		/// Saves a collection of <see cref="IMedia"/> objects
		/// </summary>
		/// <param name="medias">Collection of <see cref="IMedia"/> to save</param>
		/// <param name="userId">Id of the User saving the Content</param>
		public void Save(IEnumerable<IMedia> medias, int userId = -1)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateMediaRepository(uow))
			{
				var e = new SaveEventArgs();
				if (Saving != null)
					Saving(medias, e);

				if (!e.Cancel)
				{
					foreach (var media in medias)
					{
						SetUser(media, userId);
						repository.AddOrUpdate(media);
					}
					uow.Commit();

					if (Saved != null)
						Saved(medias, e);

					Audit.Add(AuditTypes.Save, "Save Media items performed by user", userId == -1 ? 0 : userId, -1);
				}
			}			
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
		public static event EventHandler<DeleteEventArgs> Deleting;

		/// <summary>
		/// Occurs after Delete
		/// </summary>
		public static event EventHandler<DeleteEventArgs> Deleted;

		/// <summary>
		/// Occurs before Save
		/// </summary>
		public static event EventHandler<SaveEventArgs> Saving;

		/// <summary>
		/// Occurs after Save
		/// </summary>
		public static event EventHandler<SaveEventArgs> Saved;

		/// <summary>
		/// Occurs before Create
		/// </summary>
		public static event EventHandler<NewEventArgs> Creating;

		/// <summary>
		/// Occurs after Create
		/// </summary>
		public static event EventHandler<NewEventArgs> Created;

		/// <summary>
		/// Occurs before Content is moved to Recycle Bin
		/// </summary>
		public static event EventHandler<MoveEventArgs> Trashing;

		/// <summary>
		/// Occurs after Content is moved to Recycle Bin
		/// </summary>
		public static event EventHandler<MoveEventArgs> Trashed;

		/// <summary>
		/// Occurs before Move
		/// </summary>
		public static event EventHandler<MoveEventArgs> Moving;

		/// <summary>
		/// Occurs after Move
		/// </summary>
		public static event EventHandler<MoveEventArgs> Moved;
		#endregion
	}
}