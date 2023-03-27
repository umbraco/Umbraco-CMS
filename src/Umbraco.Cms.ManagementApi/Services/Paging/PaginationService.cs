using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.ManagementApi.Services.Paging;

// TODO: remove this class once EF core is in place with proper skip/take pagination implementation
// this service is used for converting skip/take to classic pagination with page number and page size.
// it is a temporary solution that should be removed once EF core is in place, thus we'll live
// with this code being statically referenced across multiple controllers. the alternative would be
// an injectable service, but that would require a greater clean-up effort later on.
internal static class PaginationService
{
    internal static bool ConvertSkipTakeToPaging(int skip, int take, out long pageNumber, out int pageSize, out ProblemDetails? error)
    {
        if (take <= 0)
        {
            throw new ArgumentException("Must be greater than zero", nameof(take));
        }

        if (skip % take != 0)
        {
            pageSize = 0;
            pageNumber = 0;
            error = new ProblemDetails
            {
                Title = "Invalid skip/take",
                Detail = "Skip must be a multiple of take - i.e. skip = 10, take = 5",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };
            return false;
        }

        pageSize = take;
        pageNumber = skip / take;
        error = null;
        return true;
    }
}
