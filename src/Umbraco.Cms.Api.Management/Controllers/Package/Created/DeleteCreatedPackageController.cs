using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

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
    ///     Deletes a package with a given key.
    /// </summary>
    /// <param name="key">The key of the package.</param>
    /// <returns>The result of the deletion.</returns>
    [HttpDelete("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid key)
    {
        Attempt<PackageDefinition?, PackageOperationStatus> result =
            await _packagingService.DeleteCreatedPackageAsync(key, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : PackageOperationStatusResult(result.Status);
    }
}
