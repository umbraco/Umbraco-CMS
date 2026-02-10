using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for managing member type containers (folders).
/// </summary>
/// <remarks>
///     Member type containers allow organizing member types into a hierarchical folder structure.
/// </remarks>
public interface IMemberTypeContainerService : IEntityTypeContainerService<IMemberType>
{
}
