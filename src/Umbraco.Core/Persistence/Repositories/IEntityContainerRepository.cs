using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="EntityContainer" /> entities (folders).
/// </summary>
public interface IEntityContainerRepository : IReadRepository<int, EntityContainer>, IWriteRepository<EntityContainer>
{
    /// <summary>
    ///     Gets a container by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the container.</param>
    /// <returns>The container if found; otherwise, <c>null</c>.</returns>
    EntityContainer? Get(Guid id);

    /// <summary>
    ///     Gets containers by name and level.
    /// </summary>
    /// <param name="name">The name of the containers.</param>
    /// <param name="level">The level of the containers in the hierarchy.</param>
    /// <returns>A collection of containers matching the criteria.</returns>
    IEnumerable<EntityContainer> Get(string name, int level);

    /// <summary>
    ///     Checks whether a container with the same name exists under the specified parent.
    /// </summary>
    /// <param name="parentKey">The unique key of the parent container.</param>
    /// <param name="name">The name to check for duplicates.</param>
    /// <returns><c>true</c> if a duplicate name exists; otherwise, <c>false</c>.</returns>
    bool HasDuplicateName(Guid parentKey, string name);

    /// <summary>
    ///     Checks whether a container with the same name exists under the specified parent.
    /// </summary>
    /// <param name="parentId">The identifier of the parent container.</param>
    /// <param name="name">The name to check for duplicates.</param>
    /// <returns><c>true</c> if a duplicate name exists; otherwise, <c>false</c>.</returns>
    bool HasDuplicateName(int parentId, string name);
}
