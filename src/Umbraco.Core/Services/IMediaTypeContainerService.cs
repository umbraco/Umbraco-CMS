using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for managing media type containers (folders).
/// </summary>
/// <remarks>
///     Media type containers allow organizing media types into a hierarchical folder structure.
/// </remarks>
public interface IMediaTypeContainerService : IEntityTypeContainerService<IMediaType>
{
}
