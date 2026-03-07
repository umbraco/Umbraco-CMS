using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Api.Management.ViewModels.Installer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services.Installer;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Install;

    /// <summary>
    /// Controller responsible for managing the setup and installation process of Umbraco CMS via the API.
    /// </summary>
[ApiVersion("1.0")]
public class SetupInstallController : InstallControllerBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly IInstallService _installService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Install.SetupInstallController"/> class.
    /// </summary>
    /// <param name="mapper">The mapper used for object mapping within Umbraco.</param>
    /// <param name="installService">The service responsible for handling installation logic.</param>
    public SetupInstallController(
        IUmbracoMapper mapper,
        IInstallService installService)
    {
        _mapper = mapper;
        _installService = installService;
    }

    /// <summary>
    /// Initiates the initial setup and installation process for an Umbraco instance.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the installation operation.</param>
    /// <param name="installData">An <see cref="InstallRequestModel"/> containing the necessary information for installation setup.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the outcome of the installation process. Returns <c>200 OK</c> if successful, or a <see cref="ProblemDetails"/> response if a precondition is not met.
    /// </returns>
    [HttpPost("setup")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Performs installation setup.")]
    [EndpointDescription("Performs the initial setup and installation of Umbraco.")]
    public async Task<IActionResult> Setup(CancellationToken cancellationToken, InstallRequestModel installData)
    {
        InstallData data = _mapper.Map<InstallData>(installData)!;
        Attempt<InstallationResult?, InstallOperationStatus> result = await _installService.InstallAsync(data);

        return InstallOperationResult(result.Status, result.Result);
    }
}
