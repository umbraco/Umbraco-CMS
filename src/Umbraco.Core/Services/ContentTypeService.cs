using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core.Auditing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the ContentType Service, which is an easy access to operations involving <see cref="IContentType"/>
    /// </summary>
    public class ContentTypeService : IContentTypeService
    {
	    private readonly RepositoryFactory _repositoryFactory;
	    private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private readonly IUserService _userService;
        private readonly IDatabaseUnitOfWork _unitOfWork;
	    private readonly IContentTypeRepository _contentTypeRepository;
	    private readonly IMediaTypeRepository _mediaTypeRepository;
        private HttpContextBase _httpContext;

        public ContentTypeService(RepositoryFactory repositoryFactory, IContentService contentService, IMediaService mediaService)
			: this(new PetaPocoUnitOfWorkProvider(), repositoryFactory, contentService, mediaService)
        {}

        public ContentTypeService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, IContentService contentService, IMediaService mediaService)
        {
	        _repositoryFactory = repositoryFactory;
	        _contentService = contentService;
            _mediaService = mediaService;
            _unitOfWork = provider.GetUnitOfWork();
	        _contentTypeRepository = _repositoryFactory.CreateContentTypeRepository(_unitOfWork);
	        _mediaTypeRepository = _repositoryFactory.CreateMediaTypeRepository(_unitOfWork);
        }

		internal ContentTypeService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, IContentService contentService, IMediaService mediaService, IUserService userService)
        {
			_repositoryFactory = repositoryFactory;
			_contentService = contentService;
            _mediaService = mediaService;
            _userService = userService;
            _unitOfWork = provider.GetUnitOfWork();
            _contentTypeRepository = _repositoryFactory.CreateContentTypeRepository(_unitOfWork);
            _mediaTypeRepository = _repositoryFactory.CreateMediaTypeRepository(_unitOfWork);
        }

        /// <summary>
        /// Gets an <see cref="IContentType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/> to retrieve</param>
        /// <returns><see cref="IContentType"/></returns>
        public IContentType GetContentType(int id)
        {
            var repository = _contentTypeRepository;
            return repository.Get(id);
        }

        /// <summary>
        /// Gets an <see cref="IContentType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IContentType"/> to retrieve</param>
        /// <returns><see cref="IContentType"/></returns>
        public IContentType GetContentType(string alias)
        {
            var repository = _contentTypeRepository;

            var query = Query<IContentType>.Builder.Where(x => x.Alias == alias);
            var contentTypes = repository.GetByQuery(query);

            return contentTypes.FirstOrDefault();
        }

        /// <summary>
        /// Gets a list of all available <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<IContentType> GetAllContentTypes(params int[] ids)
        {
            var repository = _contentTypeRepository;
            return repository.GetAll(ids);
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IContentType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<IContentType> GetContentTypeChildren(int id)
        {
            var query = Query<IContentType>.Builder.Where(x => x.ParentId == id);
            var contentTypes = _contentTypeRepository.GetByQuery(query);
            return contentTypes;
        }

        /// <summary>
        /// Saves a single <see cref="IContentType"/> object
        /// </summary>
        /// <param name="contentType"><see cref="IContentType"/> to save</param>
        /// <param name="userId">Optional id of the user saving the ContentType</param>
        public void Save(IContentType contentType, int userId = -1)
        {
            var e = new SaveEventArgs();
            if (Saving != null)
                Saving(contentType, e);

            if (!e.Cancel)
            {
                SetUser(contentType, userId);
                _contentTypeRepository.AddOrUpdate(contentType);
                _unitOfWork.Commit();

                if (Saved != null)
                    Saved(contentType, e);

                Audit.Add(AuditTypes.Save, string.Format("Save ContentType performed by user"), userId == -1 ? 0 : userId, contentType.Id);
            }
        }

        /// <summary>
        /// Saves a collection of <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="contentTypes">Collection of <see cref="IContentType"/> to save</param>
        /// <param name="userId">Optional id of the user saving the ContentType</param>
        public void Save(IEnumerable<IContentType> contentTypes, int userId = -1)
        {
            var e = new SaveEventArgs();
            if (Saving != null)
                Saving(contentTypes, e);

            if (!e.Cancel)
            {
                foreach (var contentType in contentTypes)
                {
                    SetUser(contentType, userId);
                    _contentTypeRepository.AddOrUpdate(contentType);
                }
                _unitOfWork.Commit();

                if (Saved != null)
                    Saved(contentTypes, e);

                Audit.Add(AuditTypes.Save, string.Format("Save ContentTypes performed by user"), userId == -1 ? 0 : userId, -1);
            }
        }

        /// <summary>
        /// Saves a collection of lazy loaded <see cref="IContentType"/> objects.
        /// </summary>
        /// <remarks>
        /// This method ensures that ContentType is saved lazily, so a new graph of <see cref="IContentType"/>
        /// objects can be saved in bulk. But note that objects are saved one at a time to ensure Ids.
        /// </remarks>
        /// <param name="contentTypes">Collection of Lazy <see cref="IContentType"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the ContentTypes</param>
        public void Save(IEnumerable<Lazy<IContentType>> contentTypes, int userId = -1)
        {
            var e = new SaveEventArgs();
            if (Saving != null)
                Saving(contentTypes, e);

            if (!e.Cancel)
            {
                foreach (var content in contentTypes)
                {
                    content.Value.CreatorId = 0;
                    _contentTypeRepository.AddOrUpdate(content.Value);
                    _unitOfWork.Commit();
                }

                if (Saved != null)
                    Saved(contentTypes, e);

                Audit.Add(AuditTypes.Save, string.Format("Save (lazy) ContentTypes performed by user"), userId == -1 ? 0 : userId, -1);
            }
        }

        /// <summary>
        /// Deletes a single <see cref="IContentType"/> object
        /// </summary>
        /// <param name="contentType"><see cref="IContentType"/> to delete</param>
        /// <param name="userId">Optional id of the user issueing the delete</param>
        /// <remarks>Deleting a <see cref="IContentType"/> will delete all the <see cref="IContent"/> objects based on this <see cref="IContentType"/></remarks>
        public void Delete(IContentType contentType, int userId = -1)
        {
            var e = new DeleteEventArgs { Id = contentType.Id };
            if (Deleting != null)
                Deleting(contentType, e);

            if (!e.Cancel)
            {
                _contentService.DeleteContentOfType(contentType.Id);

                var repository = _contentTypeRepository;

                repository.Delete(contentType);
                _unitOfWork.Commit();

                if (Deleted != null)
                    Deleted(contentType, e);

                Audit.Add(AuditTypes.Delete, string.Format("Delete ContentType performed by user"), userId == -1 ? 0 : userId, contentType.Id);
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
        public void Delete(IEnumerable<IContentType> contentTypes, int userId = -1)
        {
            var e = new DeleteEventArgs();
            if (Deleting != null)
                Deleting(contentTypes, e);

            if (!e.Cancel)
            {
                var contentTypeList = contentTypes.ToList();
                foreach (var contentType in contentTypeList)
                {
                    _contentService.DeleteContentOfType(contentType.Id);
                }

                var repository = _contentTypeRepository;

                foreach (var contentType in contentTypeList)
                {
                    repository.Delete(contentType);
                }
                _unitOfWork.Commit();

                if (Deleted != null)
                    Deleted(contentTypes, e);

                Audit.Add(AuditTypes.Delete, string.Format("Delete ContentTypes performed by user"), userId == -1 ? 0 : userId, -1);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        public IMediaType GetMediaType(int id)
        {
            var repository = _mediaTypeRepository;
            return repository.Get(id);
        }

        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        public IMediaType GetMediaType(string alias)
        {
            var repository = _mediaTypeRepository;

            var query = Query<IMediaType>.Builder.Where(x => x.Alias == alias);
            var contentTypes = repository.GetByQuery(query);

            return contentTypes.FirstOrDefault();
        }

        /// <summary>
        /// Gets a list of all available <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        public IEnumerable<IMediaType> GetAllMediaTypes(params int[] ids)
        {
            var repository = _mediaTypeRepository;
            return repository.GetAll(ids);
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        public IEnumerable<IMediaType> GetMediaTypeChildren(int id)
        {
            var repository = _mediaTypeRepository;

            var query = Query<IMediaType>.Builder.Where(x => x.ParentId == id);
            var contentTypes = repository.GetByQuery(query);
            return contentTypes;
        }

        /// <summary>
        /// Saves a single <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="mediaType"><see cref="IMediaType"/> to save</param>
        /// <param name="userId">Optional Id of the user saving the MediaType</param>
        public void Save(IMediaType mediaType, int userId = -1)
        {
            var e = new SaveEventArgs();
            if (Saving != null)
                Saving(mediaType, e);

            if (!e.Cancel)
            {
                SetUser(mediaType, userId);
                _mediaTypeRepository.AddOrUpdate(mediaType);
                _unitOfWork.Commit();

                if (Saved != null)
                    Saved(mediaType, e);

                Audit.Add(AuditTypes.Save, string.Format("Save MediaType performed by user"), userId == -1 ? 0 : userId, mediaType.Id);
            }
        }

        /// <summary>
        /// Saves a collection of <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="mediaTypes">Collection of <see cref="IMediaType"/> to save</param>
        /// <param name="userId">Optional Id of the user savging the MediaTypes</param>
        public void Save(IEnumerable<IMediaType> mediaTypes, int userId = -1)
        {
            var e = new SaveEventArgs();
            if (Saving != null)
                Saving(mediaTypes, e);

            if (!e.Cancel)
            {
                foreach (var mediaType in mediaTypes)
                {
                    SetUser(mediaType, userId);
                    _mediaTypeRepository.AddOrUpdate(mediaType);
                }
                _unitOfWork.Commit();

                if (Saved != null)
                    Saved(mediaTypes, e);

                Audit.Add(AuditTypes.Save, string.Format("Save MediaTypes performed by user"), userId == -1 ? 0 : userId, -1);
            }
        }

        /// <summary>
        /// Deletes a single <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="mediaType"><see cref="IMediaType"/> to delete</param>
        /// <param name="userId">Optional Id of the user deleting the MediaType</param>
        /// <remarks>Deleting a <see cref="IMediaType"/> will delete all the <see cref="IMedia"/> objects based on this <see cref="IMediaType"/></remarks>
        public void Delete(IMediaType mediaType, int userId = -1)
        {
            var e = new DeleteEventArgs { Id = mediaType.Id };
            if (Deleting != null)
                Deleting(mediaType, e);

            if (!e.Cancel)
            {
                _mediaService.DeleteMediaOfType(mediaType.Id);

                var repository = _mediaTypeRepository;

                repository.Delete(mediaType);
                _unitOfWork.Commit();

                if (Deleted != null)
                    Deleted(mediaType, e);

                Audit.Add(AuditTypes.Delete, string.Format("Delete MediaType performed by user"), userId == -1 ? 0 : userId, mediaType.Id);
            }
        }

        /// <summary>
        /// Deletes a collection of <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="mediaTypes">Collection of <see cref="IMediaType"/> to delete</param>
        /// <param name="userId"></param>
        /// <remarks>Deleting a <see cref="IMediaType"/> will delete all the <see cref="IMedia"/> objects based on this <see cref="IMediaType"/></remarks>
        public void Delete(IEnumerable<IMediaType> mediaTypes, int userId = -1)
        {
            var e = new DeleteEventArgs();
            if (Deleting != null)
                Deleting(mediaTypes, e);

            if (!e.Cancel)
            {
                var mediaTypeList = mediaTypes.ToList();
                foreach (var mediaType in mediaTypeList)
                {
                    _mediaService.DeleteMediaOfType(mediaType.Id);
                }

                var repository = _mediaTypeRepository;

                foreach (var mediaType in mediaTypeList)
                {
                    repository.Delete(mediaType);
                }
                _unitOfWork.Commit();

                if (Deleted != null)
                    Deleted(mediaTypes, e);

                Audit.Add(AuditTypes.Delete, string.Format("Delete MediaTypes performed by user"), userId == -1 ? 0 : userId, -1);
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
            if (UmbracoSettings.UseLegacyXmlSchema)
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
                        string safeAlias = contentType.Alias.ToUmbracoAlias(StringAliasCaseType.CamelCase, true);
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
                    // Note, Log.Add quietly swallows the exception if it can't write to the database
                    //Log.Add(LogTypes.System, -1, string.Format("{0} while trying to build DTD for Xml schema; is Umbraco installed correctly and the connection string configured?", exception.Message));
                }

            }
            return dtd.ToString();
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
        /// Updates a content object with the User (id), who created the content.
        /// </summary>
        /// <param name="contentType">ContentType object to update</param>
        /// <param name="userId">Optional Id of the User</param>
        private void SetUser(IContentTypeBase contentType, int userId)
        {
            if (userId > -1)
            {
                //If a user id was passed in we use that
                contentType.CreatorId = userId;
            }
            else if (UserServiceOrContext())
            {
                var profile = _httpContext == null
                                  ? _userService.GetCurrentBackOfficeUser()
                                  : _userService.GetCurrentBackOfficeUser(_httpContext);
                contentType.CreatorId = profile.Id.SafeCast<int>();
            }
            else
            {
                //Otherwise we default to Admin user, which should always exist (almost always)
                contentType.CreatorId = 0;
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
        #endregion
    }
}