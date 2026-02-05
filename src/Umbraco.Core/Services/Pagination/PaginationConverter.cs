namespace Umbraco.Cms.Core.Services.Pagination;

/// <summary>
///     Provides utility methods for converting between different pagination strategies.
/// </summary>
internal static class PaginationConverter
{
    /// <summary>
    ///     Converts skip/take pagination parameters to page number and page size format.
    /// </summary>
    /// <param name="skip">The number of items to skip. Must be evenly divisible by <paramref name="take"/> when <paramref name="take"/> is non-zero.</param>
    /// <param name="take">The number of items to take per page. Must be equal to or greater than zero.</param>
    /// <param name="pageNumber">When this method returns, contains the calculated zero-based page number.</param>
    /// <param name="pageSize">When this method returns, contains the page size (same as <paramref name="take"/>).</param>
    /// <returns>
    ///     <see langword="true"/> if the conversion was successful; <see langword="false"/> if <paramref name="skip"/>
    ///     is not evenly divisible by <paramref name="take"/> (indicating the skip/take values don't align to page boundaries).
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="take"/> is less than zero.</exception>
    /// <remarks>
    ///     <para>
    ///         This method enables translation between offset-based pagination (skip/take) and page-based pagination
    ///         (page number/page size), which is useful when interfacing with APIs or data layers that use different
    ///         pagination conventions.
    ///     </para>
    ///     <para>
    ///         When <paramref name="take"/> is zero, both <paramref name="pageNumber"/> and <paramref name="pageSize"/>
    ///         will be set to zero, and the method returns <see langword="true"/>.
    ///     </para>
    /// </remarks>
    internal static bool ConvertSkipTakeToPaging(int skip, int take, out long pageNumber, out int pageSize)
    {
        if (take < 0)
        {
            throw new ArgumentException("Must be equal to or greater than zero", nameof(take));
        }

        if (take != 0 && skip % take != 0)
        {
            pageSize = 0;
            pageNumber = 0;
            return false;
        }

        pageSize = take;
        pageNumber = take == 0 ? 0 : skip / take;
        return true;
    }
}
