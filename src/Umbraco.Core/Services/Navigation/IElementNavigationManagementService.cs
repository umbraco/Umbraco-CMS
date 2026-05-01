namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Provides management operations for the element navigation structure.
/// </summary>
/// <remarks>
///     This interface combines <see cref="INavigationManagementService"/> and
///     <see cref="IRecycleBinNavigationManagementService"/> to provide a complete
///     set of management operations for element navigation, including adding, moving,
///     and managing elements in the recycle bin.
/// </remarks>
public interface IElementNavigationManagementService :
    INavigationManagementService,
    IRecycleBinNavigationManagementService;
