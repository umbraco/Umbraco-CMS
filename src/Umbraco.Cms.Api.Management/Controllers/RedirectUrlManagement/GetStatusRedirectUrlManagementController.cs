using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

public class GetStatusRedirectUrlManagementController : RedirectUrlManagementBaseController
{
    private readonly IRedirectUrlStatusPresentationFactory _redirectUrlStatusPresentationFactory;

    public GetStatusRedirectUrlManagementController(
        IRedirectUrlStatusPresentationFactory redirectUrlStatusPresentationFactory) =>
        _redirectUrlStatusPresentationFactory = redirectUrlStatusPresentationFactory;

    [HttpGet("status")]
    [ProducesResponseType(typeof(RedirectUrlStatusResponseModel), 200)]
    public Task<ActionResult<RedirectUrlStatusResponseModel>> GetStatus() =>
        Task.FromResult<ActionResult<RedirectUrlStatusResponseModel>>(_redirectUrlStatusPresentationFactory.CreateViewModel());
}
