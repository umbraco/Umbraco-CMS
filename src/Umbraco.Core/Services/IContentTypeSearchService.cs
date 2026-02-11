using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides search services for content types (document types).
/// </summary>
public interface IContentTypeSearchService
{
    /// <summary>
    ///     Searches for content types matching the specified criteria.
    /// </summary>
    /// <param name="query">The search query string to filter results by name or alias.</param>
    /// <param name="isElement">
    ///     Filter by element type status. <c>true</c> returns only element types,
    ///     <c>false</c> returns only non-element types, <c>null</c> returns all types.
    /// </param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <param name="skip">The number of items to skip for pagination.</param>
    /// <param name="take">The number of items to take for pagination.</param>
    /// <returns>A paged model containing the matching content types.</returns>
    Task<PagedModel<IContentType>> SearchAsync(string query, bool? isElement, CancellationToken cancellationToken, int skip = 0, int take = 100);
}
