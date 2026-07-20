namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Provides management operations for the document navigation structure.
/// </summary>
/// <remarks>
///     This interface combines <see cref="INavigationManagementService"/> and
///     <see cref="IRecycleBinNavigationManagementService"/> to provide a complete
///     set of management operations for document navigation, including adding, moving,
///     and managing documents in the recycle bin.
/// </remarks>
public interface IDocumentNavigationManagementService : INavigationManagementService, IRecycleBinNavigationManagementService
{
}
