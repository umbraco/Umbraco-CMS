using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

[ApiVersion("1.0")]
public class GetStatusRedirectUrlManagementController : RedirectUrlManagementControllerBase
{
    private readonly IRedirectUrlStatusPresentationFactory _redirectUrlStatusPresentationFactory;

    public GetStatusRedirectUrlManagementController(
        IRedirectUrlStatusPresentationFactory redirectUrlStatusPresentationFactory) =>
        _redirectUrlStatusPresentationFactory = redirectUrlStatusPresentationFactory;

    [MapToApiVersion("1.0")]
    [HttpGet("status")]
    [ProducesResponseType(typeof(RedirectUrlStatusResponseModel), 200)]
    public Task<ActionResult<RedirectUrlStatusResponseModel>> GetStatus(CancellationToken cancellationToken) =>
        Task.FromResult<ActionResult<RedirectUrlStatusResponseModel>>(_redirectUrlStatusPresentationFactory.CreateViewModel());
}
