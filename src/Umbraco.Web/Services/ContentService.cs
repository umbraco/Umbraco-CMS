using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Web.Publishing;
using Content = Umbraco.Core.Models.Content;

namespace Umbraco.Web.Services
{
    public class ContentService : IContentService
    {
        private readonly IUnitOfWorkProvider _provider;
        private readonly IPublishingStrategy _publishingStrategy;

        public ContentService() : this(new PetaPocoUnitOfWorkProvider())
        {
        }

        public ContentService(IUnitOfWorkProvider provider) : this(provider, new PublishingStrategy())
        {
            
        }

        public ContentService(IUnitOfWorkProvider provider, IPublishingStrategy publishingStrategy)
        {
            _provider = provider;
            _publishingStrategy = publishingStrategy;
        }

        /// <summary>
        /// Creates an <see cref="IContent"/> object using the alias of the <see cref="IContentType"/>
        /// that this Content is based on.
        /// </summary>
        /// <param name="parentId">Id of Parent for content</param>
        /// <param name="contentTypeAlias">Alias of the <see cref="IContentType"/></param>
        /// <returns><see cref="IContent"/></returns>
        public IContent CreateContent(int parentId, string contentTypeAlias)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(unitOfWork);
            var query = Query<IContentType>.Builder.Where(x => x.Alias == contentTypeAlias);
            var contentTypes = repository.GetByQuery(query);

            if (!contentTypes.Any())
                throw new Exception(string.Format("No ContentType matching the passed in Alias: {0} was found", contentTypeAlias));

            var contentType = contentTypes.First();

            if (contentType == null)
                throw new Exception(string.Format("No ContentType matching the passed in Alias: {0} was found", contentTypeAlias));

            return new Content(parentId, contentType);
        }

