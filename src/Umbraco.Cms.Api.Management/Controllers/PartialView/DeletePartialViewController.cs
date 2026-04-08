using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView;

/// <summary>
/// API controller responsible for handling requests to delete partial views in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class DeletePartialViewController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IOptions<RuntimeSettings> _runtimeSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePartialViewController"/> class, responsible for handling requests to delete partial views.
    /// </summary>
    /// <param name="partialViewService">Service used to manage partial views.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="runtimeSettings">The runtime configuration settings.</param>
    [ActivatorUtilitiesConstructor]
    public DeletePartialViewController(
        IPartialViewService partialViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IOptions<RuntimeSettings> runtimeSettings)
    {
        _partialViewService = partialViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _runtimeSettings = runtimeSettings;
    }

    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public DeletePartialViewController(
        IPartialViewService partialViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            partialViewService,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<RuntimeSettings>>())
    {
    }

    [HttpDelete("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a partial view.")]
    [EndpointDescription("Deletes a partial view identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, string path)
    {
        if (_runtimeSettings.Value.Mode == RuntimeMode.Production)
        {
            return PartialViewOperationStatusResult(PartialViewOperationStatus.NotAllowedInProductionMode);
        }

        path = DecodePath(path).VirtualPathToSystemPath();
        PartialViewOperationStatus operationStatus = await _partialViewService.DeleteAsync(path, CurrentUserKey(_backOfficeSecurityAccessor));

        return operationStatus is PartialViewOperationStatus.Success
            ? Ok()
            : PartialViewOperationStatusResult(operationStatus);
    }
}
