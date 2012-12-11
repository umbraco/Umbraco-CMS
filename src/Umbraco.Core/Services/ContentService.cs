using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Auditing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;

namespace Umbraco.Core.Services
{
	/// <summary>
	/// Represents the Content Service, which is an easy access to operations involving <see cref="IContent"/>
	/// </summary>
	public class ContentService : IContentService
	{
		private readonly IDatabaseUnitOfWorkProvider _uowProvider;
		private readonly IPublishingStrategy _publishingStrategy;
		private readonly IUserService _userService;
		private HttpContextBase _httpContext;

		public ContentService(IDatabaseUnitOfWorkProvider provider, IPublishingStrategy publishingStrategy)
		{
			_uowProvider = provider;
			_publishingStrategy = publishingStrategy;
		}

		internal ContentService(IDatabaseUnitOfWorkProvider provider, IPublishingStrategy publishingStrategy, IUserService userService)
		{
			_publishingStrategy = publishingStrategy;
			_userService = userService;
			_uowProvider = provider;
		}

		//TODO Add GetLatestUnpublishedVersions(int id){}

		/// <summary>
		/// Creates an <see cref="IContent"/> object using the alias of the <see cref="IContentType"/>
		/// that this Content is based on.
		/// </summary>
		/// <param name="parentId">Id of Parent for the new Content</param>
		/// <param name="contentTypeAlias">Alias of the <see cref="IContentType"/></param>
		/// <param name="userId">Optional id of the user creating the content</param>
		/// <returns><see cref="IContent"/></returns>
		public IContent CreateContent(int parentId, string contentTypeAlias, int userId = -1)
		{
			var uow = _uowProvider.GetUnitOfWork();
			var repository = RepositoryResolver.Current.Factory.CreateContentTypeRepository(uow);
			var query = Query<IContentType>.Builder.Where(x => x.Alias == contentTypeAlias);
			var contentTypes = repository.GetByQuery(query);

			if (!contentTypes.Any())
				throw new Exception(string.Format("No ContentType matching the passed in Alias: '{0}' was found", contentTypeAlias));

			var contentType = contentTypes.First();

			if (contentType == null)
				throw new Exception(string.Format("ContentType matching the passed in Alias: '{0}' was null", contentTypeAlias));

			IContent content = null;

			var e = new NewEventArgs { Alias = contentTypeAlias, ParentId = parentId };
			if (Creating != null)
				Creating(content, e);

			if (!e.Cancel)
			{
				content = new Content(parentId, contentType);
				SetUser(content, userId);
				SetWriter(content, userId);

				if (Created != null)
					Created(content, e);

				Audit.Add(AuditTypes.New, "", content.CreatorId, content.Id);
			}

			return content;
		}

		/// <summary>
		/// Gets an <see cref="IContent"/> object by Id
		/// </summary>
		/// <param name="id">Id of the Content to retrieve</param>
		/// <returns><see cref="IContent"/></returns>
		public IContent GetById(int id)
		{
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				return repository.Get(id);
			}

		}

