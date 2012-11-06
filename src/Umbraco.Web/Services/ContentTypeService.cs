using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using umbraco;

namespace Umbraco.Web.Services
{
    /// <summary>
    /// Represents the ContentType Service, which is an easy access to operations involving <see cref="IContentType"/>
    /// </summary>
    public class ContentTypeService : IContentTypeService
    {
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private readonly IUnitOfWork _unitOfWork;

        public ContentTypeService(IContentService contentService, IMediaService mediaService)
            : this(contentService, mediaService, new PetaPocoUnitOfWorkProvider())
        {}

        public ContentTypeService(IContentService contentService, IMediaService mediaService, IUnitOfWorkProvider provider)
        {
            _contentService = contentService;
            _mediaService = mediaService;
            _unitOfWork = provider.GetUnitOfWork();
        }

        /// <summary>
        /// Gets an <see cref="IContentType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/> to retrieve</param>
        /// <returns><see cref="IContentType"/></returns>
        public IContentType GetContentType(int id)
        {
            var repository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(_unitOfWork);
            return repository.Get(id);
        }

        /// <summary>
        /// Gets an <see cref="IContentType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IContentType"/> to retrieve</param>
        /// <returns><see cref="IContentType"/></returns>
        public IContentType GetContentType(string alias)
        {
            var repository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(_unitOfWork);

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
            var repository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(_unitOfWork);
            return repository.GetAll(ids);
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IContentType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<IContentType> GetContentTypeChildren(int id)
        {
            var repository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(_unitOfWork);

            var query = Query<IContentType>.Builder.Where(x => x.ParentId == id);
            var contentTypes = repository.GetByQuery(query);
            return contentTypes;
        }

        /// <summary>
        /// Saves a single <see cref="IContentType"/> object
        /// </summary>
        /// <param name="contentType"><see cref="IContentType"/> to save</param>
        public void Save(IContentType contentType)
        {
            var repository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(_unitOfWork);

            repository.AddOrUpdate(contentType);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Saves a collection of <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="contentTypes">Collection of <see cref="IContentType"/> to save</param>
        public void Save(IEnumerable<IContentType> contentTypes)
        {
            var repository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(_unitOfWork);

            foreach (var contentType in contentTypes)
            {
                repository.AddOrUpdate(contentType);
            }
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes a single <see cref="IContentType"/> object
        /// </summary>
        /// <param name="contentType"><see cref="IContentType"/> to delete</param>
        /// <remarks>Deleting a <see cref="IContentType"/> will delete all the <see cref="IContent"/> objects based on this <see cref="IContentType"/></remarks>
        public void Delete(IContentType contentType)
        {
            _contentService.DeleteContentOfType(contentType.Id);

            var repository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(_unitOfWork);

            repository.Delete(contentType);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes a collection of <see cref="IContentType"/> objects.
        /// </summary>
        /// <param name="contentTypes">Collection of <see cref="IContentType"/> to delete</param>
        /// <remarks>
        /// Deleting a <see cref="IContentType"/> will delete all the <see cref="IContent"/> objects based on this <see cref="IContentType"/>
        /// </remarks>
        public void Delete(IEnumerable<IContentType> contentTypes)
        {
            var contentTypeList = contentTypes.ToList();
            foreach (var contentType in contentTypeList)
            {
                _contentService.DeleteContentOfType(contentType.Id);
            }

            var repository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(_unitOfWork);

            foreach (var contentType in contentTypeList)
            {
                repository.Delete(contentType);
            }
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        public IMediaType GetMediaType(int id)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaTypeRepository, IMediaType, int>(_unitOfWork);
            return repository.Get(id);
        }

        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        public IMediaType GetMediaType(string alias)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaTypeRepository, IMediaType, int>(_unitOfWork);

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
            var repository = RepositoryResolver.ResolveByType<IMediaTypeRepository, IMediaType, int>(_unitOfWork);
            return repository.GetAll(ids);
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        public IEnumerable<IMediaType> GetMediaTypeChildren(int id)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaTypeRepository, IMediaType, int>(_unitOfWork);

            var query = Query<IMediaType>.Builder.Where(x => x.ParentId == id);
            var contentTypes = repository.GetByQuery(query);
            return contentTypes;
        }

        /// <summary>
        /// Saves a single <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="mediaType"><see cref="IMediaType"/> to save</param>
        public void Save(IMediaType mediaType)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaTypeRepository, IMediaType, int>(_unitOfWork);

            repository.AddOrUpdate(mediaType);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Saves a collection of <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="mediaTypes">Collection of <see cref="IMediaType"/> to save</param>
        public void Save(IEnumerable<IMediaType> mediaTypes)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaTypeRepository, IMediaType, int>(_unitOfWork);

            foreach (var mediaType in mediaTypes)
            {
                repository.AddOrUpdate(mediaType);
            }
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes a single <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="mediaType"><see cref="IMediaType"/> to delete</param>
        /// <remarks>Deleting a <see cref="IMediaType"/> will delete all the <see cref="IMedia"/> objects based on this <see cref="IMediaType"/></remarks>
        public void Delete(IMediaType mediaType)
        {
            _mediaService.DeleteMediaOfType(mediaType.Id);
            
            var repository = RepositoryResolver.ResolveByType<IMediaTypeRepository, IMediaType, int>(_unitOfWork);

            repository.Delete(mediaType);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes a collection of <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="mediaTypes">Collection of <see cref="IMediaType"/> to delete</param>
        /// <remarks>Deleting a <see cref="IMediaType"/> will delete all the <see cref="IMedia"/> objects based on this <see cref="IMediaType"/></remarks>
        public void Delete(IEnumerable<IMediaType> mediaTypes)
        {
            var mediaTypeList = mediaTypes.ToList();
            foreach (var mediaType in mediaTypeList)
            {
                _mediaService.DeleteMediaOfType(mediaType.Id);
            }

            var repository = RepositoryResolver.ResolveByType<IMediaTypeRepository, IMediaType, int>(_unitOfWork);

            foreach (var mediaType in mediaTypeList)
            {
                repository.Delete(mediaType);
            }
            _unitOfWork.Commit();
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
    }
}