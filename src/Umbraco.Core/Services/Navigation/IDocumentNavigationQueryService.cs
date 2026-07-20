namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Provides query operations for the document navigation structure.
/// </summary>
/// <remarks>
///     This interface combines <see cref="INavigationQueryService"/> and
///     <see cref="IRecycleBinNavigationQueryService"/> to provide a complete
///     set of query operations for document navigation, including traversing
///     the main structure and the recycle bin.
/// </remarks>
public interface IDocumentNavigationQueryService : INavigationQueryService, IRecycleBinNavigationQueryService
{
}
