using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the Media Service, which is an easy access to operations involving <see cref="IMedia"/>
    /// </summary>
    public class MediaService : IMediaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MediaService() : this(new PetaPocoUnitOfWorkProvider())
        {
        }

        public MediaService(IUnitOfWorkProvider provider)
        {
            _unitOfWork = provider.GetUnitOfWork();
        }

        /// <summary>
        /// Gets an <see cref="IMedia"/> object by Id
        /// </summary>
        /// <param name="id">Id of the Content to retrieve</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetById(int id)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(_unitOfWork);
            return repository.Get(id);
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetChildren(int id)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(_unitOfWork);

            var query = Query<IMedia>.Builder.Where(x => x.ParentId == id);
            var medias = repository.GetByQuery(query);

            return medias;
        }

        /// <summary>
        /// Gets descendants of a <see cref="IMedia"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve descendants from</param>
        /// <returns>An Enumerable flat list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetDescendants(int id)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(_unitOfWork);
            
            var media = repository.Get(id);

            var query = Query<IMedia>.Builder.Where(x => x.Path.StartsWith(media.Path));
            var medias = repository.GetByQuery(query);

            return medias;
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects by the Id of the <see cref="IContentType"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetMediaOfMediaType(int id)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(_unitOfWork);

            var query = Query<IMedia>.Builder.Where(x => x.ContentTypeId == id);
            var medias = repository.GetByQuery(query);

            return medias;
        }

        /// <summary>
        /// Gets a collection of <see cref="IMedia"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetRootMedia()
        {
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(_unitOfWork);

            var query = Query<IMedia>.Builder.Where(x => x.ParentId == -1);
            var medias = repository.GetByQuery(query);

            return medias;
        }

        /// <summary>
        /// Gets a collection of an <see cref="IMedia"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetMediaInRecycleBin()
        {
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(_unitOfWork);

            var query = Query<IMedia>.Builder.Where(x => x.ParentId == -20);
            var medias = repository.GetByQuery(query);

            return medias;
        }

        /// <summary>
        /// Moves an <see cref="IMedia"/> object to a new location
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to move</param>
        /// <param name="parentId">Id of the Media's new Parent</param>
        /// <param name="userId">Id of the User moving the Media</param>
        public void Move(IMedia media, int parentId, int userId)
        {
            media.ParentId = parentId;
            Save(media, userId);
        }

        /// <summary>
        /// Deletes an <see cref="IMedia"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Media</param>
        public void MoveToRecycleBin(IMedia media, int userId)
        {
            //TODO If media item has children those should also be moved to the recycle bin as well
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(_unitOfWork);
            ((Core.Models.Media)media).ChangeTrashedState(true);
            repository.AddOrUpdate(media);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IMedia"/> that resides in the bin
        /// </summary>
        public void EmptyRecycleBin()
        {
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(_unitOfWork);

            var query = Query<IMedia>.Builder.Where(x => x.ParentId == -20);
            var contents = repository.GetByQuery(query);

            foreach (var content in contents)
            {
                repository.Delete(content);
            }
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes all media of specified type. All children of deleted media is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="mediaTypeId">Id of the <see cref="IMediaType"/></param>
        public void DeleteMediaOfType(int mediaTypeId)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(_unitOfWork);

            //NOTE What about media that has the contenttype as part of its composition?
            //The ContentType has to be removed from the composition somehow as it would otherwise break
            //Dbl.check+test that the ContentType's Id is removed from the ContentType2ContentType table
            var query = Query<IMedia>.Builder.Where(x => x.ContentTypeId == mediaTypeId);
            var contents = repository.GetByQuery(query);

            foreach (var content in contents)
            {
                ((Core.Models.Media)content).ChangeTrashedState(true);
                repository.AddOrUpdate(content);
            }

            _unitOfWork.Commit();
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
        public void Delete(IMedia media, int userId)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(_unitOfWork);
            repository.Delete(media);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Saves a single <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media">The <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Content</param>
        public void Save(IMedia media, int userId)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(_unitOfWork);
            repository.AddOrUpdate(media);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Saves a collection of <see cref="IMedia"/> objects
        /// </summary>
        /// <param name="medias">Collection of <see cref="IMedia"/> to save</param>
        /// <param name="userId">Id of the User saving the Content</param>
        public void Save(IEnumerable<IMedia> medias, int userId)
        {
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(_unitOfWork);
            foreach (var media in medias)
            {
                repository.AddOrUpdate(media);
            }
            _unitOfWork.Commit();
        }
    }
}