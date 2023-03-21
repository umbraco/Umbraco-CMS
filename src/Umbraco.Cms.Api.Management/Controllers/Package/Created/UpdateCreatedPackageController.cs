using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

public class UpdateCreatedPackageController : CreatedPackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateCreatedPackageController(
        IPackagingService packagingService,
        IUmbracoMapper umbracoMapper,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _packagingService = packagingService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    ///     Updates a package.
    /// </summary>
    /// <param name="key">The key of the package.</param>
    /// <param name="updatePackageRequestModel">The model containing the data for updating a package.</param>
    /// <returns>The created package.</returns>
    [HttpPut("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid key, UpdatePackageRequestModel updatePackageRequestModel)
    {
        PackageDefinition? package = await _packagingService.GetCreatedPackageByKeyAsync(key);

        if (package is null)
        {
            return NotFound();
        }

        // Macros are not included!
        PackageDefinition packageDefinition = _umbracoMapper.Map(updatePackageRequestModel, package);

        Attempt<PackageDefinition, PackageOperationStatus> result = await _packagingService.UpdateCreatedPackageAsync(packageDefinition, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : PackageOperationStatusResult(result.Status);
    }
}
