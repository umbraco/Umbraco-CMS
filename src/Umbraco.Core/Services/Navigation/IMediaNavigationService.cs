namespace Umbraco.Cms.Core.Services.Navigation;

public interface IMediaNavigationService : INavigationQueryService, INavigationManagementService
{
    bool Move(Guid nodeKey, Guid? targetParentKey = null);
}
