using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

[ApiVersion("1.0")]
public class DeleteCreatedPackageController : CreatedPackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DeleteCreatedPackageController(IPackagingService packagingService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _packagingService = packagingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    ///     Deletes a package with a given id.
    /// </summary>
    /// <param name="id">The id of the package.</param>
    /// <returns>The result of the deletion.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        Attempt<PackageDefinition?, PackageOperationStatus> result =
            await _packagingService.DeleteCreatedPackageAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : PackageOperationStatusResult(result.Status);
    }
}
