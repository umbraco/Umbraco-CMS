using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

[VersionedApiBackOfficeRoute("package/created")]
[ApiExplorerSettings(GroupName = "Package")]
public class CreatedPackageControllerBase : PackageControllerBase
{
    protected IActionResult CreatedPackageNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The created package could not be found")
        .Build());

    protected IActionResult CreatedPackageFileStreamNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The created package file stream could not be found")
        .Build());


}
