using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IEntityTypeContainerService<TTreeEntity>
    where TTreeEntity : ITreeEntity
{
    /// <summary>
    /// Gets a container
    /// </summary>
    /// <param name="id">The ID of the container to get.</param>
    /// <returns></returns>
    Task<EntityContainer?> GetAsync(Guid id);

    /// <summary>
    /// Gets containers by name and level
    /// </summary>
    /// <param name="name">The name of the containers to get.</param>
    /// <param name="level">The level in the tree of the containers to get.</param>
    /// <returns></returns>
    Task<IEnumerable<EntityContainer>> GetAsync(string name, int level);

    /// <summary>
    /// Gets all containers
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<EntityContainer>> GetAllAsync();

    /// <summary>
    /// Gets the parent container of a container
    /// </summary>
    /// <param name="container">The container whose parent container to get.</param>
    /// <returns></returns>
    Task<EntityContainer?> GetParentAsync(EntityContainer container);

    /// <summary>
    /// Gets the parent container of an entity
    /// </summary>
    /// <param name="entity">The entity whose parent container to get.</param>
    /// <returns></returns>
    Task<EntityContainer?> GetParentAsync(TTreeEntity entity);

    /// <summary>
    /// Creates a new container
    /// </summary>
    /// <param name="key">The key to assign to the newly created container (if null is specified, a random key will be assigned).</param>
    /// <param name="name">The name of the created container.</param>
    /// <param name="parentKey">The ID of the parent container to create the new container under.</param>
    /// <param name="userKey">Key of the user issuing the creation.</param>
    /// <returns></returns>
    /// <remarks>If parent key is supplied as null, the container will be created at the tree root.</remarks>
    Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> CreateAsync(Guid? key, string name, Guid? parentKey, Guid userKey);

    /// <summary>
    /// Updates an existing container
    /// </summary>
    /// <param name="key">The key of the container to update.</param>
    /// <param name="name">The name to assign to the container.</param>
    /// <param name="userKey">Key of the user issuing the update.</param>
    /// <returns></returns>
    Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> UpdateAsync(Guid key, string name, Guid userKey);

    /// <summary>
    /// Deletes a container
    /// </summary>
    /// <param name="id">The ID of the container to delete.</param>
    /// <param name="userKey">Key of the user issuing the deletion.</param>
    /// <returns></returns>
    Task<Attempt<EntityContainer?, EntityContainerOperationStatus>> DeleteAsync(Guid id, Guid userKey);
}
