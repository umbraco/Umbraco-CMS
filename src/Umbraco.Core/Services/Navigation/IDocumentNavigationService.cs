namespace Umbraco.Cms.Core.Services.Navigation;

public interface IDocumentNavigationService : INavigationQueryService, INavigationManagementService
{
    bool Move(Guid nodeKey, Guid? targetParentKey = null);
}
