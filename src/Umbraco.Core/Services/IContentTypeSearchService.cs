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
    [Obsolete("Use the overload that accepts the allowedInLibrary parameter instead. Scheduled for removal in Umbraco 19.")]
    Task<PagedModel<IContentType>> SearchAsync(
        string query,
        bool? isElement,
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => SearchAsync(query, isElement, null, cancellationToken, skip, take);

    /// <summary>
    ///     Searches for content types matching the specified criteria.
    /// </summary>
    /// <param name="query">The search query string to filter results by name or alias. When <c>null</c>, no name/alias filtering is applied.</param>
    /// <param name="isElement">
    ///     Filter by element type status. <c>true</c> returns only element types,
    ///     <c>false</c> returns only non-element types, <c>null</c> returns all types.
    /// </param>
    /// <param name="allowedInLibrary">
    ///     Filter by library allowance status. <c>true</c> returns only types allowed in the library,
    ///     <c>false</c> returns only types not allowed in the library, <c>null</c> returns all types.
    /// </param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <param name="skip">The number of items to skip for pagination.</param>
    /// <param name="take">The number of items to take for pagination.</param>
    /// <returns>A paged model containing the matching content types.</returns>
    Task<PagedModel<IContentType>> SearchAsync(
        string? query,
        bool? isElement,
        bool? allowedInLibrary,
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100);
}
