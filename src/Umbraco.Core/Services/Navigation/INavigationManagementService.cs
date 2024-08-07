using Umbraco.Cms.Core.Models.Navigation;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Placeholder for sharing logic between the document, document recycle bin, media and media recycle bin navigation services
///     for managing the navigation structure.
/// </summary>
public interface INavigationManagementService
{
    Task RebuildAsync();

    bool Remove(Guid key);

    bool Add(Guid key, Guid? parentKey = null);

    NavigationNode? GetNavigationNode(Guid key);

    bool AddNavigationNode(NavigationNode node, Guid? parentKey = null);
}
