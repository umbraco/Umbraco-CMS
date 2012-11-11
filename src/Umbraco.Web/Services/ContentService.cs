using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;

namespace Umbraco.Web.Services
{
    /// <summary>
    /// Represents the Content Service, which is an easy access to operations involving <see cref="IContent"/>
    /// </summary>
    public class ContentService : IContentService
    {
        private readonly IPublishingStrategy _publishingStrategy;
        private readonly IUnitOfWork _unitOfWork;

        public ContentService(IUnitOfWorkProvider provider, IPublishingStrategy publishingStrategy)
        {
            _publishingStrategy = publishingStrategy;
            _unitOfWork = provider.GetUnitOfWork();
        }

        /// <summary>
        /// Creates an <see cref="IContent"/> object using the alias of the <see cref="IContentType"/>
        /// that this Content is based on.
        /// </summary>
        /// <param name="parentId">Id of Parent for content</param>
        /// <param name="contentTypeAlias">Alias of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional id of the user creating the content</param>
        /// <returns><see cref="IContent"/></returns>
        public IContent CreateContent(int parentId, string contentTypeAlias, int userId = -1)
        {
            var repository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(_unitOfWork);
            var query = Query<IContentType>.Builder.Where(x => x.Alias == contentTypeAlias);
            var contentTypes = repository.GetByQuery(query);

            if (!contentTypes.Any())
                throw new Exception(string.Format("No ContentType matching the passed in Alias: '{0}' was found", contentTypeAlias));

            var contentType = contentTypes.First();

            if (contentType == null)
                throw new Exception(string.Format("ContentType matching the passed in Alias: '{0}' was null", contentTypeAlias));

            var content = new Content(parentId, contentType);
            SetUser(content, userId);
            SetWriter(content, userId);
            return content;
        }

        /// <summary>
        /// Gets an <see cref="IContent"/> object by Id
        /// </summary>
        /// <param name="id">Id of the Content to retrieve</param>
        /// <returns><see cref="IContent"/></returns>
        public IContent GetById(int id)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);
            return repository.Get(id);
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by the Id of the <see cref="IContentType"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentOfContentType(int id)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            var query = Query<IContent>.Builder.Where(x => x.ContentTypeId == id);
            var contents = repository.GetByQuery(query);

