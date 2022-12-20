using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

public class ByKeyRedirectUrlManagementController : RedirectUrlManagementBaseController
{
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IRedirectUrlViewModelFactory _redirectUrlViewModelFactory;

    public ByKeyRedirectUrlManagementController(
        IRedirectUrlService redirectUrlService,
        IRedirectUrlViewModelFactory redirectUrlViewModelFactory)
    {
        _redirectUrlService = redirectUrlService;
        _redirectUrlViewModelFactory = redirectUrlViewModelFactory;
    }

    [HttpGet("{key:guid}")]
    [ProducesResponseType(typeof(PagedViewModel<RedirectUrlViewModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<PagedViewModel<RedirectUrlViewModel>>> ByKey(Guid key, int skip, int take)
    {
        IRedirectUrl[] redirects = _redirectUrlService.GetContentRedirectUrls(key).ToArray();

        IEnumerable<RedirectUrlViewModel> viewModels = _redirectUrlViewModelFactory.CreateMany(redirects);

        return Task.FromResult<ActionResult<PagedViewModel<RedirectUrlViewModel>>>(new PagedViewModel<RedirectUrlViewModel>
        {
            Items = viewModels.Skip(skip).Take(take),
            Total = redirects.Length,
        });
    }
}
