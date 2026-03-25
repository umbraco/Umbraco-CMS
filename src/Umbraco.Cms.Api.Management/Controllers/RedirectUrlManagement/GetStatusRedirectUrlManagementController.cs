using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

/// <summary>
/// Controller for retrieving status information related to redirect URL management.
/// </summary>
[ApiVersion("1.0")]
public class GetStatusRedirectUrlManagementController : RedirectUrlManagementControllerBase
{
    private readonly IRedirectUrlStatusPresentationFactory _redirectUrlStatusPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetStatusRedirectUrlManagementController"/> class.
    /// </summary>
    /// <param name="redirectUrlStatusPresentationFactory">The factory for creating redirect URL status presentations.</param>
    public GetStatusRedirectUrlManagementController(
        IRedirectUrlStatusPresentationFactory redirectUrlStatusPresentationFactory) =>
        _redirectUrlStatusPresentationFactory = redirectUrlStatusPresentationFactory;

    /// <summary>
    /// Retrieves the current status and configuration for redirect URL management.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{RedirectUrlStatusResponseModel}"/> with the current redirect URL management status and configuration.</returns>
    [MapToApiVersion("1.0")]
    [HttpGet("status")]
    [ProducesResponseType(typeof(RedirectUrlStatusResponseModel), 200)]
    [EndpointSummary("Gets the current redirect URL management status.")]
    [EndpointDescription("Retrieves the current status and configuration for redirect URL management.")]
    public Task<ActionResult<RedirectUrlStatusResponseModel>> GetStatus(CancellationToken cancellationToken) =>
        Task.FromResult<ActionResult<RedirectUrlStatusResponseModel>>(_redirectUrlStatusPresentationFactory.CreateViewModel());
}
