namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Placeholder for sharing logic between the document and media navigation services
///     for managing the navigation structure.
/// </summary>
public interface INavigationManagementService : IRecycleBinNavigationManagementService
{
    Task RebuildAsync();

    bool Remove(Guid key);

    bool Add(Guid key, Guid? parentKey = null);

    bool Move(Guid key, Guid? targetParentKey = null);
}
