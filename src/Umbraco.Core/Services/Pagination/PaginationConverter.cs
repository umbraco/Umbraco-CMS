namespace Umbraco.Cms.Core.Services.Pagination;

internal static class PaginationConverter
{
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
