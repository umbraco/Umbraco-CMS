using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{
    /// <summary>
    /// Provides a common base interface for <see cref="IContentTypeBase"/>.
    /// </summary>
    public interface IContentTypeBaseService
    {
        /// <summary>
        /// Gets a content type.
        /// </summary>
        IContentTypeComposition Get(int id);
    }

    /// <summary>
    /// Provides a common base interface for <see cref="IContentTypeService"/>, <see cref="IMediaTypeService"/> and <see cref="IMemberTypeService"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public interface IContentTypeBaseService<TItem> : IContentTypeBaseService, IService
        where TItem : IContentTypeComposition
    {
        /// <summary>
        /// Gets a content type.
        /// </summary>
        new TItem Get(int id);

        /// <summary>
        /// Gets a content type.
        /// </summary>
        TItem Get(Guid key);

        /// <summary>
        /// Gets a content type.
        /// </summary>
        TItem Get(string alias);

        /// <summary>
        /// Gets the count
        /// </summary>
        int Count();

        /// <summary>
        /// Returns true or false depending on whether content nodes have been created based on the provided content type id.
        /// </summary>
        bool HasContentNodes(int id);

        /// <summary>
        /// Gets all content types by int Id.
        /// </summary>
        IEnumerable<TItem> GetAll(params int[] ids);

        /// <summary>
        /// Gets all content types by Guid Id
        /// </summary>
        IEnumerable<TItem> GetAll(IEnumerable<Guid> ids);

        /// <summary>
        /// Gets all descendants for a content type, with bool to specify if you want it to return the parent
        /// </summary>
        IEnumerable<TItem> GetDescendants(int id, bool andSelf); // parent-child axis

        /// <summary>
        /// Gets all the content types its composed of
        /// </summary>
        IEnumerable<TItem> GetComposedOf(int id); // composition axis

        /// <summary>
        /// Gets all of the children of a content type by int Id
        /// </summary>
        IEnumerable<TItem> GetChildren(int id);

        /// <summary>
        /// Gets all of the children of a content type by Guid id
        /// </summary>
        IEnumerable<TItem> GetChildren(Guid id);

        /// <summary>
        /// Returns true, false depending if a content has children or not by int Id
        /// </summary>
        bool HasChildren(int id);

        /// <summary>
        /// Returns true, false depending if a content has children or not by Guid Id
        /// </summary>
        bool HasChildren(Guid id);

        /// <summary>
        /// Saves a content type
        /// </summary>
        void Save(TItem item, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Saves multiple content types
        /// </summary>
        void Save(IEnumerable<TItem> items, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Deletes a content type
        /// </summary>
        void Delete(TItem item, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Deletes multiple content types
        /// </summary>
        void Delete(IEnumerable<TItem> item, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Attempt to validate the document type composition
        /// </summary>
        Attempt<string[]> ValidateComposition(TItem compo);

        /// <summary>
        /// Given the path of a content item, this will return true if the content item exists underneath a list view content item
        /// </summary>
        bool HasContainerInPath(string contentPath);

        /// <summary>
        /// Gets a value indicating whether there is a list view content item in the path.
        /// </summary>
        bool HasContainerInPath(params int[] ids);

        /// <summary>
        /// Attempts to create a container
        /// </summary>
        Attempt<OperationResult<OperationResultType, EntityContainer>> CreateContainer(int parentContainerId, Guid key, string name, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Attempts to save a container
        /// </summary>
        Attempt<OperationResult> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Gets the container by int Id
        /// </summary>
        EntityContainer GetContainer(int containerId);

        /// <summary>
        /// Gets the containter by Guid id
        /// </summary>
        EntityContainer GetContainer(Guid containerId);

        /// <summary>
        /// Gets multiple containers by int id
        /// </summary>
        IEnumerable<EntityContainer> GetContainers(int[] containerIds);

        /// <summary>
        /// Gets multiple containers for a contentType
        /// </summary>
        IEnumerable<EntityContainer> GetContainers(TItem contentType);

        /// <summary>
        /// Gets multiple containers by folderName and level
        /// </summary>
        IEnumerable<EntityContainer> GetContainers(string folderName, int level);

        /// <summary>
        /// Attempts to delete a container
        /// </summary>
        Attempt<OperationResult> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Attempts to rename a container
        /// </summary>
        Attempt<OperationResult<OperationResultType, EntityContainer>> RenameContainer(int id, string name, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Attempts to move a content type
        /// </summary>
        Attempt<OperationResult<MoveOperationStatusType>> Move(TItem moving, int containerId);

        /// <summary>
        /// Attempts to copy a content type
        /// </summary>
        Attempt<OperationResult<MoveOperationStatusType, TItem>> Copy(TItem copying, int containerId);

        /// <summary>
        /// Copies a root content type
        /// </summary>
        TItem Copy(TItem original, string alias, string name, int parentId = -1);

        /// <summary>
        /// Copies a child content type
        /// </summary>
        TItem Copy(TItem original, string alias, string name, TItem parent);
    }
}
