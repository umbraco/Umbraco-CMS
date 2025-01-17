using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

[ApiVersion("1.0")]
public class GetAllRedirectUrlManagementController : RedirectUrlManagementControllerBase
{
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IRedirectUrlPresentationFactory _redirectUrlPresentationFactory;

    public GetAllRedirectUrlManagementController(
        IRedirectUrlService redirectUrlService,
        IRedirectUrlPresentationFactory redirectUrlPresentationFactory)
    {
        _redirectUrlService = redirectUrlService;
        _redirectUrlPresentationFactory = redirectUrlPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedViewModel<RedirectUrlResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RedirectUrlResponseModel>>> GetAll(
        CancellationToken cancellationToken,
        string? filter,
        int skip = 0,
        int take = 100)
    {
        long total;
        IEnumerable<IRedirectUrl> redirects = filter is null
            ? _redirectUrlService.GetAllRedirectUrls(skip, take, out total)
            : _redirectUrlService.SearchRedirectUrls(filter, skip, take, out total);

        IEnumerable<RedirectUrlResponseModel> redirectViewModels = _redirectUrlPresentationFactory.CreateMany(redirects);
        return new PagedViewModel<RedirectUrlResponseModel> { Items = redirectViewModels, Total = total };
    }
}
