namespace Umbraco.Cms.Core;

public static class PaginationHelper
{
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
