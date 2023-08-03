using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Folder;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.PartialView}/folder")]
[ApiExplorerSettings(GroupName = "Partial View")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessPartialViews)]
public class PartialViewFolderControllerBase : PathFolderManagementControllerBase<PartialViewFolderOperationStatus>
{
    private readonly IPartialViewFolderService _partialViewFolderService;

    public PartialViewFolderControllerBase(
        IUmbracoMapper mapper,
        IPartialViewFolderService partialViewFolderService)
        : base(mapper)
    {
        _partialViewFolderService = partialViewFolderService;
    }

    protected override Task<PathContainer?> GetContainerAsync(string path) => _partialViewFolderService.GetAsync(path);

    protected override Task<Attempt<PathContainer?, PartialViewFolderOperationStatus>> CreateContainerAsync(
        PathContainer container) =>
        _partialViewFolderService.CreateAsync(container);

    protected override Task<Attempt<PartialViewFolderOperationStatus>> DeleteContainerAsync(string path) =>
        _partialViewFolderService.DeleteAsync(path);

    protected override IActionResult OperationStatusResult(PartialViewFolderOperationStatus status) =>
        status switch
        {
            PartialViewFolderOperationStatus.AlreadyExists => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Folder already exists")
                .WithDetail("The folder already exists")
                .Build()),
            PartialViewFolderOperationStatus.NotEmpty => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Not empty")
                .WithDetail("The folder is not empty and can therefore not be deleted.")
                .Build()),
            PartialViewFolderOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Not found")
                .WithDetail("The specified folder was not found.")
                .Build()),
            PartialViewFolderOperationStatus.ParentNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Parent not found")
                .WithDetail("The parent folder was not found.")
                .Build()),
            PartialViewFolderOperationStatus.InvalidName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid name")
                .WithDetail("The name specified is not a valid name.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown partial view folder operation status.")
                .Build()),
        };
}