        /// <summary>
        /// Gets an <see cref="IContent"/> object by Id
        /// </summary>
        /// <param name="id">Id of the Content to retrieve</param>
        /// <returns><see cref="IContent"/></returns>
        public IContent GetById(int id)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);
            return repository.Get(id);
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Level
        /// </summary>
        /// <param name="level">The level to retrieve Content from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetByLevel(int level)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);

            var query = Query<IContent>.Builder.Where(x => x.Level == level);
            var contents = repository.GetByQuery(query);

            return contents;
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetChildren(int id)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);

            var query = Query<IContent>.Builder.Where(x => x.ParentId == id);
            var contents = repository.GetByQuery(query);

            return contents;
        }

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects versions by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetVersions(int id)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);
            var versions = repository.GetAllVersions(id);
            return versions;
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetRootContent()
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);

            var query = Query<IContent>.Builder.Where(x => x.ParentId == -1);
            var contents = repository.GetByQuery(query);

            return contents;
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has an expiration date greater then today
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentForExpiration()
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);

            var query = Query<IContent>.Builder.Where(x => x.Published == true && x.ExpireDate != null && x.ExpireDate.Value <= DateTime.Now);
            var contents = repository.GetByQuery(query);

            return contents;
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has a release date greater then today
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentForRelease()
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);

            var query = Query<IContent>.Builder.Where(x => x.Published == true && x.ReleaseDate != null && x.ReleaseDate.Value <= DateTime.Now);
            var contents = repository.GetByQuery(query);

            return contents;
        }

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentInRecycleBin()
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);

            var query = Query<IContent>.Builder.Where(x => x.ParentId == -20);
            var contents = repository.GetByQuery(query);

            return contents;
        }

        /// <summary>
        /// Re-Publishes all Content
        /// </summary>
        /// <param name="userId">Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public bool RePublishAll(int userId)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);

            var list = new List<IContent>();

            var rootContent = GetRootContent();
            foreach (var content in rootContent)
            {
                list.AddRange(GetChildrenDeep(content.Id));
            }

            foreach (var item in list)
            {
                ((Content)item).ChangePublishedState(true);
                repository.AddOrUpdate(item);
            }

            unitOfWork.Commit();

            return _publishingStrategy.PublishWithChildren(list, userId);
        }

        /// <summary>
        /// Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public bool Publish(IContent content, int userId)
        {
            return SaveAndPublish(content, userId);
        }

        /// <summary>
        /// Publishes a <see cref="IContent"/> object and all its children
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish along with its children</param>
        /// <param name="userId">Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public bool PublishWithChildren(IContent content, int userId)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);

            var list = GetChildrenDeep(content.Id);
            list.Add(content);

            foreach (var item in list)
            {
                ((Content)item).ChangePublishedState(true);
                repository.AddOrUpdate(item);
            }

            unitOfWork.Commit();

            return _publishingStrategy.PublishWithChildren(list, userId);
        }

        /// <summary>
        /// UnPublishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Id of the User issueing the publishing</param>
        /// <returns>True if unpublishing succeeded, otherwise False</returns>
        public bool UnPublish(IContent content, int userId)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);

            ((Content)content).ChangePublishedState(false);
            repository.AddOrUpdate(content);
            unitOfWork.Commit();

            return _publishingStrategy.UnPublish(content, userId);
        }

        /// <summary>
        /// Gets a flat list of decendents of content from parent id
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private List<IContent> GetChildrenDeep(int parentId)
        {
            var list = new List<IContent>();
            var children = GetChildren(parentId);
            foreach (var child in children)
            {
                list.Add(child);
                list.AddRange(GetChildrenDeep(child.Id));
            }
            return list;
        }

        /// <summary>
        /// Saves and Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save and publish</param>
        /// <param name="userId">Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public bool SaveAndPublish(IContent content, int userId)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);
            
            ((Content)content).ChangePublishedState(true);
            repository.AddOrUpdate(content);
            unitOfWork.Commit();

            return _publishingStrategy.Publish(content, userId);
        }

        /// <summary>
        /// Saves a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save</param>
        /// <param name="userId">Id of the User saving the Content</param>
        public void Save(IContent content, int userId)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);
            repository.AddOrUpdate(content);
            unitOfWork.Commit();
        }

        /// <summary>
        /// Saves a collection of <see cref="IContent"/> objects
        /// </summary>
        /// <param name="contents">Collection of <see cref="IContent"/> to save</param>
        /// <param name="userId">Id of the User saving the Content</param>
        public void Save(IEnumerable<IContent> contents, int userId)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);
            foreach (var content in contents)
            {
                repository.AddOrUpdate(content);
            }
            unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes all content of specified type. All children of deleted content is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="contentTypeId">Id of the <see cref="IContentType"/></param>
        public void DeleteContentOfType(int contentTypeId)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);

            //NOTE What about content that has the contenttype as part of its composition?
            var query = Query<IContent>.Builder.Where(x => x.ContentTypeId == contentTypeId);
            var contents = repository.GetByQuery(query);

            foreach (var content in contents)
            {
                ((Content)content).ChangeTrashedState(true);
                repository.AddOrUpdate(content);
            }

            unitOfWork.Commit();
        }

        /// <summary>
        /// Permanently deletes an <see cref="IContent"/> object
        /// </summary>
        /// <remarks>Please note that this method will completely remove the Content from the database</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Content</param>
        public void Delete(IContent content, int userId)
        {
            //TODO This method should handle/react to errors when there is a constraint issue with the content being deleted
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);
            repository.Delete(content);
            unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes an <see cref="IContent"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <remarks>Move an item to the Recycle Bin will result in the item being unpublished</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Id of the User deleting the Content</param>
        public void MoveToRecycleBin(IContent content, int userId)
        {
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);
            ((Content)content).ChangeTrashedState(true);
            repository.AddOrUpdate(content);
            unitOfWork.Commit();
        }

        /// <summary>
        /// Moves an <see cref="IContent"/> object to a new location
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to move</param>
        /// <param name="parentId">Id of the Content's new Parent</param>
        /// <param name="userId">Id of the User moving the Content</param>
        public void Move(IContent content, int parentId, int userId)
        {
            content.ParentId = parentId;
            SaveAndPublish(content, userId);
        }

        /// <summary>
        /// Copies an <see cref="IContent"/> object by creating a new Content object of the same type and copies all data from the current 
        /// to the new copy which is returned.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to copy</param>
        /// <param name="parentId">Id of the Content's new Parent</param>
        /// <param name="userId">Id of the User copying the Content</param>
        /// <returns>The newly created <see cref="IContent"/> object</returns>
        public IContent Copy(IContent content, int parentId, int userId)
        {
            var copy = ((Content) content).Clone();
            copy.ParentId = parentId;

            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);

            repository.AddOrUpdate(copy);
            unitOfWork.Commit();
            
            return copy;
        }

        /// <summary>
        /// Sends an <see cref="IContent"/> to Publication, which executes handlers and events for the 'Send to Publication' action.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to send to publication</param>
        /// <param name="userId">Id of the User issueing the send to publication</param>
        /// <returns>True if sending publication was succesfull otherwise false</returns>
        public bool SendToPublication(IContent content, int userId)
        {
            //TODO Implement something similar to this
            /*SendToPublishEventArgs e = new SendToPublishEventArgs();
            FireBeforeSendToPublish(e);
            if (!e.Cancel)
            {
                global::umbraco.BusinessLogic.Actions.Action.RunActionHandlers(content, ActionToPublish.Instance);

                FireAfterSendToPublish(e);
                return true;
            }

            return false;*/
            return false;
        }

        /// <summary>
        /// Rollback an <see cref="IContent"/> object to a previous version.
        /// This will create a new version, which is a copy of all the old data.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/>being rolled back</param>
        /// <param name="versionId">Id of the version to rollback to</param>
        /// <param name="userId">Id of the User issueing the rollback of the Content</param>
        /// <returns>The newly created <see cref="IContent"/> object</returns>
        public IContent Rollback(int id, Guid versionId, int userId)
        {
            //TODO Need to test if this actually works
            var unitOfWork = _provider.GetUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(unitOfWork);
            var content = repository.GetByVersion(id, versionId);
            
            repository.AddOrUpdate(content);
            unitOfWork.Commit();

            return content;
        }
    }
}