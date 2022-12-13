using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Paging;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

public class QueryRedirectUrlManagementController : RedirectUrlManagementBaseController
{
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IRedirectUrlViewModelFactory _redirectUrlViewModelFactory;

    public QueryRedirectUrlManagementController(
        IRedirectUrlService redirectUrlService,
        IRedirectUrlViewModelFactory redirectUrlViewModelFactory)
    {
        _redirectUrlService = redirectUrlService;
        _redirectUrlViewModelFactory = redirectUrlViewModelFactory;
    }

    [HttpGet("query")]
    [ProducesResponseType(typeof(PagedViewModel<RedirectUrlViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RedirectUrlViewModel>>> Query(string query, int skip, int take)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid query",
                Detail = "No query was provided",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            });
        }

        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out long pageNumber, out int pageSize, out ProblemDetails? error) is false)
        {
            return BadRequest(error);
        }

        IEnumerable<IRedirectUrl> redirects = _redirectUrlService.SearchRedirectUrls(query, pageNumber, pageSize, out long total);

        IEnumerable<RedirectUrlViewModel> viewModels = _redirectUrlViewModelFactory.CreateMany(redirects);
        return new PagedViewModel<RedirectUrlViewModel> { Items = viewModels, Total = total };
    }
}
