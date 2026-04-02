using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides container (folder) management services for content blueprints.
/// </summary>
/// <remarks>
///     Content blueprints are templates for creating new content items. This service manages
///     the folder structure used to organize blueprints in the backoffice.
/// </remarks>
public interface IContentBlueprintContainerService : IEntityTypeContainerService<IContent>
{
}
