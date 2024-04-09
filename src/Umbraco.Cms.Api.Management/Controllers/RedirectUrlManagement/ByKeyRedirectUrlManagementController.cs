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
public class ByKeyRedirectUrlManagementController : RedirectUrlManagementControllerBase
{
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IRedirectUrlPresentationFactory _redirectUrlPresentationFactory;

    public ByKeyRedirectUrlManagementController(
        IRedirectUrlService redirectUrlService,
        IRedirectUrlPresentationFactory redirectUrlPresentationFactory)
    {
        _redirectUrlService = redirectUrlService;
        _redirectUrlPresentationFactory = redirectUrlPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PagedViewModel<RedirectUrlResponseModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<PagedViewModel<RedirectUrlResponseModel>>> ByKey(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 100)
    {
        IRedirectUrl[] redirects = _redirectUrlService.GetContentRedirectUrls(id).ToArray();

        IEnumerable<RedirectUrlResponseModel> viewModels = _redirectUrlPresentationFactory.CreateMany(redirects);

        return Task.FromResult<ActionResult<PagedViewModel<RedirectUrlResponseModel>>>(new PagedViewModel<RedirectUrlResponseModel>
        {
            Items = viewModels.Skip(skip).Take(take),
            Total = redirects.Length,
        });
    }
}