            return contents;
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Level
        /// </summary>
        /// <param name="level">The level to retrieve Content from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetByLevel(int level)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

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
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

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
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);
            var versions = repository.GetAllVersions(id);
            return versions;
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetRootContent()
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            var query = Query<IContent>.Builder.Where(x => x.ParentId == -1);
            var contents = repository.GetByQuery(query);

            return contents;
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has an expiration date less than or equal to today.
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentForExpiration()
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            var query = Query<IContent>.Builder.Where(x => x.Published == true && x.ExpireDate <= DateTime.UtcNow);
            var contents = repository.GetByQuery(query);

            return contents;
        }

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has a release date less than or equal to today.
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentForRelease()
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            var query = Query<IContent>.Builder.Where(x => x.Published == false && x.ReleaseDate <= DateTime.UtcNow);
            var contents = repository.GetByQuery(query);

            return contents;
        }

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetContentInRecycleBin()
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            var query = Query<IContent>.Builder.Where(x => x.ParentId == -20);
            var contents = repository.GetByQuery(query);

            return contents;
        }

        /// <summary>
        /// Re-Publishes all Content
        /// </summary>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public bool RePublishAll(int userId = -1)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            var list = new List<IContent>();

            //Consider creating a Path query instead of recursive method:
            //var query = Query<IContent>.Builder.Where(x => x.Path.StartsWith("-1"));

            var rootContent = GetRootContent();
            foreach (var content in rootContent)
            {
                if(content.IsValid())
                {
                    list.Add(content);
                    list.AddRange(GetChildrenDeep(content.Id));
                }
            }

            //Publish and then update the database with new status
            var published = _publishingStrategy.PublishWithChildren(list, userId);
            if (published)
            {
                //Only loop through content where the Published property has been updated
                foreach (var item in list.Where(x => ((ICanBeDirty)x).IsPropertyDirty("Published")))
                {
                    SetWriter(item, userId);
                    repository.AddOrUpdate(item);
                }

                _unitOfWork.Commit();

                //TODO Change this so we can avoid a depencency to the horrible library method / umbraco.content (singleton) class.
                //global::umbraco.library.RefreshContent();
            }

            return published;
        }

        /// <summary>
        /// Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public bool Publish(IContent content, int userId = -1)
        {
            return SaveAndPublish(content, userId);
        }

        /// <summary>
        /// Publishes a <see cref="IContent"/> object and all its children
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish along with its children</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public bool PublishWithChildren(IContent content, int userId = -1)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            //Check if parent is published (although not if its a root node) - if parent isn't published this Content cannot be published
            if (content.ParentId != -1 && content.ParentId != -20 && !GetById(content.ParentId).Published)
            {
                LogHelper.Info<ContentService>(
                    string.Format("Content '{0}' with Id '{1}' could not be published because its parent is not published.",
                                  content.Name, content.Id));
                return false;
            }

            //Content contains invalid property values and can therefore not be published - fire event?
            if (!content.IsValid())
            {
                LogHelper.Info<ContentService>(
                    string.Format("Content '{0}' with Id '{1}' could not be published because of invalid properties.",
                                  content.Name, content.Id));
                return false;
            }

            //Consider creating a Path query instead of recursive method:
            //var query = Query<IContent>.Builder.Where(x => x.Path.StartsWith(content.Path));

            var list = new List<IContent>();
            list.Add(content);
            list.AddRange(GetChildrenDeep(content.Id));

            //Publish and then update the database with new status
            var published = _publishingStrategy.PublishWithChildren(list, userId);
            if (published)
            {
                //Only loop through content where the Published property has been updated
                foreach (var item in list.Where(x => ((ICanBeDirty)x).IsPropertyDirty("Published")))
                {
                    SetWriter(item, userId);
                    repository.AddOrUpdate(item);
                }

                _unitOfWork.Commit();

                //TODO Change this so we can avoid a depencency to the horrible library method / umbraco.content (singleton) class.
                //TODO Need to investigate if it will also update the cache for children of the Content object
                //global::umbraco.library.UpdateDocumentCache(content.Id);
            }

            return published;
        }

        /// <summary>
        /// UnPublishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if unpublishing succeeded, otherwise False</returns>
        public bool UnPublish(IContent content, int userId = -1)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            //Look for children and unpublish them if any exists, otherwise just unpublish the passed in Content.
            var children = GetChildrenDeep(content.Id);
            var hasChildren = children.Any();
            
            if(hasChildren)
                children.Add(content);

            var unpublished = hasChildren
                                  ? _publishingStrategy.UnPublish(children, userId)
                                  : _publishingStrategy.UnPublish(content, userId);

            if (unpublished)
            {
                repository.AddOrUpdate(content);

                if (hasChildren)
                {
                    foreach (var child in children)
                    {
                        SetWriter(child, userId);
                        repository.AddOrUpdate(child);
                    }
                }

                _unitOfWork.Commit();

                //TODO Change this so we can avoid a depencency to the horrible library method / umbraco.content class.
                //global::umbraco.library.UnPublishSingleNode(content.Id);
            }

            return unpublished;
        }

        /// <summary>
        /// Gets a flat list of decendents of content from parent id
        /// </summary>
        /// <remarks>
        /// Only contains valid <see cref="IContent"/> objects, which means
        /// that everything in the returned list can be published.
        /// If an invalid <see cref="IContent"/> object is found it will not
        /// be added to the list neither will its children.
        /// </remarks>
        /// <param name="parentId">Id of the parent to retrieve children from</param>
        /// <returns>A list of valid <see cref="IContent"/> that can be published</returns>
        private List<IContent> GetChildrenDeep(int parentId)
        {
            var list = new List<IContent>();
            var children = GetChildren(parentId);
            foreach (var child in children)
            {
                if (child.IsValid())
                {
                    list.Add(child);
                    list.AddRange(GetChildrenDeep(child.Id));
                }
            }
            return list;
        }

        /// <summary>
        /// Saves and Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save and publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public bool SaveAndPublish(IContent content, int userId = -1)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            //Check if parent is published (although not if its a root node) - if parent isn't published this Content cannot be published
            if (content.ParentId != -1 && content.ParentId != -20 && GetById(content.ParentId).Published == false)
            {
                LogHelper.Info<ContentService>(
                    string.Format("Content '{0}' with Id '{1}' could not be published because its parent is not published.",
                                  content.Name, content.Id));
                return false;
            }

            //Content contains invalid property values and can therefore not be published - fire event?
            if (!content.IsValid())
            {
                LogHelper.Info<ContentService>(
                    string.Format("Content '{0}' with Id '{1}' could not be published because of invalid properties.",
                                  content.Name, content.Id));
                return false;
            }

            //Publish and then update the database with new status
            bool published = _publishingStrategy.Publish(content, userId);
            if (published)
            {
                SetWriter(content, userId);
                repository.AddOrUpdate(content);
                _unitOfWork.Commit();

                //TODO Change this so we can avoid a depencency to the horrible library method / umbraco.content (singleton) class.
                //global::umbraco.library.UpdateDocumentCache(content.Id);
            }

            return published;
        }

        /// <summary>
        /// Saves a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        public void Save(IContent content, int userId = -1)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            SetWriter(content, userId);

            repository.AddOrUpdate(content);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Saves a collection of <see cref="IContent"/> objects
        /// </summary>
        /// <param name="contents">Collection of <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        public void Save(IEnumerable<IContent> contents, int userId = -1)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);
            foreach (var content in contents)
            {
                SetWriter(content, userId);
                repository.AddOrUpdate(content);
            }
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Deletes all content of specified type. All children of deleted content is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="contentTypeId">Id of the <see cref="IContentType"/></param>
        public void DeleteContentOfType(int contentTypeId)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            //NOTE What about content that has the contenttype as part of its composition?
            var query = Query<IContent>.Builder.Where(x => x.ContentTypeId == contentTypeId);
            var contents = repository.GetByQuery(query);

            foreach (var content in contents)
            {
                ((Content)content).ChangeTrashedState(true);
                repository.AddOrUpdate(content);
            }

            _unitOfWork.Commit();
        }

        /// <summary>
        /// Permanently deletes an <see cref="IContent"/> object
        /// </summary>
        /// <remarks>Please note that this method will completely remove the Content from the database</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
        public void Delete(IContent content, int userId = -1)
        {
            //TODO Ensure that content is unpublished when deleted
            //TODO This method should handle/react to errors when there is a constraint issue with the content being deleted
            //TODO Children should either be deleted or moved to the recycle bin
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);
            SetWriter(content, userId);
            repository.Delete(content);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Permanently deletes versions from an <see cref="IContent"/> object prior to a specific date.
        /// </summary>
        /// <param name="content">Id of the <see cref="IContent"/> object to delete versions from</param>
        /// <param name="versionDate">Latest version date</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        public void Delete(IContent content, DateTime versionDate, int userId = -1)
        {
            Delete(content.Id, versionDate, userId);
        }

        /// <summary>
        /// Permanently deletes a specific version from an <see cref="IContent"/> object.
        /// </summary>
        /// <param name="content">Id of the <see cref="IContent"/> object to delete a version from</param>
        /// <param name="versionId">Id of the version to delete</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        public void Delete(IContent content, Guid versionId, int userId = -1)
        {
            Delete(content.Id, versionId, userId);
        }

        /// <summary>
        /// Permanently deletes versions from an <see cref="IContent"/> object prior to a specific date.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> object to delete versions from</param>
        /// <param name="versionDate">Latest version date</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        public void Delete(int id, DateTime versionDate, int userId = -1)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);
            repository.Delete(id, versionDate);
        }

        /// <summary>
        /// Permanently deletes a specific version from an <see cref="IContent"/> object.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> object to delete a version from</param>
        /// <param name="versionId">Id of the version to delete</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        public void Delete(int id, Guid versionId, int userId = -1)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);
            repository.Delete(id, versionId);
        }

        /// <summary>
        /// Deletes an <see cref="IContent"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <remarks>Move an item to the Recycle Bin will result in the item being unpublished</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
        public void MoveToRecycleBin(IContent content, int userId = -1)
        {
            //TODO If content item has children those should also be moved to the recycle bin
            //TODO Unpublish deleted content + children
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);
            SetWriter(content, userId);
            content.ChangeTrashedState(true);
            repository.AddOrUpdate(content);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Moves an <see cref="IContent"/> object to a new location by changing its parent id.
        /// </summary>
        /// <remarks>
        /// If the <see cref="IContent"/> object is already published it will be
        /// published after being moved to its new location. Otherwise it'll just
        /// be saved with a new parent id.
        /// </remarks>
        /// <param name="content">The <see cref="IContent"/> to move</param>
        /// <param name="parentId">Id of the Content's new Parent</param>
        /// <param name="userId">Optional Id of the User moving the Content</param>
        public void Move(IContent content, int parentId, int userId = -1)
        {
            SetWriter(content, userId);

            //If Content is being moved away from Recycle Bin, its state should be un-trashed
            if(content.Trashed && parentId != -20)
            {
                content.ChangeTrashedState(false, parentId);
            }
            else
            {
                content.ParentId = parentId;
            }

            //If Content is published, it should be (re)published from its new location
            if(content.Published)
            {
                SaveAndPublish(content, userId);
            }
            else
            {
                Save(content, userId);
            }
        }

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IContent"/> that resides in the bin
        /// </summary>
        public void EmptyRecycleBin()
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            var query = Query<IContent>.Builder.Where(x => x.ParentId == -20);
            var contents = repository.GetByQuery(query);

            foreach (var content in contents)
            {
                repository.Delete(content);
            }
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Copies an <see cref="IContent"/> object by creating a new Content object of the same type and copies all data from the current 
        /// to the new copy which is returned.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to copy</param>
        /// <param name="parentId">Id of the Content's new Parent</param>
        /// <param name="userId">Optional Id of the User copying the Content</param>
        /// <returns>The newly created <see cref="IContent"/> object</returns>
        public IContent Copy(IContent content, int parentId, int userId = -1)
        {
            var copy = ((Content) content).Clone();
            copy.ParentId = parentId;
            copy.Name = copy.Name + " (1)";

            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);

            SetWriter(content, userId);

            repository.AddOrUpdate(copy);
            _unitOfWork.Commit();
            
            return copy;
        }

        /// <summary>
        /// Sends an <see cref="IContent"/> to Publication, which executes handlers and events for the 'Send to Publication' action.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to send to publication</param>
        /// <param name="userId">Optional Id of the User issueing the send to publication</param>
        /// <returns>True if sending publication was succesfull otherwise false</returns>
        public bool SendToPublication(IContent content, int userId = -1)
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
        /// <remarks>
        /// The way data is stored actually only allows us to rollback on properties
        /// and not data like Name and Alias of the Content.
        /// </remarks>
        /// <param name="id">Id of the <see cref="IContent"/>being rolled back</param>
        /// <param name="versionId">Id of the version to rollback to</param>
        /// <param name="userId">Optional Id of the User issueing the rollback of the Content</param>
        /// <returns>The newly created <see cref="IContent"/> object</returns>
        public IContent Rollback(int id, Guid versionId, int userId = -1)
        {
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(_unitOfWork);
            var content = repository.GetByVersion(id, versionId);
            
            SetUser(content, userId);
            SetWriter(content, userId);

            repository.AddOrUpdate(content);
            _unitOfWork.Commit();

            return content;
        }

        /// <summary>
        /// Updates a content object with the User (id), who created the content.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> object to update</param>
        /// <param name="userId">Optional Id of the User</param>
        private void SetUser(IContent content, int userId)
        {
            if(userId > -1)
            {
                //If a user id was passed in we use that
                content.CreatorId = userId;
            }
            else
            {
                //Otherwise we default to Admin user, which should always exist (almost always)
                content.CreatorId = 0;
            }
        }

        /// <summary>
        /// Updates a content object with a Writer (user id), who updated the content.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> object to update</param>
        /// <param name="userId">Optional Id of the Writer</param>
        private void SetWriter(IContent content, int userId)
        {
            if (userId > -1)
            {
                //If a user id was passed in we use that
                content.WriterId = userId;
            }
            else
            {
                //Otherwise we default to Admin user, which should always exist (almost always)
                content.WriterId = 0;
            }
        }

        //TODO Add method to remove versions from Content
        //TODO Add method to remove versions from all Content - parameters to select date-interval, ea. remove versions older then.
    }
}