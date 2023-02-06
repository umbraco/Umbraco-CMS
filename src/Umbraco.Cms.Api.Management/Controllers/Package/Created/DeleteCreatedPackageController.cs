using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

public class DeleteCreatedPackageController : CreatedPackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

    public DeleteCreatedPackageController(IPackagingService packagingService, IBackOfficeSecurityAccessor backofficeSecurityAccessor)
    {
        _packagingService = packagingService;
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
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
        PackageDefinition? package = _packagingService.GetCreatedPackageByKey(key);

        if (package is null)
        {
            return NotFound();
        }

        _packagingService.DeleteCreatedPackage(
            package.Id,
            _backofficeSecurityAccessor.BackOfficeSecurity?.GetUserId().Result ?? -1);

        return await Task.FromResult(Ok());
    }
}
