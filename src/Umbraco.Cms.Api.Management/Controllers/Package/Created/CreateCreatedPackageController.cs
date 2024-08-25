using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

[ApiVersion("1.0")]
public class CreateCreatedPackageController : CreatedPackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IPackagePresentationFactory _packagePresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateCreatedPackageController(
        IPackagingService packagingService,
        IPackagePresentationFactory packagePresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _packagingService = packagingService;
        _packagePresentationFactory = packagePresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    ///     Creates a package.
    /// </summary>
    /// <param name="createPackageRequestModel">The model containing the data for a new package.</param>
    /// <returns>The created package.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreatePackageRequestModel createPackageRequestModel)
    {
        PackageDefinition packageDefinition = _packagePresentationFactory.CreatePackageDefinition(createPackageRequestModel);

        Attempt<PackageDefinition, PackageOperationStatus> result = await _packagingService.CreateCreatedPackageAsync(packageDefinition, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyCreatedPackageController>(controller => nameof(controller.ByKey), packageDefinition.PackageId)
            : PackageOperationStatusResult(result.Status);
    }
}
