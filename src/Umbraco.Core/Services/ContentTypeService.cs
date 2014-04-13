using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using Umbraco.Core.Auditing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the ContentType Service, which is an easy access to operations involving <see cref="IContentType"/>
    /// </summary>
    public class ContentTypeService : ContentTypeServiceBase, IContentTypeService
    {
	    private readonly RepositoryFactory _repositoryFactory;
	    private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;
        //Support recursive locks because some of the methods that require locking call other methods that require locking. 
        //for example, the Move method needs to be locked but this calls the Save method which also needs to be locked.
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion); 

        public ContentTypeService(IContentService contentService, IMediaService mediaService)
			: this(new PetaPocoUnitOfWorkProvider(), new RepositoryFactory(), contentService, mediaService)
        {}

        public ContentTypeService(RepositoryFactory repositoryFactory, IContentService contentService, IMediaService mediaService)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory, contentService, mediaService)
        { }

        public ContentTypeService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, IContentService contentService, IMediaService mediaService)
        {
            _uowProvider = provider;
	        _repositoryFactory = repositoryFactory;
	        _contentService = contentService;
            _mediaService = mediaService;
        }

        /// <summary>
        /// Gets an <see cref="IContentType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/> to retrieve</param>
        /// <returns><see cref="IContentType"/></returns>
        public IContentType GetContentType(int id)
        {
            using (var repository = _repositoryFactory.CreateContentTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets an <see cref="IContentType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IContentType"/> to retrieve</param>
        /// <returns><see cref="IContentType"/></returns>
        public IContentType GetContentType(string alias)
        {
            using (var repository = _repositoryFactory.CreateContentTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IContentType>.Builder.Where(x => x.Alias == alias);
                var contentTypes = repository.GetByQuery(query);

                return contentTypes.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a list of all available <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<IContentType> GetAllContentTypes(params int[] ids)
        {
            using (var repository = _repositoryFactory.CreateContentTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(ids);
            }
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IContentType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<IContentType> GetContentTypeChildren(int id)
        {
            using (var repository = _repositoryFactory.CreateContentTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IContentType>.Builder.Where(x => x.ParentId == id);
                var contentTypes = repository.GetByQuery(query);
                return contentTypes;
            }
        }

        ///// <summary>
        ///// Returns the content type descendant Ids for the content type specified
        ///// </summary>
        ///// <param name="contentTypeId"></param>
        ///// <returns></returns>
        //internal IEnumerable<int> GetDescendantContentTypeIds(int contentTypeId)
        //{            
        //    using (var uow = _uowProvider.GetUnitOfWork())
        //    {
        //        //method to return the child content type ids for the id specified
        //        Func<int, int[]> getChildIds =
        //            parentId =>
        //            uow.Database.Fetch<ContentType2ContentTypeDto>("WHERE parentContentTypeId = @Id", new {Id = parentId})
        //               .Select(x => x.ChildId).ToArray();

        //        //recursively get all descendant ids
        //        return getChildIds(contentTypeId).FlattenList(getChildIds);                
        //    }
        //} 

        /// <summary>
        /// Checks whether an <see cref="IContentType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/></param>
        /// <returns>True if the content type has any children otherwise False</returns>
        public bool HasChildren(int id)
        {
            using (var repository = _repositoryFactory.CreateContentTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IContentType>.Builder.Where(x => x.ParentId == id);
                int count = repository.Count(query);
                return count > 0;
            }
        }

        /// <summary>
        /// This is called after an IContentType is saved and is used to update the content xml structures in the database
        /// if they are required to be updated.
        /// </summary>
        /// <param name="contentTypes">A tuple of a content type and a boolean indicating if it is new (HasIdentity was false before committing)</param>
        private void UpdateContentXmlStructure(params IContentTypeBase[] contentTypes)
        {

            var toUpdate = GetContentTypesForXmlUpdates(contentTypes).ToArray();

            if (toUpdate.Any())
            {
                var firstType = toUpdate.First();
                //if it is a content type then call the rebuilding methods or content
                if (firstType is IContentType)
                {
                    var typedContentService = _contentService as ContentService;
                    if (typedContentService != null)
                    {
                        typedContentService.RePublishAll(toUpdate.Select(x => x.Id).ToArray());
                    }
                    else
                    {
                        //this should never occur, the content service should always be typed but we'll check anyways.
                        _contentService.RePublishAll();
                    }    
                }
                else if (firstType is IMediaType)
                {
                    //if it is a media type then call the rebuilding methods for media
                    var typedContentService = _mediaService as MediaService;
                    if (typedContentService != null)
                    {
                        typedContentService.RebuildXmlStructures(toUpdate.Select(x => x.Id).ToArray());
                    }                     
                }
            }
            
        }

        /// <summary>
        /// Saves a single <see cref="IContentType"/> object
        /// </summary>
        /// <param name="contentType"><see cref="IContentType"/> to save</param>
        /// <param name="userId">Optional id of the user saving the ContentType</param>
        public void Save(IContentType contentType, int userId = 0)
        {
	        if (SavingContentType.IsRaisedEventCancelled(new SaveEventArgs<IContentType>(contentType), this)) 
				return;

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateContentTypeRepository(uow))
                {
                    contentType.CreatorId = userId;
                    repository.AddOrUpdate(contentType);

                    uow.Commit();
                }

                UpdateContentXmlStructure(contentType);
            }
            SavedContentType.RaiseEvent(new SaveEventArgs<IContentType>(contentType, false), this);
	        Audit.Add(AuditTypes.Save, string.Format("Save ContentType performed by user"), userId, contentType.Id);
        }

        /// <summary>
        /// Saves a collection of <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="contentTypes">Collection of <see cref="IContentType"/> to save</param>
        /// <param name="userId">Optional id of the user saving the ContentType</param>
        public void Save(IEnumerable<IContentType> contentTypes, int userId = 0)
        {
            var asArray = contentTypes.ToArray();

            if (SavingContentType.IsRaisedEventCancelled(new SaveEventArgs<IContentType>(asArray), this)) 
				return;

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateContentTypeRepository(uow))
                {
                    foreach (var contentType in asArray)
                    {
                        contentType.CreatorId = userId;
                        repository.AddOrUpdate(contentType);
                    }

                    //save it all in one go
                    uow.Commit();
                }

                UpdateContentXmlStructure(asArray.Cast<IContentTypeBase>().ToArray());
            }
            SavedContentType.RaiseEvent(new SaveEventArgs<IContentType>(asArray, false), this);
	        Audit.Add(AuditTypes.Save, string.Format("Save ContentTypes performed by user"), userId, -1);
        }

        /// <summary>
        /// Deletes a single <see cref="IContentType"/> object
        /// </summary>
        /// <param name="contentType"><see cref="IContentType"/> to delete</param>
        /// <param name="userId">Optional id of the user issueing the delete</param>
        /// <remarks>Deleting a <see cref="IContentType"/> will delete all the <see cref="IContent"/> objects based on this <see cref="IContentType"/></remarks>
        public void Delete(IContentType contentType, int userId = 0)
        {            
	        if (DeletingContentType.IsRaisedEventCancelled(new DeleteEventArgs<IContentType>(contentType), this)) 
				return;

            using (new WriteLock(Locker))
            {
                _contentService.DeleteContentOfType(contentType.Id);

                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateContentTypeRepository(uow))
                {
                    repository.Delete(contentType);
                    uow.Commit();

                    DeletedContentType.RaiseEvent(new DeleteEventArgs<IContentType>(contentType, false), this);
                }

                Audit.Add(AuditTypes.Delete, string.Format("Delete ContentType performed by user"), userId, contentType.Id);
            }
        }

        /// <summary>
        /// Deletes a collection of <see cref="IContentType"/> objects.
        /// </summary>
        /// <param name="contentTypes">Collection of <see cref="IContentType"/> to delete</param>
        /// <param name="userId">Optional id of the user issueing the delete</param>
        /// <remarks>
        /// Deleting a <see cref="IContentType"/> will delete all the <see cref="IContent"/> objects based on this <see cref="IContentType"/>
        /// </remarks>
        public void Delete(IEnumerable<IContentType> contentTypes, int userId = 0)
        {
            var asArray = contentTypes.ToArray();

            if (DeletingContentType.IsRaisedEventCancelled(new DeleteEventArgs<IContentType>(asArray), this)) 
				return;

            using (new WriteLock(Locker))
            {
                foreach (var contentType in asArray)
                {
                    _contentService.DeleteContentOfType(contentType.Id);
                }

                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateContentTypeRepository(uow))
                {
                    foreach (var contentType in asArray)
                    {
                        repository.Delete(contentType);
                    }

                    uow.Commit();

                    DeletedContentType.RaiseEvent(new DeleteEventArgs<IContentType>(asArray, false), this);
                }

                Audit.Add(AuditTypes.Delete, string.Format("Delete ContentTypes performed by user"), userId, -1);
            }
        }
        
        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        public IMediaType GetMediaType(int id)
        {
            using (var repository = _repositoryFactory.CreateMediaTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        public IMediaType GetMediaType(string alias)
        {
            using (var repository = _repositoryFactory.CreateMediaTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMediaType>.Builder.Where(x => x.Alias == alias);
                var contentTypes = repository.GetByQuery(query);

                return contentTypes.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a list of all available <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        public IEnumerable<IMediaType> GetAllMediaTypes(params int[] ids)
        {
            using (var repository = _repositoryFactory.CreateMediaTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(ids);
            }
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        public IEnumerable<IMediaType> GetMediaTypeChildren(int id)
        {
            using (var repository = _repositoryFactory.CreateMediaTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMediaType>.Builder.Where(x => x.ParentId == id);
                var contentTypes = repository.GetByQuery(query);
                return contentTypes;
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IMediaType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/></param>
        /// <returns>True if the media type has any children otherwise False</returns>
        public bool MediaTypeHasChildren(int id)
        {
            using (var repository = _repositoryFactory.CreateMediaTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMediaType>.Builder.Where(x => x.ParentId == id);
                int count = repository.Count(query);
                return count > 0;
            }
        }

        /// <summary>
        /// Saves a single <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="mediaType"><see cref="IMediaType"/> to save</param>
        /// <param name="userId">Optional Id of the user saving the MediaType</param>
        public void Save(IMediaType mediaType, int userId = 0)
        {
	        if (SavingMediaType.IsRaisedEventCancelled(new SaveEventArgs<IMediaType>(mediaType), this)) 
				return;

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateMediaTypeRepository(uow))
                {
                    mediaType.CreatorId = userId;
                    repository.AddOrUpdate(mediaType);
                    uow.Commit();
                    
                }

                UpdateContentXmlStructure(mediaType);
            }

            SavedMediaType.RaiseEvent(new SaveEventArgs<IMediaType>(mediaType, false), this);
	        Audit.Add(AuditTypes.Save, string.Format("Save MediaType performed by user"), userId, mediaType.Id);
        }

        /// <summary>
        /// Saves a collection of <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="mediaTypes">Collection of <see cref="IMediaType"/> to save</param>
        /// <param name="userId">Optional Id of the user savging the MediaTypes</param>
        public void Save(IEnumerable<IMediaType> mediaTypes, int userId = 0)
        {
            var asArray = mediaTypes.ToArray();

            if (SavingMediaType.IsRaisedEventCancelled(new SaveEventArgs<IMediaType>(asArray), this))
				return;

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateMediaTypeRepository(uow))
                {

                    foreach (var mediaType in asArray)
                    {
                        mediaType.CreatorId = userId;
                        repository.AddOrUpdate(mediaType);
                    }

                    //save it all in one go
                    uow.Commit();                    
                }

                UpdateContentXmlStructure(asArray.Cast<IContentTypeBase>().ToArray());
            }

            SavedMediaType.RaiseEvent(new SaveEventArgs<IMediaType>(asArray, false), this);
			Audit.Add(AuditTypes.Save, string.Format("Save MediaTypes performed by user"), userId, -1);
        }

        /// <summary>
        /// Deletes a single <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="mediaType"><see cref="IMediaType"/> to delete</param>
        /// <param name="userId">Optional Id of the user deleting the MediaType</param>
        /// <remarks>Deleting a <see cref="IMediaType"/> will delete all the <see cref="IMedia"/> objects based on this <see cref="IMediaType"/></remarks>
        public void Delete(IMediaType mediaType, int userId = 0)
        {
	        if (DeletingMediaType.IsRaisedEventCancelled(new DeleteEventArgs<IMediaType>(mediaType), this)) 
				return;
            using (new WriteLock(Locker))
            {
                _mediaService.DeleteMediaOfType(mediaType.Id, userId);

                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateMediaTypeRepository(uow))
                {

                    repository.Delete(mediaType);
                    uow.Commit();

                    DeletedMediaType.RaiseEvent(new DeleteEventArgs<IMediaType>(mediaType, false), this);
                }

                Audit.Add(AuditTypes.Delete, string.Format("Delete MediaType performed by user"), userId, mediaType.Id);
            }
        }

        /// <summary>
        /// Deletes a collection of <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="mediaTypes">Collection of <see cref="IMediaType"/> to delete</param>
        /// <param name="userId"></param>
        /// <remarks>Deleting a <see cref="IMediaType"/> will delete all the <see cref="IMedia"/> objects based on this <see cref="IMediaType"/></remarks>
        public void Delete(IEnumerable<IMediaType> mediaTypes, int userId = 0)
        {
            var asArray = mediaTypes.ToArray();

            if (DeletingMediaType.IsRaisedEventCancelled(new DeleteEventArgs<IMediaType>(asArray), this)) 
				return;
            using (new WriteLock(Locker))
            {
                foreach (var mediaType in asArray)
                {
                    _mediaService.DeleteMediaOfType(mediaType.Id);
                }

                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateMediaTypeRepository(uow))
                {
                    foreach (var mediaType in asArray)
                    {
                        repository.Delete(mediaType);
                    }
                    uow.Commit();

                    DeletedMediaType.RaiseEvent(new DeleteEventArgs<IMediaType>(asArray, false), this);
                }

                Audit.Add(AuditTypes.Delete, string.Format("Delete MediaTypes performed by user"), userId, -1);
            }            
        }

        /// <summary>
        /// Generates the complete (simplified) XML DTD.
        /// </summary>
        /// <returns>The DTD as a string</returns>
        public string GetDtd()
        {
            var dtd = new StringBuilder();
            dtd.AppendLine("<!DOCTYPE root [ ");

            dtd.AppendLine(GetContentTypesDtd());
            dtd.AppendLine("]>");

            return dtd.ToString();
        }

        /// <summary>
        /// Generates the complete XML DTD without the root.
        /// </summary>
        /// <returns>The DTD as a string</returns>
        public string GetContentTypesDtd()
        {
            var dtd = new StringBuilder();
            if (UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
            {
                dtd.AppendLine("<!ELEMENT node ANY> <!ATTLIST node id ID #REQUIRED>  <!ELEMENT data ANY>");
            }
            else
            {
                try
                {
                    var strictSchemaBuilder = new StringBuilder();

                    var contentTypes = GetAllContentTypes();
                    foreach (ContentType contentType in contentTypes)
                    {
                        string safeAlias = contentType.Alias.ToUmbracoAlias();
                        if (safeAlias != null)
                        {
                            strictSchemaBuilder.AppendLine(String.Format("<!ELEMENT {0} ANY>", safeAlias));
                            strictSchemaBuilder.AppendLine(String.Format("<!ATTLIST {0} id ID #REQUIRED>", safeAlias));
                        }
                    }

                    // Only commit the strong schema to the container if we didn't generate an error building it
                    dtd.Append(strictSchemaBuilder);
                }
                catch (Exception exception)
                {
                    LogHelper.Error<ContentTypeService>("Error while trying to build DTD for Xml schema; is Umbraco installed correctly and the connection string configured?", exception);
                }

            }
            return dtd.ToString();
        }
        
        #region Event Handlers

		/// <summary>
		/// Occurs before Delete
		/// </summary>
		public static event TypedEventHandler<IContentTypeService, DeleteEventArgs<IContentType>> DeletingContentType;

		/// <summary>
		/// Occurs after Delete
		/// </summary>
		public static event TypedEventHandler<IContentTypeService, DeleteEventArgs<IContentType>> DeletedContentType;
		
		/// <summary>
		/// Occurs before Delete
		/// </summary>
		public static event TypedEventHandler<IContentTypeService, DeleteEventArgs<IMediaType>> DeletingMediaType;

		/// <summary>
		/// Occurs after Delete
		/// </summary>
		public static event TypedEventHandler<IContentTypeService, DeleteEventArgs<IMediaType>> DeletedMediaType;
		
        /// <summary>
        /// Occurs before Save
        /// </summary>
		public static event TypedEventHandler<IContentTypeService, SaveEventArgs<IContentType>> SavingContentType;

        /// <summary>
        /// Occurs after Save
        /// </summary>
		public static event TypedEventHandler<IContentTypeService, SaveEventArgs<IContentType>> SavedContentType;

		/// <summary>
		/// Occurs before Save
		/// </summary>
		public static event TypedEventHandler<IContentTypeService, SaveEventArgs<IMediaType>> SavingMediaType;

		/// <summary>
		/// Occurs after Save
		/// </summary>
		public static event TypedEventHandler<IContentTypeService, SaveEventArgs<IMediaType>> SavedMediaType;

        #endregion
    }
}