using Umbraco.Cms.Core.Models.Navigation;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface INavigationRepository
{
    public Dictionary<Guid, NavigationNode> GetContentNodesByObjectType(Guid objectTypeKey);

    public Dictionary<Guid, NavigationNode> GetTrashedContentNodesByObjectType(Guid objectTypeKey);
}