		/// <summary>
		/// Gets an <see cref="IContent"/> object by its 'UniqueId'
		/// </summary>
		/// <param name="key">Guid key of the Content to retrieve</param>
		/// <returns><see cref="IContent"/></returns>
		public IContent GetById(Guid key)
		{
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.Key == key);
				var contents = repository.GetByQuery(query);
				return contents.SingleOrDefault();
			}
		}


		/// <summary>
		/// Gets a collection of <see cref="IContent"/> objects by the Id of the <see cref="IContentType"/>
		/// </summary>
		/// <param name="id">Id of the <see cref="IContentType"/></param>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetContentOfContentType(int id)
		{
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.ContentTypeId == id);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IContent"/> objects by Level
		/// </summary>
		/// <param name="level">The level to retrieve Content from</param>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetByLevel(int level)
		{
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.Level == level);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IContent"/> objects by Parent Id
		/// </summary>
		/// <param name="id">Id of the Parent to retrieve Children from</param>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetChildren(int id)
		{
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.ParentId == id);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

		/// <summary>
		/// Gets a collection of an <see cref="IContent"/> objects versions by Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetVersions(int id)
		{
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var versions = repository.GetAllVersions(id);
				return versions;
			}

		}

		/// <summary>
		/// Gets a collection of <see cref="IContent"/> objects, which reside at the first level / root
		/// </summary>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetRootContent()
		{
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.ParentId == -1);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IContent"/> objects, which has an expiration date less than or equal to today.
		/// </summary>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetContentForExpiration()
		{
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.Published == true && x.ExpireDate <= DateTime.UtcNow);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IContent"/> objects, which has a release date less than or equal to today.
		/// </summary>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetContentForRelease()
		{
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.Published == false && x.ReleaseDate <= DateTime.UtcNow);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

		/// <summary>
		/// Gets a collection of an <see cref="IContent"/> objects, which resides in the Recycle Bin
		/// </summary>
		/// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
		public IEnumerable<IContent> GetContentInRecycleBin()
		{
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				var query = Query<IContent>.Builder.Where(x => x.ParentId == -20);
				var contents = repository.GetByQuery(query);

				return contents;
			}
		}

		/// <summary>
		/// Re-Publishes all Content
		/// </summary>
		/// <param name="userId">Optional Id of the User issueing the publishing</param>
		/// <returns>True if publishing succeeded, otherwise False</returns>
		public bool RePublishAll(int userId = -1)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
			{
				var list = new List<IContent>();

				//Consider creating a Path query instead of recursive method:
				//var query = Query<IContent>.Builder.Where(x => x.Path.StartsWith("-1"));

				var rootContent = GetRootContent();
				foreach (var content in rootContent)
				{
					if (content.IsValid())
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

					uow.Commit();

					//TODO Change this so we can avoid a depencency to the horrible library method / umbraco.content (singleton) class.
					//global::umbraco.library.RefreshContent();

					Audit.Add(AuditTypes.Publish, "RePublish All performed by user", userId == -1 ? 0 : userId, -1);
				}

				return published;
			}
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
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
			{
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

					uow.Commit();

					//TODO Change this so we can avoid a depencency to the horrible library method / umbraco.content (singleton) class.
					//TODO Need to investigate if it will also update the cache for children of the Content object
					//global::umbraco.library.UpdateDocumentCache(content.Id);

					Audit.Add(AuditTypes.Publish, "Publish with Children performed by user", userId == -1 ? 0 : userId, content.Id);
				}

				return published;
			}
		}

		/// <summary>
		/// UnPublishes a single <see cref="IContent"/> object
		/// </summary>
		/// <param name="content">The <see cref="IContent"/> to publish</param>
		/// <param name="userId">Optional Id of the User issueing the publishing</param>
		/// <returns>True if unpublishing succeeded, otherwise False</returns>
		public bool UnPublish(IContent content, int userId = -1)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
			{
				//Look for children and unpublish them if any exists, otherwise just unpublish the passed in Content.
				var children = GetChildrenDeep(content.Id);
				var hasChildren = children.Any();

				if (hasChildren)
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

					uow.Commit();

					//TODO Change this so we can avoid a depencency to the horrible library method / umbraco.content class.
					//global::umbraco.library.UnPublishSingleNode(content.Id);

					Audit.Add(AuditTypes.Publish, "UnPublish performed by user", userId == -1 ? 0 : userId, content.Id);
				}

				return unpublished;
			}
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
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
			{
				var e = new SaveEventArgs();
				if (Saving != null)
					Saving(content, e);

				if (!e.Cancel)
				{

					//Check if parent is published (although not if its a root node) - if parent isn't published this Content cannot be published
					if (content.ParentId != -1 && content.ParentId != -20 && GetById(content.ParentId).Published == false)
					{
						LogHelper.Info<ContentService>(
							string.Format(
								"Content '{0}' with Id '{1}' could not be published because its parent is not published.",
								content.Name, content.Id));
						return false;
					}

					//Content contains invalid property values and can therefore not be published - fire event?
					if (!content.IsValid())
					{
						LogHelper.Info<ContentService>(
							string.Format(
								"Content '{0}' with Id '{1}' could not be published because of invalid properties.",
								content.Name, content.Id));
						return false;
					}

					//Publish and then update the database with new status
					bool published = _publishingStrategy.Publish(content, userId);
					if (published)
					{
						SetWriter(content, userId);
						repository.AddOrUpdate(content);
						uow.Commit();

						//TODO Change this so we can avoid a depencency to the horrible library method / umbraco.content (singleton) class.
						//global::umbraco.library.UpdateDocumentCache(content.Id);
					}

					if (Saved != null)
						Saved(content, e);

					Audit.Add(AuditTypes.Publish, "Save and Publish performed by user", userId == -1 ? 0 : userId, content.Id);

					return published;
				}


				return false;
			}
		}

		/// <summary>
		/// Saves a single <see cref="IContent"/> object
		/// </summary>
		/// <param name="content">The <see cref="IContent"/> to save</param>
		/// <param name="userId">Optional Id of the User saving the Content</param>
		public void Save(IContent content, int userId = -1)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
			{
				var e = new SaveEventArgs();
				if (Saving != null)
					Saving(content, e);

				if (!e.Cancel)
				{

					SetWriter(content, userId);
					content.ChangePublishedState(false);
					repository.AddOrUpdate(content);
					uow.Commit();

					if (Saved != null)
						Saved(content, e);

					Audit.Add(AuditTypes.Save, "Save Content performed by user", userId == -1 ? 0 : userId, content.Id);
				}
			}
		}

		/// <summary>
		/// Saves a collection of <see cref="IContent"/> objects.
		/// </summary>
		/// <remarks>
		/// If the collection of content contains new objects that references eachother by Id or ParentId,
		/// then use the overload Save method with a collection of Lazy <see cref="IContent"/>.
		/// </remarks>
		/// <param name="contents">Collection of <see cref="IContent"/> to save</param>
		/// <param name="userId">Optional Id of the User saving the Content</param>
		public void Save(IEnumerable<IContent> contents, int userId = -1)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
			{
				var containsNew = contents.Any(x => x.HasIdentity == false);

				var e = new SaveEventArgs();
				if (Saving != null)
					Saving(contents, e);

				if (!e.Cancel)
				{
					if (containsNew)
					{
						foreach (var content in contents)
						{
							SetWriter(content, userId);
							content.ChangePublishedState(false);
							repository.AddOrUpdate(content);
							uow.Commit();
						}
					}
					else
					{
						foreach (var content in contents)
						{
							if (Saving != null)
								Saving(content, e);

							SetWriter(content, userId);
							repository.AddOrUpdate(content);
						}
						uow.Commit();
					}

					if (Saved != null)
						Saved(contents, e);

					Audit.Add(AuditTypes.Save, "Bulk Save content performed by user", userId == -1 ? 0 : userId, -1);
				}
			}
		}

		/// <summary>
		/// Saves a collection of lazy loaded <see cref="IContent"/> objects.
		/// </summary>
		/// <remarks>
		/// This method ensures that Content is saved lazily, so a new graph of <see cref="IContent"/>
		/// objects can be saved in bulk. But note that objects are saved one at a time to ensure Ids.
		/// </remarks>
		/// <param name="contents">Collection of Lazy <see cref="IContent"/> to save</param>
		/// <param name="userId">Optional Id of the User saving the Content</param>
		public void Save(IEnumerable<Lazy<IContent>> contents, int userId = -1)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
			{
				var e = new SaveEventArgs();
				if (Saving != null)
					Saving(contents, e);

				if (!e.Cancel)
				{
					foreach (var content in contents)
					{
						SetWriter(content.Value, userId);
						content.Value.ChangePublishedState(false);
						repository.AddOrUpdate(content.Value);
						uow.Commit();
					}

					if (Saved != null)
						Saved(contents, e);

					Audit.Add(AuditTypes.Save, "Bulk Save (lazy) content performed by user", userId == -1 ? 0 : userId, -1);
				}
			}
		}

		/// <summary>
		/// Deletes all content of specified type. All children of deleted content is moved to Recycle Bin.
		/// </summary>
		/// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
		/// <param name="contentTypeId">Id of the <see cref="IContentType"/></param>
		/// <param name="userId">Optional Id of the user issueing the delete operation</param>
		public void DeleteContentOfType(int contentTypeId, int userId = -1)
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
			{
				//NOTE What about content that has the contenttype as part of its composition?
				var query = Query<IContent>.Builder.Where(x => x.ContentTypeId == contentTypeId);
				var contents = repository.GetByQuery(query);

				var e = new DeleteEventArgs { Id = contentTypeId };
				if (Deleting != null)
					Deleting(contents, e);

				if (!e.Cancel)
				{
					foreach (var content in contents)
					{
						((Content)content).ChangeTrashedState(true);
						repository.AddOrUpdate(content);
					}

					uow.Commit();

					if (Deleted != null)
						Deleted(contents, e);

					Audit.Add(AuditTypes.Delete, string.Format("Delete Content of Type {0} performed by user", contentTypeId), userId == -1 ? 0 : userId, -1);
				}
			}
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

			var e = new DeleteEventArgs { Id = content.Id };
			if (Deleting != null)
				Deleting(content, e);

			if (!e.Cancel)
			{
				var uow = _uowProvider.GetUnitOfWork();
				using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
				{
					SetWriter(content, userId);
					repository.Delete(content);
					uow.Commit();

					if (Deleted != null)
						Deleted(content, e);

					Audit.Add(AuditTypes.Delete, "Delete Content performed by user", userId == -1 ? 0 : userId, content.Id);
				}

			}
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
		/// Permanently deletes specific version(s) from an <see cref="IContent"/> object.
		/// </summary>
		/// <param name="content">Id of the <see cref="IContent"/> object to delete a version from</param>
		/// <param name="versionId">Id of the version to delete</param>
		/// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
		/// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
		public void Delete(IContent content, Guid versionId, bool deletePriorVersions, int userId = -1)
		{
			Delete(content.Id, versionId, deletePriorVersions, userId);
		}

		/// <summary>
		/// Permanently deletes versions from an <see cref="IContent"/> object prior to a specific date.
		/// </summary>
		/// <param name="id">Id of the <see cref="IContent"/> object to delete versions from</param>
		/// <param name="versionDate">Latest version date</param>
		/// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
		public void Delete(int id, DateTime versionDate, int userId = -1)
		{
			var e = new DeleteEventArgs { Id = id };
			if (Deleting != null)
				Deleting(versionDate, e);

			if (!e.Cancel)
			{
				using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
				{
					repository.Delete(id, versionDate);

					if (Deleted != null)
						Deleted(versionDate, e);

					Audit.Add(AuditTypes.Delete, "Delete Content by version date performed by user", userId == -1 ? 0 : userId, -1);
				}

			}
		}

		/// <summary>
		/// Permanently deletes specific version(s) from an <see cref="IContent"/> object.
		/// </summary>
		/// <param name="id">Id of the <see cref="IContent"/> object to delete a version from</param>
		/// <param name="versionId">Id of the version to delete</param>
		/// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
		/// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
		public void Delete(int id, Guid versionId, bool deletePriorVersions, int userId = -1)
		{
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(_uowProvider.GetUnitOfWork()))
			{
				if (deletePriorVersions)
				{
					var content = repository.GetByVersion(id, versionId);
					Delete(id, content.UpdateDate, userId);
				}

				var e = new DeleteEventArgs { Id = id };
				if (Deleting != null)
					Deleting(versionId, e);

				if (!e.Cancel)
				{
					repository.Delete(id, versionId);

					if (Deleted != null)
						Deleted(versionId, e);

					Audit.Add(AuditTypes.Delete, "Delete Content by version performed by user", userId == -1 ? 0 : userId, -1);
				}
			}
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
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
			{

				var e = new MoveEventArgs { ParentId = -20 };
				if (Trashing != null)
					Trashing(content, e);

				if (!e.Cancel)
				{
					SetWriter(content, userId);
					content.ChangeTrashedState(true);
					repository.AddOrUpdate(content);
					uow.Commit();

					if (Trashed != null)
						Trashed(content, e);

					Audit.Add(AuditTypes.Move, "Move Content to Recycle Bin performed by user", userId == -1 ? 0 : userId, content.Id);
				}
			}
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
			var e = new MoveEventArgs { ParentId = parentId };
			if (Moving != null)
				Moving(content, e);

			if (!e.Cancel)
			{
				SetWriter(content, userId);

				//If Content is being moved away from Recycle Bin, its state should be un-trashed
				if (content.Trashed && parentId != -20)
				{
					content.ChangeTrashedState(false, parentId);
				}
				else
				{
					content.ParentId = parentId;
				}

				//If Content is published, it should be (re)published from its new location
				if (content.Published)
				{
					SaveAndPublish(content, userId);
				}
				else
				{
					Save(content, userId);
				}

				if (Moved != null)
					Moved(content, e);

				Audit.Add(AuditTypes.Move, "Move Content performed by user", userId == -1 ? 0 : userId, content.Id);
			}
		}

		/// <summary>
		/// Empties the Recycle Bin by deleting all <see cref="IContent"/> that resides in the bin
		/// </summary>
		public void EmptyRecycleBin()
		{
			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
			{
				var query = Query<IContent>.Builder.Where(x => x.ParentId == -20);
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
		/// Copies an <see cref="IContent"/> object by creating a new Content object of the same type and copies all data from the current 
		/// to the new copy which is returned.
		/// </summary>
		/// <param name="content">The <see cref="IContent"/> to copy</param>
		/// <param name="parentId">Id of the Content's new Parent</param>
		/// <param name="userId">Optional Id of the User copying the Content</param>
		/// <returns>The newly created <see cref="IContent"/> object</returns>
		public IContent Copy(IContent content, int parentId, int userId = -1)
		{
			var e = new CopyEventArgs { ParentId = parentId };
			if (Copying != null)
				Copying(content, e);

			IContent copy = null;

			if (!e.Cancel)
			{
				copy = ((Content)content).Clone();
				copy.ParentId = parentId;
				copy.Name = copy.Name + " (1)";

				var uow = _uowProvider.GetUnitOfWork();
				using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
				{
					SetWriter(content, userId);

					repository.AddOrUpdate(copy);
					uow.Commit();
				}
			}

			if (Copied != null)
				Copied(copy, e);

			Audit.Add(AuditTypes.Delete, "Copy Content performed by user", content.WriterId, content.Id);

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
			var e = new SendToPublishEventArgs();

			if (SendingToPublish != null)
				SendingToPublish(content, e);

			if (!e.Cancel)
			{
				// Do some stuff here..

				if (SentToPublish != null)
					SentToPublish(content, e);

				Audit.Add(AuditTypes.SendToPublish, "Send to Publish performed by user", content.WriterId, content.Id);
			}

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
			var e = new RollbackEventArgs();

			var uow = _uowProvider.GetUnitOfWork();
			using (var repository = RepositoryResolver.Current.Factory.CreateContentRepository(uow))
			{
				var content = repository.GetByVersion(id, versionId);

				if (Rollingback != null)
					Rollingback(content, e);

				if (!e.Cancel)
				{
					SetUser(content, userId);
					SetWriter(content, userId);

					repository.AddOrUpdate(content);
					uow.Commit();

					if (Rolledback != null)
						Rolledback(content, e);

					Audit.Add(AuditTypes.RollBack, "Content rollback performed by user", content.WriterId, content.Id);
				}

				return content;
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
		/// Updates a content object with the User (id), who created the content.
		/// </summary>
		/// <param name="content"><see cref="IContent"/> object to update</param>
		/// <param name="userId">Optional Id of the User</param>
		private void SetUser(IContent content, int userId)
		{
			if (userId > -1)
			{
				//If a user id was passed in we use that
				content.CreatorId = userId;
			}
			else if (UserServiceOrContext())
			{
				var profile = _httpContext == null
								  ? _userService.GetCurrentBackOfficeUser()
								  : _userService.GetCurrentBackOfficeUser(_httpContext);
				content.CreatorId = profile.Id.SafeCast<int>();
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
			else if (UserServiceOrContext())
			{
				var profile = _httpContext == null
								  ? _userService.GetCurrentBackOfficeUser()
								  : _userService.GetCurrentBackOfficeUser(_httpContext);
				content.WriterId = profile.Id.SafeCast<int>();
			}
			else
			{
				//Otherwise we default to Admin user, which should always exist (almost always)
				content.WriterId = 0;
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
		/// Occurs before Copy
		/// </summary>
		public static event EventHandler<CopyEventArgs> Copying;

		/// <summary>
		/// Occurs after Copy
		/// </summary>
		public static event EventHandler<CopyEventArgs> Copied;

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

		/// <summary>
		/// Occurs before Rollback
		/// </summary>
		public static event EventHandler<RollbackEventArgs> Rollingback;

		/// <summary>
		/// Occurs after Rollback
		/// </summary>
		public static event EventHandler<RollbackEventArgs> Rolledback;

		/// <summary>
		/// Occurs before Send to Publish
		/// </summary>
		public static event EventHandler<SendToPublishEventArgs> SendingToPublish;

		/// <summary>
		/// Occurs after Send to Publish
		/// </summary>
		public static event EventHandler<SendToPublishEventArgs> SentToPublish;
		#endregion
	}
}