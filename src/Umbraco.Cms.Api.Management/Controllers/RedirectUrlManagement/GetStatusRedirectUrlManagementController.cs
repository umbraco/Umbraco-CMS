using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

public class GetStatusRedirectUrlManagementController : RedirectUrlManagementBaseController
{
    private readonly IRedirectUrlStatusViewModelFactory _redirectUrlStatusViewModelFactory;

    public GetStatusRedirectUrlManagementController(
        IRedirectUrlStatusViewModelFactory redirectUrlStatusViewModelFactory) =>
        _redirectUrlStatusViewModelFactory = redirectUrlStatusViewModelFactory;

    [HttpGet("status")]
    [ProducesResponseType(typeof(RedirectUrlStatusViewModel), 200)]
    public Task<ActionResult<RedirectUrlStatusViewModel>> GetStatus() =>
        Task.FromResult<ActionResult<RedirectUrlStatusViewModel>>(_redirectUrlStatusViewModelFactory.CreateViewModel());
}
