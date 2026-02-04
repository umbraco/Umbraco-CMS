namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Provides management operations for the media navigation structure.
/// </summary>
/// <remarks>
///     This interface combines <see cref="INavigationManagementService"/> and
///     <see cref="IRecycleBinNavigationManagementService"/> to provide a complete
///     set of management operations for media navigation, including adding, moving,
///     and managing media items in the recycle bin.
/// </remarks>
public interface IMediaNavigationManagementService : INavigationManagementService, IRecycleBinNavigationManagementService
{
}
