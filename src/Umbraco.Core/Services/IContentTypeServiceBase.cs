using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
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
        TItem Get(int id);

        /// <summary>
        /// Gets a content type.
        /// </summary>
        TItem Get(Guid key);

        /// <summary>
        /// Gets a content type.
        /// </summary>
        TItem Get(string alias);

        int Count();

        IEnumerable<TItem> GetAll(params int[] ids);

        IEnumerable<TItem> GetDescendants(int id, bool andSelf); // parent-child axis
        IEnumerable<TItem> GetComposedOf(int id); // composition axis

        IEnumerable<TItem> GetChildren(int id);
        bool HasChildren(int id);

        void Save(TItem item, int userId = Constants.Security.SuperUserId);
        void Save(IEnumerable<TItem> items, int userId = Constants.Security.SuperUserId);

        void Delete(TItem item, int userId = Constants.Security.SuperUserId);
        void Delete(IEnumerable<TItem> item, int userId = Constants.Security.SuperUserId);


        Attempt<string[]> ValidateComposition(TItem compo);

        /// <summary>
        /// Given the path of a content item, this will return true if the content item exists underneath a list view content item
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        bool HasContainerInPath(string contentPath);

        Attempt<OperationResult<OperationResultType, EntityContainer>> CreateContainer(int parentContainerId, string name, int userId = Constants.Security.SuperUserId);
        Attempt<OperationResult> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId);
        EntityContainer GetContainer(int containerId);
        EntityContainer GetContainer(Guid containerId);
        IEnumerable<EntityContainer> GetContainers(int[] containerIds);
        IEnumerable<EntityContainer> GetContainers(TItem contentType);
        IEnumerable<EntityContainer> GetContainers(string folderName, int level);
        Attempt<OperationResult> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId);
        Attempt<OperationResult<OperationResultType, EntityContainer>> RenameContainer(int id, string name, int userId = Constants.Security.SuperUserId);

        Attempt<OperationResult<MoveOperationStatusType>> Move(TItem moving, int containerId);
        Attempt<OperationResult<MoveOperationStatusType, TItem>> Copy(TItem copying, int containerId);
        TItem Copy(TItem original, string alias, string name, int parentId = -1);
        TItem Copy(TItem original, string alias, string name, TItem parent);
    }
}
