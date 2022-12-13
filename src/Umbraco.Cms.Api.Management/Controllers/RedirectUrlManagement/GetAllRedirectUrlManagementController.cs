using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Paging;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

public class GetAllRedirectUrlManagementController : RedirectUrlManagementBaseController
{
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IRedirectUrlViewModelFactory _redirectUrlViewModelFactory;

    public GetAllRedirectUrlManagementController(
        IRedirectUrlService redirectUrlService,
        IRedirectUrlViewModelFactory redirectUrlViewModelFactory)
    {
        _redirectUrlService = redirectUrlService;
        _redirectUrlViewModelFactory = redirectUrlViewModelFactory;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedViewModel<RedirectUrlViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RedirectUrlViewModel>>> GetAll(int skip, int take)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out long pageNumber, out int pageSize, out ProblemDetails? error) is false)
        {
            return BadRequest(error);
        }

        IEnumerable<IRedirectUrl> redirects = _redirectUrlService.GetAllRedirectUrls(pageNumber, pageSize, out long total);
        IEnumerable<RedirectUrlViewModel> redirectViewModels = _redirectUrlViewModelFactory.CreateMany(redirects);

        return new PagedViewModel<RedirectUrlViewModel> { Items = redirectViewModels, Total = total };
    }
}
