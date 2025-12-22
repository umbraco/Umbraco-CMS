// src/Umbraco.Core/Services/IContentQueryOperationService.cs
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for content query operations (counting, filtering by type/level).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Implementation Note:</strong> Do not implement this interface directly.
/// Instead, inherit from <see cref="ContentServiceBase"/> which provides required
/// infrastructure (scoping, repository access, auditing). Direct implementation
/// without this base class will result in missing functionality.
/// </para>
/// <para>
/// This interface is part of the ContentService refactoring initiative (Phase 2).
/// It extracts query operations into a focused, testable service.
/// </para>
/// <para>
/// <strong>Versioning Policy:</strong> This interface follows additive-only changes.
/// New methods may be added with default implementations. Existing methods will not
/// be removed or have signatures changed without a 2 major version deprecation period.
/// </para>
/// <para>
/// <strong>Version History:</strong>
/// <list type="bullet">
///   <item><description>v1.0 (Phase 2): Initial interface with Count, GetByLevel, GetPagedOfType operations</description></item>
/// </list>
/// </para>
/// </remarks>
/// <since>1.0</since>
public interface IContentQueryOperationService : IService
{
    #region Count Operations

    /// <summary>
    /// Counts content items, optionally filtered by content type.
    /// </summary>
    /// <param name="contentTypeAlias">Optional content type alias to filter by. If the alias doesn't exist, returns 0.</param>
    /// <returns>The count of matching content items (includes trashed items).</returns>
    int Count(string? contentTypeAlias = null);

    /// <summary>
    /// Counts published content items, optionally filtered by content type.
    /// </summary>
    /// <param name="contentTypeAlias">Optional content type alias to filter by. If the alias doesn't exist, returns 0.</param>
    /// <returns>The count of matching published content items.</returns>
    int CountPublished(string? contentTypeAlias = null);

    /// <summary>
    /// Counts children of a parent, optionally filtered by content type.
    /// </summary>
    /// <param name="parentId">The parent content id. If the parent doesn't exist, returns 0.</param>
    /// <param name="contentTypeAlias">Optional content type alias to filter by. If the alias doesn't exist, returns 0.</param>
    /// <returns>The count of matching child content items.</returns>
    int CountChildren(int parentId, string? contentTypeAlias = null);

    /// <summary>
    /// Counts descendants of an ancestor, optionally filtered by content type.
    /// </summary>
    /// <param name="parentId">The ancestor content id. If the ancestor doesn't exist, returns 0.</param>
    /// <param name="contentTypeAlias">Optional content type alias to filter by. If the alias doesn't exist, returns 0.</param>
    /// <returns>The count of matching descendant content items.</returns>
    int CountDescendants(int parentId, string? contentTypeAlias = null);

    #endregion

    #region Hierarchy Queries

    /// <summary>
    /// Gets content items at a specific tree level.
    /// </summary>
    /// <param name="level">The tree level (1 = root children, 2 = grandchildren, etc.).</param>
    /// <returns>Content items at the specified level, excluding trashed items.</returns>
    IEnumerable<IContent> GetByLevel(int level);

    #endregion

    #region Paged Type Queries

    /// <summary>
    /// Gets paged content items of a specific content type.
    /// </summary>
    /// <param name="contentTypeId">The content type id. If the content type doesn't exist, returns empty results with totalRecords = 0.</param>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="totalRecords">Output: total number of matching records.</param>
    /// <param name="filter">Optional filter query.</param>
    /// <param name="ordering">Optional ordering (defaults to sortOrder).</param>
    /// <returns>Paged content items.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when pageIndex is negative or pageSize is less than or equal to zero.</exception>
    IEnumerable<IContent> GetPagedOfType(
        int contentTypeId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IContent>? filter = null,
        Ordering? ordering = null);

    /// <summary>
    /// Gets paged content items of multiple content types.
    /// </summary>
    /// <param name="contentTypeIds">The content type ids. If empty or containing non-existent IDs, returns empty results with totalRecords = 0.</param>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="totalRecords">Output: total number of matching records.</param>
    /// <param name="filter">Optional filter query.</param>
    /// <param name="ordering">Optional ordering (defaults to sortOrder).</param>
    /// <returns>Paged content items.</returns>
    /// <exception cref="ArgumentNullException">Thrown when contentTypeIds is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when pageIndex is negative or pageSize is less than or equal to zero.</exception>
    IEnumerable<IContent> GetPagedOfTypes(
        int[] contentTypeIds,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IContent>? filter = null,
        Ordering? ordering = null);

    #endregion
}
