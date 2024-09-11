using Asp.Versioning;
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

[ApiVersion("1.0")]
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
    /// <param name="id">The id of the package.</param>
    /// <param name="updatePackageRequestModel">The model containing the data for updating a package.</param>
    /// <returns>The created package.</returns>
    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdatePackageRequestModel updatePackageRequestModel)
    {
        PackageDefinition? package = await _packagingService.GetCreatedPackageByKeyAsync(id);

        if (package is null)
        {
            return CreatedPackageNotFound();
        }

        PackageDefinition packageDefinition = _umbracoMapper.Map(updatePackageRequestModel, package);

        Attempt<PackageDefinition, PackageOperationStatus> result = await _packagingService.UpdateCreatedPackageAsync(packageDefinition, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : PackageOperationStatusResult(result.Status);
    }
}
