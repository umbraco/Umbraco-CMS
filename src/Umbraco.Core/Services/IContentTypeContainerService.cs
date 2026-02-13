using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides container (folder) management services for content types (document types).
/// </summary>
/// <remarks>
///     This service manages the folder structure used to organize content types
///     in the backoffice Settings section.
/// </remarks>
public interface IContentTypeContainerService : IEntityTypeContainerService<IContentType>
{
}
