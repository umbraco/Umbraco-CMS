namespace Umbraco.Cms.Core;

/// <summary>
///     Provides helper methods for pagination operations.
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    ///     Converts skip/take values to page number and page size.
    /// </summary>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take (page size).</param>
    /// <param name="pageNumber">When this method returns, contains the calculated page number (zero-based).</param>
    /// <param name="pageSize">When this method returns, contains the page size.</param>
    /// <exception cref="ArgumentException">Skip is not a multiple of take.</exception>
    public static void ConvertSkipTakeToPaging(int skip, int take, out long pageNumber, out int pageSize)
    {
        if (skip % take != 0)
        {
            throw new ArgumentException("Invalid skip/take, Skip must be a multiple of take - i.e. skip = 10, take = 5");
        }

        pageSize = take;
        pageNumber = skip / take;
    }
}
